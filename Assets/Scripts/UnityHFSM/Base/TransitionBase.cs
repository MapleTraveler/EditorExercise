namespace UnityHFSM
{
    public class TransitionBase<TStateId> : ITransitionListener
    {
        public readonly TStateId from;
        public readonly TStateId to;

        public readonly bool forceInstantly;
        public bool isExitTransition;

        public IStateMachine<TStateId> fsm;

        /// <summary>
        /// 初始化一个新的 TransitionBase 实例。
        /// </summary>
        /// <param name="from">转换起点状态的名称/标识符。</param>
        /// <param name="to">转换目标状态的名称/标识符。</param>
        /// <param name="forceInstantly">
        /// 如果为 true，则忽略起点状态的 <c>needsExitTime</c> 设置，
        /// 强制立即转换状态。
        /// </param>
        public TransitionBase(TStateId from, TStateId to, bool forceInstantly = false)
        {
            this.from = from;
            this.to = to;
            this.forceInstantly = forceInstantly;
            this.isExitTransition = false;
        }
        
        /// <summary>
        /// 初始化转换方法，通常在 <c>fsm</c> 等属性设置之后调用。
        /// 子类可重写此方法用于注册监听器或内部缓存等初始化操作。
        /// </summary>
        public virtual void Init()
        {

        }
        
        
        /// <summary>
        /// 当状态机进入 <c>from</c> 状态时调用。
        /// 可用于准备转换逻辑或记录当前状态信息。
        /// </summary>
        public virtual void OnEnter()
        {
            
        }
        
        /// <summary>
        /// 判断是否应从 <c>from</c> 状态转换到 <c>to</c> 状态。
        /// </summary>
        /// <returns>如果应执行状态转换，则返回 true。</returns>
        public virtual bool ShouldTransition()
        {
            return true;
        }


        /// <summary>
        /// 状态转换即将执行前调用的回调方法。
        /// 可用于设置标志位、播放动画或中断其他状态。
        /// </summary>
        public virtual void BeforeTransition()
        {
            
        }

        /// <summary>
        /// 状态转换执行完成后调用的回调方法。
        /// 可用于清理、日志记录或触发其他逻辑。
        /// </summary>
        public virtual void AfterTransition()
        {
            
        }
    }
    /// <inheritdoc />
    public class TransitionBase : TransitionBase<string>
    {
        /// <inheritdoc />
        public TransitionBase(string @from, string to, bool forceInstantly = false) : base(@from, to, forceInstantly)
        {
        }
    }
}