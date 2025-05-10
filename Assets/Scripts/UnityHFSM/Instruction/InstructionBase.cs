using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityHFSM.StateMachine;

public abstract class InstructionBase
{
    protected Queue<RequestBase> m_foreRequestsQueue;
    protected Queue<RequestBase> m_backRequestsQueue;
    protected BehaviourBase m_behaviour;
    protected StateMachine m_stateMachine;
    public Dictionary<Type, RequestBase> requestDic;

    protected InstructionBase(StateMachine stateMachine,BehaviourBase mBehaviour)
    {
        m_behaviour = mBehaviour;
        m_stateMachine = stateMachine;
        m_foreRequestsQueue = new Queue<RequestBase>();
        m_backRequestsQueue = new Queue<RequestBase>();
        requestDic = new Dictionary<Type, RequestBase>();
    }
    
    public abstract void ReceiveRequest(RequestBase inputData);
    protected abstract void ParsingInstruction(int inputData);

    protected virtual void OnUpdate()
    {

    }

    protected virtual void OnFixedUpdate()
    {

    }

    protected void EnqueueFore(RequestBase request)
    {
        m_foreRequestsQueue.Enqueue(request);
    }

    protected RequestBase DequeueFore()
    {
        return m_foreRequestsQueue.Dequeue();
    }

    protected void EnqueueBack(RequestBase request)
    {
        m_backRequestsQueue.Enqueue(request);
    }
    protected RequestBase DequeueBack()
    {
        return m_backRequestsQueue.Dequeue();
    }

    public void Update()
    {
        OnUpdate();
    }
    
    public void FixedUpdate()
    {
        OnFixedUpdate();
    }        
        
}
