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