using System;
using System.Collections.Generic;
using System.Linq;
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
        
        // 是否在重新进入状态机时回到上次离开时的状态，而不是 startState
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
        private readonly Dictionary<TEventNameType,List<TransitionBase<TStateId>>> triggerTransitionsFromAny
            = new ();

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
        /// <summary>
        /// 尝试所有“全局”状态转移（即可以从任意状态触发的转移）。
        /// </summary>
        /// <returns>如果发生了转移，返回 true；否则返回 false。</returns>
        private bool TryAllTriggerTransitions()
        {
            for (int i = 0,count = transitionsFromAny.Count; i < count; i++)
            {
                TransitionBase<TStateId> transition = transitionsFromAny[i];
                
                // 如果目标状态已经是当前激活状态，则不执行状态转换。
                if(EqualityComparer<TStateId>.Default.Equals(transition.to, activeState.name))
                    continue;
                if(TryTransition(transition))
                    return true;
            }
            return false;
            
        }

        /// <summary>
        /// 如果这是根状态机，则调用 <c>OnEnter</c> 方法，从而初始化状态机。
        /// </summary>
        public override void Init()
        {
            if(!IsRootFsm) return;
            
            OnEnter();
        }

        public override void OnEnter()
        {
            if (!startState.hasState)
            {
                throw UnityHFSM.Exceptions.Common.MissingStartState(this, context: "运行状态机的 OnEnter 时");
            }
            // 清除上一次运行中可能遗留下的待定转换
            pendingTransition.Clear();
            
            ChangeState(startState.state);

            // 调用“从任意状态出发”的通用转换的 OnEnter 方法
            for (int i = 0, count = transitionsFromAny.Count; i < count; i++)
            {
                transitionsFromAny[i].OnEnter();
            }
            // 调用“从任意状态出发”的触发器转换的 OnEnter 方法
            foreach (List<TransitionBase<TStateId>> transitions in triggerTransitionsFromAny.Values)
            {
                for (int i = 0, count = transitions.Count; i < count; i++)
                {
                    transitions[i].OnEnter();
                }
            }
        }

        /// <summary>
        /// 执行一帧逻辑更新：
        /// - 尝试执行一次状态转换（最多一次）（优先触发器转换，其次是直接转换）。
        /// - 若状态转换发生，则在转换之后执行当前状态的 OnLogic() 方法。
        /// - 若未发生转换，则直接执行当前状态的 OnLogic()。
        /// </summary>
        public override void OnLogic()
        {
            EnsureIsInitializedFor("Running OnLogic");

            if (TryAllTriggerTransitions())
                goto runOnLogic;
            if (TryAllDirectTransitions())
                goto runOnLogic;
            
            runOnLogic:
            activeState?.OnLogic();
        }
        
        /// <summary>
        /// 当状态机被退出时调用。执行当前活动状态的 OnExit，并重置活动状态。
        /// </summary>
        public override void OnExit()
        {
            if (activeState == null)
                return;
            // 如果启用了“记住上次状态”的选项
            if (rememberLastState)
            {
                // 将当前活动状态保存为下一次进入状态机时的起始状态
                startState = (activeState.name, true);
            }
            
            activeState.OnExit();
            // 通过将 activeState 置为 null，可以防止在状态机再次进入并切换到起始状态时，
            // 当前状态的 OnExit 方法被重复调用。
            // 否则再次进入状态机时，会在 OnEnter 中调用 ChangeState(startState)，又再次触发该状态的 OnExit()，逻辑出错。
            activeState = null;
        }

        public override void OnExitRequest()
        {
            if(activeState.needsExitTime)
                activeState.OnExitRequest();
        }

        /// <summary>
        /// 定义状态机的起始状态（首次进入状态机时将进入该状态）。
        /// </summary>
        /// <param name="stateName">起始状态的名称或标识符。</param>
        public void SetStartState(TStateId stateName)
        {
            startState = (stateName,false);
        }

        
        /// <summary>
        /// 获取与指定 <c>stateName</c> 状态标识符对应的 StateBundle（状态包）
        /// 如果已存在该状态的 StateBundle，则直接返回；
        /// 如果尚不存在，则会新建一个 StateBundle，并将其添加到 Dictionary 中，
        /// 然后返回新建的实例。
        /// </summary>
        private StateBundle GetOrCreateStateBundle(TStateId stateName)
        {
            StateBundle stateBundle;

            if (!stateBundlesByName.TryGetValue(stateName, out stateBundle))
            {
                stateBundle = new StateBundle();
                stateBundlesByName.Add(stateName, stateBundle);
            }
            return stateBundle;
        }

        /// <summary>
        /// 向状态机中添加一个新的节点 / 状态。
        /// </summary>
        /// <param name="stateName">新状态的名称 / 标识符。</param>
        /// <param name="state">新的状态实例，
        ///     例如 <see cref="State"/>、<see cref="CoState"/>、或<see cref="StateMachine"/>。</param>
        public void AddState(TStateId stateName, StateBase<TStateId> state)
        {
            state.fsm = this;
            state.name = stateName;
            state.Init();
            
            StateBundle stateBundle = GetOrCreateStateBundle(stateName);
            stateBundle.state = state;

            if (stateBundlesByName.Count == 1 && !startState.hasState)
            {
                SetStartState(stateName);
            }
        }

        /// <summary>
        /// 初始化一个状态转换（Transition）：
        /// - 设置该转换所属的状态机（fsm）
        /// - 调用转换的初始化方法（Init）
        /// </summary>
        /// <param name="transition">需要初始化的状态转换对象。</param>
        private void InitTransition(TransitionBase<TStateId> transition)
        {
            transition.fsm = this;
            transition.Init();
        }
        
        /// <summary>
        /// 添加一个状态之间的普通状态转换（Transition）。
        /// </summary>
        /// <param name="transition"></param>
        private void AddTransition(TransitionBase<TStateId> transition)
        {
            InitTransition(transition);
            StateBundle stateBundle = GetOrCreateStateBundle(transition.from);
            stateBundle.AddTransition(transition);
        }
        
        
        /// <summary>
        /// 添加一个“全局转移”
        /// 该转换可以从任意状态触发，无需指定具体的起始状态（from）。
        /// </summary>
        /// <param name="transition">
        /// 转换实例；其中的 from 字段可以留空（无实际意义）。
        ///</param>
        public void AddTransitionFromAny(TransitionBase<TStateId> transition)
        {
            InitTransition(transition);

            transitionsFromAny.Add(transition);
        }
        
        /// <summary>
        /// 添加一个“带触发器的转换”：只有在指定的 trigger 被激活时才会检查该转换是否发生。
        /// </summary>
        /// <param name="trigger">触发器的名称或标识符。</param>
        /// <param name="transition">
        /// 转换实例，例如 <see cref="Transition"/>、<see cref="TransitionAfter"/> 等。
        /// </param>
        public void AddTriggerTransition(TEventNameType trigger, TransitionBase<TStateId> transition)
        {
            InitTransition(transition);

            StateBundle bundle = GetOrCreateStateBundle(transition.from);
            bundle.AddTriggerTransition(trigger, transition);
        }


        /// <summary>
        /// 添加一个“全局触发器转换”：
        /// 该转换可以从任何状态出发，但只会在指定的 trigger 被激活时才检查是否可以发生。
        /// </summary>
        /// <param name="trigger">触发器的名称或标识符。</param>
        /// <param name="transition">
        /// 转换实例；其 from 字段可以留空，因为在此上下文中没有实际意义。
        /// </param>
        public void AddTriggerTransitionFromAny(TEventNameType trigger, TransitionBase<TStateId> transition)
        {
            InitTransition(transition);
            
            List<TransitionBase<TStateId>> transitionsOfTrigger;

            if (!triggerTransitionsFromAny.TryGetValue(trigger, out transitionsOfTrigger))
            {
                transitionsOfTrigger = new List<TransitionBase<TStateId>>();
                triggerTransitionsFromAny.Add(trigger, transitionsOfTrigger);
            }
            
            transitionsOfTrigger.Add(transition);
        }


        /// <summary>
        /// 添加两个双向转换：
        /// 如果传入的 transition 的条件为 true，则从 "from" 状态转换到 "to" 状态；
        /// 否则会执行反向转换，即从 "to" 状态回到 "from" 状态。
        /// </summary>
        /// <remarks>
        /// 内部会使用同一个 transition 实例，并通过包装成 <see cref="ReverseTransition"/> 来实现反向转换。
        /// 在反向转换过程中，<c>afterTransition</c> 回调会在转换前被调用，
        /// 而 <c>BeforeTransition</c> 回调会在转换之后调用。
        /// 如果不希望这种行为，可以通过创建两个独立的 transition 实例来自定义行为。
        /// </remarks>
        public void AddTwoWayTransition(TransitionBase<TStateId> transition)
        {
            
            InitTransition(transition);
            AddTransition(transition);

            ReverseTransition<TStateId> reverse = new ReverseTransition<TStateId>(transition, false);
            InitTransition(reverse);
            AddTransition(reverse);
        }
        
        /// <summary>
        /// 添加两个由同一个触发器激活的转移：
        /// - 如果传入的转移条件成立，将从 "from" 状态切换到 "to" 状态；
        /// - 否则，将执行相反方向的切换，即从 "to" 状态切换回 "from"。
        /// </summary>
        /// <remarks>
        /// 内部会使用 <see cref="ReverseTransition"/> 包装传入的转移，从而创建反向版本。
        /// 注意：反向转移中，<c>afterTransition</c> 回调会在切换前调用，
        /// 而 <c>BeforeTransition</c> 回调会在切换后调用，这与正向逻辑相反。
        /// 如果不希望出现此行为，请手动创建两个独立的 Transition 实例替代本方法。
        /// </remarks>
        public void AddTwoWayTriggerTransition(TEventNameType trigger, TransitionBase<TStateId> transition)
        {
            InitTransition(transition);
            AddTriggerTransition(trigger, transition);

            ReverseTransition<TStateId> reverse = new ReverseTransition<TStateId>(transition, false);
            InitTransition(reverse);
            AddTriggerTransition(trigger, reverse);
        }
        
        /// <summary>
        /// 添加一个退出转换（Exit Transition），从特定状态发起，
        /// 表示该状态是状态机的出口，允许当前状态机退出、
        /// 并让其父状态机继续进行状态转换。
        /// 只有当父状态机存在待处理的转换（pending transition）时才会检查此转换。
        /// </summary>
        /// <param name="transition">
        /// 转换实例。其中的 "to" 字段在该上下文中无意义，可留空。
        /// </param>
        public void AddExitTransition(TransitionBase<TStateId> transition)
        {
            transition.isExitTransition = true;
            AddTransition(transition);
        }
        
        /// <summary>
        /// 添加一个基于触发器（Trigger）触发的退出转换，
        /// 从某一特定状态触发，允许当前状态机退出，
        /// 并让其父状态机继续进行状态切换。
        /// 仅在指定触发器激活且父状态机存在 pending transition 时生效。
        /// </summary>
        /// <param name="trigger">触发器的标识符</param>
        /// <param name="transition">
        /// 转换实例。"to" 字段无意义，可忽略。
        /// </param>
        public void AddExitTriggerTransition(TEventNameType trigger, TransitionBase<TStateId> transition)
        {
            transition.isExitTransition = true;
            AddTriggerTransition(trigger, transition);
        }
        
        /// <summary>
        /// 添加一个“从任意状态发起”的退出转换（Exit Transition），
        /// 用于标记状态机的出口，使其可以退出并让父状态机继续转换。
        /// 仅当父状态机存在待处理转换时才会检查此转换。
        /// </summary>
        /// <param name="transition">
        /// 转换实例。"from" 和 "to" 字段在该上下文中无意义，可留空。
        /// </param>
        public void AddExitTransitionFromAny(TransitionBase<TStateId> transition)
        {
            transition.isExitTransition = true;
            AddTransitionFromAny(transition);
        }


        /// <summary>
        /// 添加一个从任意状态出发、基于触发器激活的退出转换，
        /// 当指定触发器激活时允许状态机退出，并让父状态机继续切换状态。
        /// 该转换只会在父状态机存在待处理转换时生效。
        /// </summary>
        /// <param name="trigger">触发器标识符</param>
        /// <param name="transition">
        /// 转换实例。"from" 和 "to" 字段无意义，可忽略。
        /// </param>
        public void AddExitTriggerTransitionFromAny(TEventNameType trigger, TransitionBase<TStateId> transition)
        {
            transition.isExitTransition = true;
            AddTriggerTransitionFromAny(trigger, transition);
        }
        
        

        /// <summary>
        /// 激活指定的触发器（trigger），
        /// 检查所有与该触发器相关的触发器转换（trigger transitions），
        /// 判断是否应该执行状态转换。
        /// </summary>
        /// <param name="eventName">触发器的名称 / 标识符。</param>
        /// <returns>如果发生了状态转换则返回 true，否则返回 false。</returns>
        public bool TryTrigger(TEventNameType eventName)
        {
            EnsureIsInitializedFor("Checking all trigger transitions of the active state");
            
            List<TransitionBase<TStateId>> triggerTransitions;

            if (triggerTransitionsFromAny.TryGetValue(eventName, out triggerTransitions))
            {
                for (int i = 0, count = triggerTransitions.Count; i < count; i++)
                {
                    TransitionBase<TStateId> transition = triggerTransitions[i];
                    if(EqualityComparer<TStateId>.Default.Equals(transition.to,activeState.name))
                        continue;
                    
                    if(TryTransition(transition))
                        return true;
                }
            }
            
            if(activeTriggerTransitions.TryGetValue(eventName, out triggerTransitions))
            {
                for (int i = 0, count = triggerTransitions.Count; i < count; i++)
                {
                    TransitionBase<TStateId> transition = triggerTransitions[i];

                    if (TryTransition(transition))
                        return true;
                }   
            }
            
            return false;
        }

        
        /// <summary>
        /// 在整个分层状态机中激活指定的触发器（trigger），
        /// 会从当前状态机开始，依次检查所有活跃状态中相关的触发器转换，
        /// 以判断是否应当发生状态转换。
        /// </summary>
        /// <param name="eventName">触发器的名称或标识符。</param>
        public void Trigger(TEventNameType eventName)
        {
            // 如果当前状态机已经根据该 trigger 执行了一次转换，
            // 就不再继续向更深层次的子状态机传播此 trigger。
            if(TryTrigger(eventName)) return;
            
            // 如果当前激活状态本身是一个状态机（支持 ITriggerable 接口），则向其继续传递触发器
            (activeState as ITriggerable<TEventNameType>)?.Trigger(eventName);
        }

        /// <summary>
        /// 仅在当前状态机内部激活指定的触发器，不会传播给子状态机。
        /// </summary>
        /// <param name="eventName">触发器的名称或标识符。</param>
        public void TryTriggerLocally(TEventNameType eventName)
        {
            TryTrigger(eventName);
        }
        
        
        

        /// <summary>
        /// 在当前活跃状态上执行一个无参动作。
        /// </summary>
        /// <param name="eventName">动作的名称。</param>
        public void OnAction(TEventNameType eventName)
        {
            EnsureIsInitializedFor("运行当前状态的 OnAction 动作");
            (activeState as IActionable<TEventNameType>)?.OnAction(eventName);
        }

        /// <summary>
        /// 在当前活跃状态上执行一个带参数的动作。
        /// </summary>
        /// <param name="eventName">动作的名称。</param>
        /// <param name="data">附带的数据参数。</param>
        /// <typeparam name="TData">
        /// 数据参数的类型。必须与通过 <c>AddAction&lt;T&gt;(...)</c> 添加动作时的数据类型一致。
        /// </typeparam>
        public void OnAction<TData>(TEventNameType eventName, TData data)
        {
            EnsureIsInitializedFor("运行当前状态的带参 OnAction 动作");
            (activeState as IActionable<TEventNameType>)?.OnAction<TData>(eventName, data);
        }
        
        

        public StateBase<TStateId> GetState(TStateId stateName)
        {
            StateBundle stateBundle;
            if (!stateBundlesByName.TryGetValue(stateName, out stateBundle) || stateBundle == null)
            {
                throw UnityHFSM.Exceptions.Common.StateNotFound(this, name.ToString(), context: "Getting a state");
            }
            
            return stateBundle.state;
        }

        /// <summary>
        /// 仅适用于使用 string 类型的状态机：
        /// 返回指定名称的子状态机实例。
        /// 当你使用字符串作为状态标识符进行分层状态机开发时，这是一种快捷访问方式。
        /// </summary>
        public StateMachine<string, string, string> this [TStateId stateName]// 只读的索引器属性
        {
            get
            {
                StateBase<TStateId> state = GetState(stateName);
                StateMachine<string, string, string> subFsm = state as StateMachine<string, string, string>;

                if (subFsm == null)
                {
                    UnityHFSM.Exceptions.Common.QuickIndexerMisusedForGettingState(this, stateName.ToString());
                }

                return subFsm;
            }
        }

        public override string GetActiveHierarchyPath()
        {
            if (activeState == null)
            {
                // 当状态机当前没有激活状态时，层级路径为空字符串。
                return "";
            }
            
            return $"{name}/{activeState.GetActiveHierarchyPath()}";
        }

        /// <summary>返回当前状态机中所有已定义状态的名称列表。</summary>
        /// <remarks>注意：该操作代价较高（需要遍历整个状态字典）。</remarks>
        public IReadOnlyList<TStateId> GetAllStateNames()
        {
            return stateBundlesByName.Values
                .Where(bundle => bundle.state != null)
                .Select(bundle => bundle.state.name)
                .ToArray();
        }
        
        /// <summary>返回当前状态机中所有已定义的状态对象。</summary>
        /// <remarks>注意：该操作代价较高（需要遍历整个状态字典）。</remarks>
        public IReadOnlyList<StateBase<TStateId>> GetAllStates()
        {
            return stateBundlesByName.Values
                .Where(bundle => bundle.state != null)
                .Select(bundle => bundle.state)
                .ToArray();
        }

        public TStateId GetStartName()
        {
            if (!startState.hasState)
            {
                throw UnityHFSM.Exceptions.Common.MissingStartState(
                    this,
                    context: "Getting the start state",
                    solution: "确保调用 fsm.AddState(...) 之后再使用 GetStartStateName()。");
            }
            return startState.state;
        }

        /// <summary>返回所有“普通状态转移”的集合。</summary>
        /// <remarks>注意：该操作代价较高。</remarks>
        public IReadOnlyList<TransitionBase<TStateId>> GetAllTransitions()
        {
            return stateBundlesByName.Values
                .Where(bundle => bundle.transitions != null)
                .SelectMany(bundle => bundle.transitions)
                .ToArray();
        }

        public IReadOnlyList<TransitionBase<TStateId>> GetAllTransitionsFromAny()
        {
            return transitionsFromAny.ToArray();// ToArray也是一种拷贝
        }

        /// <summary>返回所有已添加的触发器转换，并按触发事件分组。</summary>
        /// <remarks>注意：这是一个开销较大的操作。</remarks>
        public IReadOnlyDictionary<TEventNameType, IReadOnlyList<TransitionBase<TStateId>>> GetAllTriggerTransitions()
        {
            var transitionsByEventName = new Dictionary<TEventNameType, List<TransitionBase<TStateId>>>();

            foreach (var bundle in stateBundlesByName.Values)
            {
                if(bundle.triggerToTransitions == null)
                    continue;

                foreach ((TEventNameType trigger, List<TransitionBase<TStateId>> transitions) in bundle.triggerToTransitions)
                {
                    if (!transitionsByEventName.TryGetValue(trigger,
                            out List<TransitionBase<TStateId>> transitionsForEvent))
                    {
                        transitionsForEvent = new List<TransitionBase<TStateId>>();
                        transitionsByEventName.Add(trigger, transitionsForEvent);
                    }
                    
                    transitionsForEvent.AddRange(transitions);
                }
            }

            var immutableCopy = new Dictionary<TEventNameType, IReadOnlyList<TransitionBase<TStateId>>>();
            foreach ((TEventNameType trigger, List<TransitionBase<TStateId>> transitions) in transitionsByEventName)
            {
                immutableCopy.Add(trigger,transitions);
            }
            return immutableCopy;
        }
        
        /// <summary>返回所有“全局触发器转换”（即从任意状态出发），并按触发事件分组。</summary>
        /// <remarks>注意：这是一个开销较大的操作。</remarks>
        public IReadOnlyDictionary<TEventNameType, IReadOnlyList<TransitionBase<TStateId>>> GetAllTriggerTransitionsFromAny()
        {
            var immutableCopy = new Dictionary<TEventNameType, IReadOnlyList<TransitionBase<TStateId>>>();
            foreach ((TEventNameType trigger, List<TransitionBase<TStateId>> transitions) in triggerTransitionsFromAny)
            {
                immutableCopy.Add(trigger, transitions);
            }
            return immutableCopy;
        }
        
        
        //TODO:缺少可视化部分的代码
        
        
        
        
        
    }
    
    // 重载的 StateMachine 类，以简化通用情况的使用方式。
    // 例如：你可以写 new StateMachine()，而不用写 new StateMachine<string, string, string>()
    
    /// <inheritdoc />
    public class StateMachine<TStateId, TEventNameType> : StateMachine<TStateId, TStateId, TEventNameType>
    {
        /// <inheritdoc />
        public StateMachine(bool needsExitTime = false, bool isGhostState = false, bool rememberLastState = false)
            : base(needsExitTime: needsExitTime, isGhostState: isGhostState, rememberLastState: rememberLastState)
        {
        }
    }

    /// <inheritdoc />
    public class StateMachine<TStateId> : StateMachine<TStateId, TStateId, string>
    {
        public StateMachine(bool needsExitTime = false, bool isGhostState = false, bool rememberLastState = false)
            : base(needsExitTime: needsExitTime, isGhostState: isGhostState, rememberLastState: rememberLastState)
        {
        }
    }

    /// <inheritdoc />
    public class StateMachine : StateMachine<string, string, string>
    {
        /// <inheritdoc />
        public StateMachine(bool needsExitTime = false, bool isGhostState = false, bool rememberLastState = false)
            : base(needsExitTime: needsExitTime, isGhostState: isGhostState, rememberLastState: rememberLastState)
        {
        }
    }
}
