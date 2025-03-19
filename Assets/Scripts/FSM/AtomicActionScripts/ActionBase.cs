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
public class ActionMethod  
{
    public int activeFrame;
    public int continueFrameCount;
    
    [SerializeField] public ActionMethod actionEvent;
}
public abstract class ActionBase : ScriptableObject
{
    protected IStateHandler m_AnimState;
    protected Rigidbody2D m_rb2D;
    protected InputData inputData;
    
    protected int curFrame;
    
    [SerializeField] protected string animName;
    [SerializeField] protected int totalFrame;
    [field:SerializeField] public int priorityIndex { get; protected set; }
    [SerializeField] protected bool canBeInterrupted;
    [SerializeField] protected bool canInterruptCurrentAnim;
    [SerializeField] protected List<ConditionBase> executeConditions = new();
    [SerializeField] protected List<ActionMethod> actionMethods = new();
     
    protected virtual void OnInit(){}
    protected virtual void OnEnter(){}
    protected virtual void OnExit(){}
    protected virtual void OnUpdate(){}
    protected virtual void OnFixedUpdate(){}

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
        curFrame++;
        OnUpdate();
    }

    public void PhysicsAction()
    {
        OnFixedUpdate();
    }

    public bool IsOver()
    {
        return curFrame >= totalFrame;
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
    
    
}
