namespace UnityHFSM
{
    /// <summary>
    /// 一个反向转换（ReverseTransition）会包装另一个转换，并反转其逻辑：
    /// "from" 和 "to" 状态互换，仅当被包装的转换条件为 false 时才执行该反向转换。
    /// 同时还会交换被包装转换的 <c>BeforeTransition</c> 和 <c>AfterTransition</c> 回调。
    /// </summary>
    public class ReverseTransition<TStateId> : TransitionBase<TStateId>
    {
        public readonly TransitionBase<TStateId> wrappedTransition;
        private readonly bool shouldInitWrappedTransition;

        public ReverseTransition(TransitionBase<TStateId> wrappedTransition,
            bool shouldInitWrappedTransition)
            : base(
                from: wrappedTransition.to,
                to: wrappedTransition.from,
                forceInstantly: wrappedTransition.forceInstantly)
        {
            this.wrappedTransition = wrappedTransition;
            this.shouldInitWrappedTransition = shouldInitWrappedTransition;
        }

        public override void Init()
        {
            if (shouldInitWrappedTransition)
            {
                wrappedTransition.fsm = this.fsm;
                wrappedTransition.Init();
            }
        }
        
        public override void OnEnter()
        {
            wrappedTransition.OnEnter();
        }

        public override bool ShouldTransition()
        {
            // 反向转换的核心逻辑：仅当原转换不能成立时，才执行反向转换。
            return !wrappedTransition.ShouldTransition();
        }
        
        public override void BeforeTransition()
        {
            wrappedTransition.AfterTransition();
        }

        public override void AfterTransition()
        {
            wrappedTransition.BeforeTransition();
        }
    }
    
    /// <inheritdoc />
    public class ReverseTransition : ReverseTransition<string>
    {
        /// <inheritdoc />
        public ReverseTransition(
            TransitionBase<string> wrappedTransition,
            bool shouldInitWrappedTransition = true)
            : base(wrappedTransition, shouldInitWrappedTransition)
        {
        }
    }
}