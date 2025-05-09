namespace UnityHFSM
{
    /// <summary>
    /// 表示可以接收触发器事件（trigger）的状态接口，例如状态机（StateMachine）等。
    /// </summary>
    /// <typeparam name="TEventNameType">触发器的类型。</typeparam>
    public interface ITriggerable<in TEventNameType>
    {
        /// <summary>
        /// 当某个触发器被激活时调用。
        /// </summary>
        /// <param name="eventName">触发器的名称或标识符。</param>
        void Trigger(TEventNameType eventName);
    }
    
    /// <inheritdoc />
    public interface ITriggerable : ITriggerable<string>
    {
    }
}