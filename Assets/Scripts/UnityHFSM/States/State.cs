using System;

namespace UnityHFSM
{
    /// <summary>
    /// “普通”状态类，可以在进入、执行逻辑和退出时运行代码，
    /// 同时还处理下一次状态转换的时机。
    /// </summary>
    /// <typeparam name="TStateId">状态名称标识符的类型</typeparam>
    /// <typeparam name="TEventNameType">事件名称标识符的类型</typeparam>
    public class State<TStateId,TEventNameType> : ActionState<TStateId, TEventNameType>
    {
        private readonly Action<State<TStateId,TEventNameType>> onEnter;
        private readonly Action<State<TStateId,TEventNameType>> onLogic;
        private readonly Action<State<TStateId,TEventNameType>> onExit;
        private readonly Func<State<TStateId,TEventNameType>,bool> canExit;

        public ITimer timer;

        ///  <summary>
        ///  初始化 State 类的新实例。
        ///  </summary>
        ///  <param name="onEnter">当状态机过渡到此状态（进入此状态）时调用。</param>
        ///  <param name="onLogic">当该状态为活动状态时，由状态机的逻辑函数每帧调用的函数。</param>
        ///  <param name="onExit">当状态机从此状态过渡到另一状态（退出此状态）时调用。</param>
        ///  <param name="canExit">仅当 <c>needsExitTime</c> 为 true 时才会调用）：
        ///		当从当前状态切换到其他状态的请求发生时，会调用此方法。
        ///		如果此状态已经可以退出，应立即调用 <c>fsm.StateCanExit()</c>。
        ///		如果此状态当前还不能退出（例如还在播放动画），
        ///		则应稍后（如在 <c>OnLogic()</c> 中）再调用 <c>fsm.StateCanExit()</c> 通知状态机。</param>
        ///  <param name="needsExitTime">
        ///     指定该状态在转换时是否允许立即退出（false），
        ///     如果为 true，状态机将在状态准备好退出前等待。</param>
        ///  <param name="isGhostState">
        ///     如果为 true，则该状态将变为“幽灵状态”，即状态机不会停留在此状态，
        /// 	一旦进入该状态，会立即尝试所有可能的出边转换，而不是等到下一次 OnLogic 调用。</param>
        public State(
            Action<State<TStateId,TEventNameType>> onEnter = null,
            Action<State<TStateId,TEventNameType>> onLogic = null,
            Action<State<TStateId,TEventNameType>> onExit = null,
            Func<State<TStateId,TEventNameType>,bool> canExit = null,
            bool needsExitTime = false, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
            this.onEnter = onEnter;
            this.onLogic = onLogic;
            this.onExit = onExit;
            this.canExit = canExit;
            this.timer = new Timer();
        }

        public override void OnEnter()
        {
            timer.Reset();
            onEnter?.Invoke(this);
        }

        public override void OnLogic()
        {
            onLogic?.Invoke(this);
            // 在调用 onLogic 之后检查该状态是否可以退出，因为 onLogic 中可能触发状态转换。
            // 如果在状态已经不再活跃的情况下仍然调用 onLogic，会导致无效行为。
            if (needsExitTime && canExit != null && fsm.HasPendingTransition && canExit(this))
            {
                fsm.StateCanExit();
            }
        }

        public override void OnExit()
        {
            onExit?.Invoke(this);
        }

        public override void OnExitRequest()
        {
            if (canExit != null && canExit(this))
            {
                fsm.StateCanExit();
            }
        }
    }
    /// <inheritdoc />
    public class State<TStateId> : State<TStateId, string>
    {
        /// <inheritdoc />
        public State(
            Action<State<TStateId, string>> onEnter = null,
            Action<State<TStateId, string>> onLogic = null,
            Action<State<TStateId, string>> onExit = null,
            Func<State<TStateId, string>, bool> canExit = null,
            bool needsExitTime = false,
            bool isGhostState = false)
            : base(
                onEnter,
                onLogic,
                onExit,
                canExit,
                needsExitTime: needsExitTime,
                isGhostState: isGhostState)
        {
        }
    }

    /// <inheritdoc />
    public class State : State<string, string>
    {
        /// <inheritdoc />
        public State(
            Action<State<string, string>> onEnter = null,
            Action<State<string, string>> onLogic = null,
            Action<State<string, string>> onExit = null,
            Func<State<string, string>, bool> canExit = null,
            bool needsExitTime = false,
            bool isGhostState = false)
            : base(
                onEnter,
                onLogic,
                onExit,
                canExit,
                needsExitTime: needsExitTime,
                isGhostState: isGhostState)
        {
        }
    }
}