namespace UnityHFSM
{
    /// <summary>
    /// 支持自定义操作（Action）的状态接口。
    /// 操作类似于内置事件（如 <c>OnEnter</c> / <c>OnLogic</c> / ...），
    /// 但这些操作是由用户自定义的。
    /// </summary>
    public interface IActionable<in TEventNameType>//TEvent是Action事件的名称，默认string
    {
        void OnAction(TEventNameType eventName);
        void OnAction<TData>(TEventNameType eventName, TData data);
    }
    
    /// <inheritdoc />
    public interface IActionable : IActionable<string>
    {
    }
}