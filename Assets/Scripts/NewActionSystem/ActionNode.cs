using UnityEngine;



public class ActionNode
{
    public ActionType Type { get; }
    public int Priority { get; }     // 优先级数值
    public float Timestamp { get; }  // 请求产生时间
    public bool CanBeInterrupted { get; } // 是否允许被更高优先级打断
    public object Context { get; }   // 附加数据（如移动向量）

    public ActionNode(ActionType type, int priority, object context = null)
    {
        Type = type;
        Priority = priority;
        Timestamp = Time.time;
        Context = context;
        CanBeInterrupted = true; // 默认允许打断
    }
}