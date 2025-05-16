namespace UnityHFSM.Exceptions
{
    public static class Common
    {
        public static StateMachineException NonInitialized<TStateId>(
            StateBase<TStateId> fsm,
            string context = null,
            string problem = null,
            string solution = null)
        {
            return CreateStateMachineException(
                fsm, 
                context, 
                problem ?? "当前状态为 null，因为状态机尚未初始化。", 
                solution ?? "请调用 fsm.SetStartState(...) 和 fsm.Init() 或 fsm.OnEnter() 来初始化状态机。");
        }

        public static StateMachineException StateNotFound<TStateId>(
            StateBase<TStateId> fsm,
            string stateName,
            string context = null,
            string problem = null,
            string solution = null)
        {
            return CreateStateMachineException(
                fsm,
                context,
                problem ?? $"状态 \"{stateName}\" 尚未定义或不存在。",
                solution ?? ("\n"
                             + "1. 请检查状态名称以及转移的起始状态（from）和目标状态（to）是否有拼写错误\n"
                             + "2. 在调用 Init / OnEnter / OnLogic / RequestStateChange / ... 之前先添加该状态")
            );
        }

        public static StateMachineException MissingStarState<TStateId>(
            StateBase<TStateId> fsm,
            string context = null,
            string problem = null,
            string solution = null)
        {
            return CreateStateMachineException(
                fsm,
                context,
                problem ?? "未选择初始状态。状态机至少需要一个状态才能正常运行。",
                solution ?? "请确保在调用 Init() 或 OnEnter() 方法前，已通过 fsm.AddState(...) 添加至少一个状态。"
            );

        }
        
        /// <summary>
        /// 抛出异常：错误地通过索引器获取了一个非状态机的状态。
        /// </summary>
        /// <param name="fsm">当前的状态机实例。</param>
        /// <param name="stateName">尝试获取的状态名称。</param>
        /// <returns>封装好的状态机异常实例。</returns>
        public static StateMachineException QuickIndexerMisusedForGettingState<TStateId>(
            StateBase<TStateId> fsm,
            string stateName)
        {
            return CreateStateMachineException(
                fsm,
                context: "使用索引器访问嵌套状态机时出错",
                problem: "选中的状态并不是一个状态机（StateMachine 类型）。",
                solution: "索引器（this[...]）仅用于快速访问嵌套状态机。若要访问普通状态，请使用 GetState(\"" + stateName + "\") 方法。"
            );
        }

        
        // 封装一层， 通过 StateMachineWalker 定位用
        private static StateMachineException CreateStateMachineException<TStateId>(
            StateBase<TStateId> fsm,
            string context = null,
            string problem = null,
            string solution = null)
        {
            // TODO:等待补充 StateMachineWalker 工具类
            // string path = StateMachineWalker.GetStringPathOfState(fsm);
            string path = fsm.ToString();
            return new StateMachineException(ExceptionFormatter.Format(
                location: $"state machine'{path}'",
                context: context,
                problem: problem,
                solution: solution
            ));
        }
    }
}