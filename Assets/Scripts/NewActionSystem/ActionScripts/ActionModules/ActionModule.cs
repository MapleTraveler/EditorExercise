using UnityEngine;

public abstract class ActionModule : ScriptableObject
{
    public abstract void Execute();
    public abstract void Execute(InputData inputData);
    
}