using System;

namespace UnityHFSM
{
    /// <summary>
    /// 用于确定状态机是否应过渡到另一状态的类。
    /// </summary>
    public class Transition<TStateId> : TransitionBase<TStateId>
    {
        private readonly Func<Transition<TStateId>,bool> condition;
        private readonly Action<Transition<TStateId>> beforeTransition;
        private readonly Action<Transition<TStateId>> afterTransition;
        
        /// <summary>
        /// 初始化一个新的 Transition 实例。
        /// </summary>
        /// <param name="from">转换起点状态的名称/标识符。</param>
        /// <param name="to">转换目标状态的名称/标识符。</param>
        /// <param name="condition">一个函数，当返回 true 时，状态机将从 <c>from</c> 状态转换到 <c>to</c> 状态。</param>
        /// <param name="beforeTransition">在状态转换即将发生前调用的回调函数。</param>
        /// <param name="afterTransition">在状态转换完成后调用的回调函数。</param>
        /// <param name="forceInstantly">
        /// 如果为 true，则忽略起点状态的 <c>needsExitTime</c> 设置，
        /// 强制立即转换状态。
        /// </param>
        public Transition(
            TStateId from, 
            TStateId to, 
            Func<Transition<TStateId>,bool> condition = null,
            Action<Transition<TStateId>> beforeTransition = null,
            Action<Transition<TStateId>> afterTransition = null,
            bool forceInstantly = false) : base(from, to, forceInstantly)
        {
            this.condition = condition;
            this.beforeTransition = beforeTransition;
            this.afterTransition = afterTransition;
        }

        
        public override bool ShouldTransition()
        {
            if (condition == null) return true;
            
            return condition(this);
        }

        public override void BeforeTransition()
        {
            beforeTransition?.Invoke(this);
        }

        public override void AfterTransition()
        {
            afterTransition?.Invoke(this);
        }
    }
    /// <inheritdoc />
    public class Transition : Transition<string>
    {
        /// <inheritdoc />
        public Transition(
            string @from,
            string to,
            Func<Transition<string>, bool> condition = null,
            Action<Transition<string>> beforeTransition = null,
            Action<Transition<string>> afterTransition = null,
            bool forceInstantly = false) : base(
            @from,
            to,
            condition,
            beforeTransition: beforeTransition,
            afterTransition: afterTransition,
            forceInstantly: forceInstantly)
        {
        }
    }
}