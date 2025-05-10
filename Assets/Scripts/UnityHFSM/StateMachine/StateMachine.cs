using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityHFSM
{
    /// <summary>
    /// 状态机的主类。它可以作为另一个状态机的子状态，从而构建出分层状态机（Hierarchical State Machine）。
    /// </summary>
    /// <typeparam name="TOwnId"></typeparam>
    /// <typeparam name="TStateId"></typeparam>
    /// <typeparam name="TEventNameType"></typeparam>
    public class StateMachine<TOwnId,TStateId,TEventNameType> : 
        StateBase<TOwnId>,
        ITriggerable<TEventNameType>,
        IStateMachine<TStateId>,
        IActionable<TEventNameType>
    {
        /// <summary>
        /// 一个状态的打包结构，包含该状态本身、从该状态出发的普通转移和触发器转移。
        /// 这样设计可以只进行一次 Dictionary 查找来获取三类信息，
        /// => 提升性能。
        /// </summary>
        private class StateBundle
        {
            // 默认情况下，这些字段全部为 null，只有在实际需要时才会被赋值。
            // => 延迟初始化（Lazy Evaluation） => 仅使用部分功能时更加节省内存
            public StateBase<TStateId> state;
            public List<TransitionBase<TStateId>> transitions;
            public Dictionary<TEventNameType,List<TransitionBase<TStateId>>> triggerToTransitions;

            public void AddTransition(TransitionBase<TStateId> trans)
            {
                transitions ??= new List<TransitionBase<TStateId>>();
                transitions.Add(trans);
            }

            public void AddTriggerTransition(TEventNameType trigger, TransitionBase<TStateId> trans)
            {
                triggerToTransitions ??= new Dictionary<TEventNameType, List<TransitionBase<TStateId>>>();
                List<TransitionBase<TStateId>> newTransitionsOfTrigger;

                if (!triggerToTransitions.TryGetValue(trigger, out newTransitionsOfTrigger))
                {
                    newTransitionsOfTrigger = new List<TransitionBase<TStateId>>();
                    triggerToTransitions.Add(trigger, newTransitionsOfTrigger);
                }
                newTransitionsOfTrigger.Add(trans);
            }
        }
        
        /// <summary>
        /// 表示一个延迟执行的状态转移（pending transition）。
        /// </summary>
        /// <remarks>
        /// 此结构体是可变的（mutable），其方法会修改自身的字段状态。
        /// 虽然“可变结构体”通常被认为是不推荐的（"mutable structs are evil"），
        /// 但这种设计在此处显著提升了性能。
        /// </remarks>
        private struct PendingTransition
        {
            // 以下字段的排列方式经过优化，旨在最小化该结构体的内存占用，
            // 特别是当 TStateId 为较小类型时（参考结构体自动顺序布局）。
            
            // 可选字段（可能为 null），在转换成功后用于调用回调方法。
            public ITransitionListener listener;
            
            public TStateId targetState;

            
            // 由于 TStateId 可能是值类型（非可空类型），
            // 需要额外的字段 isPending 来标记是否已设置待定转换。
            public bool isPending;
            
            public bool isExitTransition;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                // 只需要将 isPending 设置为 false 即可，
                // 因为在 isPending 为 false 时，其他字段不会被访问。
                this.isPending = false;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetToExit(ITransitionListener listener = null)
            {
                this.listener = listener;
                this.isExitTransition = true;
                this.isPending = true;
                // 设置为退出类型的待定转换，不需要设置 targetState

            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetToState(TStateId targetState, ITransitionListener listener = null)
            {
                this.targetState = targetState;
                this.listener = listener;
                this.isExitTransition = false;
                this.isPending = true;
                // 设置为切换到指定状态的待定转换。
            }
        }

        
        // 一个缓存的空列表，用于默认赋值，提升代码可读性并减少 GC 生成。
        private static readonly List<TransitionBase<TStateId>> noTransitions 
            = new List<TransitionBase<TStateId>>(0);
        private static readonly Dictionary<TEventNameType,List<TransitionBase<TStateId>>> noTriggerTransitions
            = new Dictionary<TEventNameType, List<TransitionBase<TStateId>>>();
        
        
        /// <summary>
        /// 当状态机的活动状态发生变化时触发的事件。
        /// </summary>
        /// <remarks>
        /// 会在状态机第一次进入初始状态，以及每次状态跳转后触发。
        /// 但注意：不会在状态机退出时触发。
        /// </remarks>
        public event Action<StateBase<TStateId>> OnStateChanged;  //每次切状态都会执行的事件集合
        
        // 保存状态机的起始状态和是否设置标志。
        private (TStateId state, bool hasState) startState = (default, false);
        
        // 当前待执行的状态转移
        private PendingTransition pendingTransition = default;
        
        // TODO:是否在重新进入状态机时回到上次离开时的状态，而不是 startState?/未来支持状态回滚？
        private bool rememberLastState = false;
        
        
        // 状态的主存储结构：状态名 => 状态本体+所有相关转移（StateBundle）
        private readonly Dictionary<TStateId,StateBundle> stateBundlesByName
            = new Dictionary<TStateId,StateBundle>();
        //当前的状态的显式字段
        private StateBase<TStateId> activeState = null; 
        private List<TransitionBase<TStateId>> activeTransitions = noTransitions;

        private Dictionary<TEventNameType, List<TransitionBase<TStateId>>> activeTriggerTransitions 
            = noTriggerTransitions;
        
        private readonly List<TransitionBase<TStateId>> transitionsFromAny
            = new List<TransitionBase<TStateId>>();
        private readonly List<TransitionBase<TStateId>> triggerTransitionsFromAny
            = new List<TransitionBase<TStateId>>();

        public StateBase<TStateId> ActiveState
        {
            get
            {
                EnsureIsInitializedFor("正在尝试获取当前激活状态");
                return activeState;
            } 
            
        }

        // TODO:为什么要这样获取，而不是直接获取结构字段，为什么这样设计
        // Answer: 避免外部直接访问结构体导致结构体复制带来的隐藏错误和性能开销
        public TStateId ActiveStateName => ActiveState.name;
        public TStateId PendingStateName => pendingTransition.targetState;
        public StateBase<TStateId> PendingState => GetState(PendingStateName);
        public bool HasPendingTransition => pendingTransition.isPending;
        public IStateTimingManager ParentFsm => fsm;
        
        public bool IsRootFsm => fsm == null;
        
        public StateMachine(bool needsExitTime, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
        }

        public void Trigger(TEventNameType eventName)
        {
            throw new NotImplementedException();
        }

        public void StateCanExit()
        {
            throw new NotImplementedException();
        }
        
        
        public StateBase<TStateId> GetState(TStateId name)
        {
            throw new NotImplementedException();
        }

        public void OnAction(TEventNameType eventName)
        {
            throw new NotImplementedException();
        }

        public void OnAction<TData>(TEventNameType eventName, TData data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 如果状态机尚未初始化，则抛出异常。
        /// </summary>
        /// <param name="context">
        /// 操作上下文描述，用于提示当前执行哪个操作时发现状态机尚未初始化。
        /// </param>
        private void EnsureIsInitializedFor(string context)
        {
            if (activeState == null)
            {
                throw UnityHFSM.Exceptions.Common.NonInitialized(this, context);
            }
        }
        
        
        
    }
}
