using System;
using System.Collections.Generic;
using UnityHFSM.Exceptions;

namespace UnityHFSM
{
    /// <summary>
    /// 可以存储和运行动作的类。
    /// 它使各种状态类中的动作系统实施起来更容易。
    /// </summary>
    public class ActionStorage<TEventNameType>
    {
        private readonly Dictionary<TEventNameType, Delegate> actionsByEventName = new Dictionary<TEventNameType, Delegate>();




        private TTargetType TryGetAndCastAction<TTargetType>(TEventNameType eventName) where TTargetType : Delegate
        {
            Delegate action = null;
            actionsByEventName.TryGetValue(eventName, out action);
            if (action == null)
            {
                return null;
            }
            
            TTargetType target = action as TTargetType;
            if (target == null)
            {
                throw new InvalidOperationException(ExceptionFormatter.Format(
                    context: $"Trying to call the action '{eventName}'.",
                    problem: $"The expected argument type ({typeof(TTargetType)}) does not match the "
                             + $"type of the added action ({action}).",
                    solution: "Check that the type of action that was added matches the type of action that is called. \n"
                              + "E.g. AddAction<int>(...) => OnAction<int>(...) \n"
                              + "E.g. AddAction(...) => OnAction(...) \n"
                              + "E.g. NOT: AddAction<int>(...) => OnAction<bool>(...)"
                ));
            }

            return target;
        }
        
        
        /// <summary>
        /// 添加一个可以通过 <see cref="RunAction"/> 调用的动作。动作类似于内建事件
        /// <c>OnEnter</c> / <c>OnLogic</c> / ...，但由用户自定义。
        /// </summary>
        /// <param name="eventName">动作的名称。</param>
        /// <param name="action">在该动作被执行时调用的函数。</param>
        public void AddAction(TEventNameType eventName, Action action)
        {
            actionsByEventName[eventName] = action;
        }
        /// <summary>
        /// 执行具有指定名称的无参动作。
        /// 如果该动作未定义或未被添加，则不会执行任何操作。
        /// </summary>
        /// <param name="eventName">动作的名称（标识符）。</param>
        public void RunAction(TEventNameType eventName)
            => TryGetAndCastAction<Action>(eventName)?.Invoke();

        /// <summary>
        /// 执行具有指定名称的带参动作，并传递一个参数给动作函数。
        /// 如果该动作未定义或未被添加，则不会执行任何操作。
        /// </summary>
        /// <param name="eventName">动作的名称（标识符）。</param>
        /// <param name="data">传递给动作函数的参数。</param>
        /// <typeparam name="TData">参数的数据类型。</typeparam>
        public void RunAction<TData>(TEventNameType eventName, TData data)
            => TryGetAndCastAction<Action<TData>>(eventName)?.Invoke(data);
    }
}