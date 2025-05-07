namespace UnityHFSM
{
    /// <summary>
    /// 一个抽象层接口，提供每个父状态机都必须支持的一部分功能，
    /// 用于实现状态转换的时间控制机制（如延迟退出）。
    /// 这个接口的作用是：让父状态机的行为可被子状态透明调用，
    /// 并且对子状态隐藏具体实现细节，使结构更灵活。
    /// </summary>
    /// <remarks>
    /// 尤其重要的一点是：
    /// 子状态不需要知道父状态机完整的泛型参数类型列表。
    /// 否则，在每一层使用不同泛型类型的分层状态机将无法实现。
    /// </remarks>
    public interface IStateTimingManager//TODO:暂时抛弃泛型remark中的内容供之后扩展泛型查看
    {
        /// <summary>
        /// 通知状态机当前激活的状态可以安全退出。如果当前存在待执行的转换请求，状态机会立即执行该转换。
        /// </summary>
        /// <remarks>
        /// 该信号仅在调用时刻有效，不能用于之后的某个时刻，因此不会被保存或记忆。<para/>
        /// 此方法只有在存在 pendingTransition（待定状态转换）时才有效，并且转换只能在 <c>OnEnter</c> 调用后才会检查，
        /// 因此如果在 <c>OnEnter</c> 中调用该方法将不会产生任何效果。
        /// </remarks>
        void StateCanExit();
        
        bool HasPendingTransition { get; }
        
        IStateTimingManager ParentFsm { get; }
    }
}