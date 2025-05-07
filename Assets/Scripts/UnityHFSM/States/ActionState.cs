using System;

namespace UnityHFSM
{
    /// <summary>
    /// 支持自定义操作的状态基类。
    /// </summary>
    public class ActionState<TStateId, TEventNameType> : StateBase<TStateId>,IActionable<TEventNameType>
    {
        // Lazy initialized
        private ActionStorage<TEventNameType> actionStorage;
        
        /// <summary>
        /// 初始化 ActionState 类的新实例。
        /// </summary>
        /// <inheritdoc cref="StateBase{T}(bool, bool)"/>
        public ActionState(bool needsExitTime, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
        }
        
        /// <summary>
        /// 添加一个可以通过 <c>OnAction()</c> 调用的动作。动作类似于内建事件 <c>OnEnter</c> / <c>OnLogic</c> 等，
        /// 但由用户自定义。
        /// </summary>
        /// <param name="eventName">动作的名称（标识符）。</param>
        /// <param name="action">当该动作被触发时应调用的函数。</param>
        /// <returns>返回自身，方便链式调用（Fluent Interface）。</returns>
        public ActionState<TStateId, TEventNameType> AddAction(TEventNameType eventName, Action action)
        {
            actionStorage = actionStorage ?? new ActionStorage<TEventNameType>();
            actionStorage.AddAction(eventName, action);
            return this;
        }

        
        /// <summary>
        /// 执行指定名称的动作。
        /// 如果该动作未被定义或未添加，则不会执行任何操作。
        /// </summary>
        /// <param name="eventName">动作的名称（标识符）。</param>
        public void OnAction(TEventNameType eventName)
        {
            actionStorage?.RunAction(eventName);
        }

        
        /// <summary>
        /// 执行指定名称的动作，并传入一个参数给动作函数。
        /// 如果该动作未被定义或未添加，则不会执行任何操作。
        /// </summary>
        /// <param name="eventName">动作的名称（标识符）。</param>
        /// <param name="data">传递给动作函数的参数。</param>
        /// <typeparam name="TData">参数的类型。</typeparam>
        public void OnAction<TData>(TEventNameType eventName, TData data)
        {
            actionStorage?.RunAction<TData>(eventName, data);
        }
    }
    /// <inheritdoc />
    public class ActionState<TStateId> : ActionState<TStateId, string>
    {
        /// <inheritdoc />
        public ActionState(bool needsExitTime, bool isGhostState = false)
            : base(needsExitTime: needsExitTime, isGhostState: isGhostState)
        {
        }
    }

    /// <inheritdoc />
    public class ActionState : ActionState<string, string>
    {
        /// <inheritdoc />
        public ActionState(bool needsExitTime, bool isGhostState = false)
            : base(needsExitTime: needsExitTime, isGhostState: isGhostState)
        {
        }
    }
}