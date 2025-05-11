namespace UnityHFSM
{
    /// <summary>
    ///  一个抽象层，提供所有父状态机需要具备的核心功能，
    /// 用于实现状态转换的计时机制。
    /// 除了继承自 <see cref="IStateTimingManager"/> 的方法，
    /// 此接口还提供对当前状态和待切换状态的访问，
    /// 这对处理转换逻辑非常有用。
    /// </summary>
    public interface IStateMachine<TStateId> : IStateTimingManager
    {
        /// <summary>
        /// 正在等待的（延迟）转换目标状态。
        /// 如果当前没有挂起的转换，或者正在等待的是一个退出转换（exit transition），则返回 null。
        /// </summary>
        StateBase<TStateId> PendingState { get; }
        
        
        /// <summary>
        /// <inheritdoc cref="PendingState"/>
        /// </summary>
        TStateId PendingStateName { get; }
        
        /// <summary>
        /// 当前处于活动状态的状态对象。
        /// </summary>
        /// <remarks>
        /// 注意：当某个状态“处于活动中”时，
        /// ActiveState 返回的可能不是该状态本身，而是某个包装器状态的引用，
        /// 具体取决于所使用的状态类实现方式。
        /// </remarks>
        StateBase<TStateId> ActiveState { get; }
        
        /// <summary>
        /// <inheritdoc cref="ActiveState"/>
        /// </summary>
        TStateId ActiveStateName { get; }
        
        StateBase<TStateId> GetState(TStateId name);
    }
}