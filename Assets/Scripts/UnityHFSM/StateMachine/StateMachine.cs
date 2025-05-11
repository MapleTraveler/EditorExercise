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
            
            // 标识此次转换是否退出当前子状态机
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
        public IStateTimingManager ParentFsm => fsm;// ParentFsm是对fsm字段的属性（四舍五入是个Get方法）封装
        
        public bool IsRootFsm => fsm == null;
        
        
        /// <summary>
        /// 初始化一个新的 StateMachine 实例。
        /// </summary>
        /// <param name="needsExitTime">
        /// （仅适用于分层状态）
        /// 表示当此状态机作为另一个状态机的子状态时，是否可以立即退出（false），
        /// 还是需要等到显式的退出转换（true）。
        /// </param>
        /// <param name="isGhostState">
        /// 如果为 true，则该状态将变为“幽灵状态”，即状态机不会停留在此状态
        /// 一旦进入该状态，会立即尝试所有可能的出边转换，而不是等到下一次 OnLogic 调用。
        /// </param>
        /// <param name="rememberLastState">
        /// （仅适用于分层状态）
        /// 如果为 true，当重新进入此状态机时，会返回上一次的活跃状态，
        /// 而不是返回初始启动状态（startState）。
        /// </param>
        /// <inheritdoc cref="StateBase{TStateId}(bool,bool)"/>
        public StateMachine(bool needsExitTime, bool isGhostState = false,bool rememberLastState = false) 
            : base(needsExitTime: needsExitTime, isGhostState : isGhostState)
        {
            this.rememberLastState = rememberLastState;
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
        
        
        /// <summary>
        /// 通知状态机“当前活跃状态”可以安全退出。如果当前存在待处理的转换（pending transition），
        /// 状态机将立即执行这次转换。
        /// </summary>
        /// <remarks>
        /// 此信号仅在当前时刻有效，不能用于表示“之后”也可以转换 ——
        /// 它不会被缓存或记住。<br/>
        /// 它只在存在 pending transition 且状态机在 <c>OnEnter</c> 执行之后检查转换时才生效。<br/>
        /// 所以如果在 <c>OnEnter</c> 中调用本方法，将不会产生任何效果。
        /// </remarks>
        public void StateCanExit()
        {
            if(!pendingTransition.isPending)
                return;
            ITransitionListener listener = pendingTransition.listener;
            if (pendingTransition.isExitTransition)
            {
                pendingTransition = default;
                listener?.BeforeTransition();
                PerformVerticalTransition();
                listener?.AfterTransition();
            }
            else
            {
                TStateId state =  pendingTransition.targetState;
                
                // 当待切换的目标状态是一个幽灵状态（ghost state）时，
                // ChangeState() 会立即尝试该状态的所有出边转移（outgoing transitions），
                // 这可能会在过程中修改 pendingTransition。
                // 因此，我们需要先将 pendingTransition 清空（default），而不是在调用 ChangeState() 之后清空，
                // 否则新的、有效的 pendingTransition 可能会被错误地覆盖掉。
                pendingTransition = default;
                ChangeState(state,listener);// 这句内部可能再次设置 pendingTransition！
            }
        }
        

        /// <summary>
        /// 立即切换到目标状态。
        /// </summary>
        /// <param name="targetState">目标状态的名称 / 标识符。</param>
        /// <param name="listener">可选：状态切换前后触发回调的监听器。</param>
        private void ChangeState(TStateId targetState, ITransitionListener listener = null)
        {
            listener?.BeforeTransition();
            activeState?.OnExit();
            
            StateBundle bundle;
            if (!stateBundlesByName.TryGetValue(targetState, out bundle) || bundle == null)
            {
                //TODO:打状态未找到报错   
                throw UnityHFSM.Exceptions.Common.StateNotFound(this, name.ToString(), context: "Switching states");
            }
            
            activeTransitions = bundle.transitions ?? noTransitions;
            activeTriggerTransitions = bundle.triggerToTransitions ?? noTriggerTransitions;
            
            activeState = bundle.state;//此处状态字段切换完成
            activeState.OnEnter();
            // 处理该状态的全部 Transitions 和 TriggerTransitions 的 OnEnter 事件
            for (int i = 0, count = activeTransitions.Count;i < count; i++)
            {
                activeTransitions[i].OnEnter();
            }

            foreach (List<TransitionBase<TStateId>> transitions in activeTriggerTransitions.Values)
            {
                for (int i = 0, count = transitions.Count;i < count; i++)
                {
                    transitions[i].OnEnter();
                }
            }
            
            listener?.AfterTransition();
            
            // 状态切换的回调函数
            OnStateChanged?.Invoke(activeState);

            if (activeState.isGhostState)
            {
                TryAllDirectTransitions();
            }
        }
        
        /// <summary>
        /// 向父状态机发出信号，表明当前状态机已准备好退出，
        /// 从而允许父状态机进行状态转换。
        /// </summary>
        private void PerformVerticalTransition()// 状态机发起的状态转换请求，状态发生的退出请求为 OnExitRequest() 退出状态而非状态机
        {
            fsm?.StateCanExit();
        }
        
        
        /// <summary>
        /// 请求状态切换，且会考虑当前状态的 <c>needsExitTime</c> 属性。
        /// </summary>
        /// <param name="targetState">目标状态的名称或标识符。</param>
        /// <param name="forceInstantly">
        /// 如果为 true，将忽略当前状态的 <c>needsExitTime</c> 要求，强制立即切换状态。
        /// </param>
        /// <param name="listener">可选：切换前后触发回调的监听器对象。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RequestStateChange(
            TStateId targetState,
            bool forceInstantly = false,
            ITransitionListener listener = null)
        {
            if (!activeState.needsExitTime || forceInstantly)
            {
                pendingTransition = default;
                ChangeState(targetState,listener);
            }
            else
            {
                pendingTransition.SetToState(targetState, listener);
                activeState.OnExitRequest();
                // 如果当前状态可以退出，它将调用：
                // -> state.fsm.StateCanExit()，接着会调用：
                // -> fsm.ChangeState(...)

            }
        }

        
        /// <summary>
        /// 请求一次“垂直跳转”，允许当前状态机退出，从而使其父状态机能够进行状态切换。
        /// 该方法会遵循当前状态的 <c>needsExitTime</c> 属性（即是否需要等待状态退出）。
        /// </summary>
        /// <param name="forceInstantly">
        /// 如果为 true，将忽略 <c>needsExitTime</c>，强制立即退出当前状态。
        /// </param>
        /// <param name="listener">可选：状态切换前后调用的回调监听器。</param>
        public void RequestExit(bool forceInstantly = false, ITransitionListener listener = null)
        {
            if (!activeState.needsExitTime || forceInstantly)
            {
                pendingTransition.Clear();
                listener?.BeforeTransition();
                PerformVerticalTransition();
                listener?.AfterTransition();
            }
            else
            {
                pendingTransition.SetToExit(listener);
                activeState.OnExitRequest();
            }
        }

        
        
        private bool TryTransition(TransitionBase<TStateId> transition)
        {
            if (transition.isExitTransition)
            {
                /*
                你现在要离职（退出状态），
                → 得确保你老板（fsm）还在（fsm != null），
                → 而且他真的准备安排新人接替（HasPendingTransition），
                → 并且你符合离职条件（ShouldTransition）；
                否则你不能走。
                */
                if (fsm == null || !fsm.HasPendingTransition || !transition.ShouldTransition())
                {
                    return false;
                }
                RequestExit(transition.forceInstantly,transition as ITransitionListener);
            }
            else
            {
                if (!transition.ShouldTransition())
                    return false;
                RequestStateChange(transition.to,transition.forceInstantly,transition as ITransitionListener);
            }

            return true;
        }
        
        
        
        /// <summary>
        /// 尝试执行“普通”状态转换（即从当前状态出发的直接转换）。
        /// </summary>
        /// <returns>如果发生了状态转换，返回 true；否则返回 false。</returns>
        private bool TryAllDirectTransitions()
        {
            for (int i = 0, count = activeTransitions.Count; i < count; i++)
            {
                TransitionBase<TStateId> transition = activeTransitions[i];
                if (TryTransition(transition))
                    return true;
            }

            return false;
        }
        public void Trigger(TEventNameType eventName)
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

        
        
        
        
    }
}
