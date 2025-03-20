using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
public enum ActionType
{
    Move,
    Jump,
    Roll,
    LightAttack,  // 左键
    HeavyAttack,   // 右键
    UseItem1,
    UseItem2,
    
}
[System.Serializable]
public class ActionMethod : IComparable<ActionMethod>
{
    // ActionMethod 所在的 List不同，ActiveFrame对应的计数形式也不同
    // 在LogicList则是逻辑帧，物理方法表则是物理帧
    [field:SerializeField] public int ActiveFrame { get;private set; }
    
    [SerializeField] public ActionModule actionEvent;
    
    public bool CanActive(int frame){
        return ActiveFrame <= frame;
    }

    public int CompareTo(ActionMethod other)
    {
        return ActiveFrame.CompareTo(other.ActiveFrame);
    }
}
[CreateAssetMenu(menuName = "Action")]
public class ActionBase : ScriptableObject
{
    protected IStateHandler m_AnimState;
    protected Rigidbody2D m_rb2D;
    protected InputData inputData;
    
    protected int curLogicFrame;
    protected int curPhysicFrame;
    
    
    [SerializeField] protected string animName;
    //protected int totalFrame;
    [field:SerializeField] public int priorityIndex { get; protected set; }
    [SerializeField] protected bool canBeInterrupted;
    [SerializeField] protected bool canInterruptCurrentAnim;
    [SerializeField] protected List<ConditionBase> executeConditions = new();
    
    [SerializeField] 
    [ListDrawerSettings(ShowIndexLabels = true)]
    protected List<ActionMethod> logicActionMethods = new();

    [SerializeField]
    [ListDrawerSettings(ShowIndexLabels = true)]
    protected List<ActionMethod> physicActionMethods = new();
    
    
    
    // 内部管理私有变量
    private int willExecuteLogicIndex;
    private int willExecutePhysicIndex;
    
    
    

    protected virtual void OnInit()
    {
        logicActionMethods.Sort();
        physicActionMethods.Sort();
    }

    protected virtual void OnEnter()
    {
        willExecuteLogicIndex = 0;
        willExecutePhysicIndex = 0;
        curLogicFrame = 0;
        curPhysicFrame = 0;
    }
    protected virtual void OnExit(){}
    
    // 暂时只实现了含参函数
    protected virtual void OnUpdate()
    {
        if (willExecuteLogicIndex < logicActionMethods.Count && logicActionMethods[willExecuteLogicIndex].CanActive(curLogicFrame))
        {
            logicActionMethods[willExecuteLogicIndex].actionEvent.Execute(inputData);
            willExecuteLogicIndex++;
        }
    }

    protected virtual void OnFixedUpdate()
    {
        if (willExecutePhysicIndex < physicActionMethods.Count && physicActionMethods[willExecutePhysicIndex].CanActive(curPhysicFrame))
        {
            physicActionMethods[willExecutePhysicIndex].actionEvent.Execute(inputData);
            willExecutePhysicIndex++;
        }
    }

    public bool CheckConditions()
    {
        bool result = true;
        foreach (ConditionBase condition in executeConditions)
        {
            result &= condition.Evaluate();
        }
        return result;
    }

    public void Enter()
    {
        OnEnter();
    }

    public void Exit()
    {
        OnExit();
    }

    public void LogicAction()
    {
        curLogicFrame++;
        OnUpdate();
    }

    public void PhysicsAction()
    {
        curPhysicFrame++;
        OnFixedUpdate();
    }

    public bool IsOver()
    {
        return willExecuteLogicIndex >= logicActionMethods.Count && willExecutePhysicIndex >= physicActionMethods.Count;
    }
    

    public void Init(IStateHandler animState)
    {
        m_AnimState = animState;
        
        OnInit();
        
    }

    public void ExtraInitWithData(object data)
    {
        inputData = (InputData)data;
    }
    
#if UNITY_EDITOR
    [Button("Sort Actions")]
    private void SortActionsInEditor()
    {
        logicActionMethods.Sort();
        physicActionMethods.Sort();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
    
    
}
