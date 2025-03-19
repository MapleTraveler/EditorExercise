using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public enum RequestType
{
    Move,
    Jump,
    Roll,
    LightAttack,
    HeavyAttack,
    UseItem1,
    UseItem2
}


public abstract class RequestBase : ScriptableObject
{
    [SerializeField] public ActionType actionType;//可配置的下一个状态切换请求
    [SerializeField] protected int m_requestLifeSteps = 1;//生命周期
    [SerializeField] public int priorityIndex { get; } = 1; // 越小则越优先，同级则后来者可以替代先来者
    
    
    protected int m_requestCount;//当前周期
    protected BehaviourBase m_behaviour;
    protected object data;

    protected abstract bool OnRequestAction();
    protected abstract bool OnRequestOver();

    public virtual bool RequestUpdate()
    {
        m_requestCount++;
        return IsOver();
    }

    public bool IsOver()
    {
        return m_requestCount >= m_requestLifeSteps;
    }
    
    public bool RequestAction() 
    {
        m_requestCount = 0;
        return OnRequestAction();
    }
    public bool RequestOver() 
    {
        m_requestCount = 0;
        return OnRequestOver();
    }

    public virtual void ExternalInit(params object[] data)
    {
        this.data = data;
    }
    
    
    public void Init(BehaviourBase behaviour)
    {
        m_behaviour = behaviour;
    }
}
