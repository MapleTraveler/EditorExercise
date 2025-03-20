using System.Collections.Generic;
using UnityEngine;

public abstract class RequestHandlerBase
{
    protected Queue<RequestBase> m_foreRequestsQueue;
    protected Queue<RequestBase> m_backRequestsQueue;
    

    public RequestHandlerBase()
    {
        m_foreRequestsQueue = new Queue<RequestBase>();
        m_backRequestsQueue = new Queue<RequestBase>();
        Debug.Log("请求处理层初始化完成");
    }
    
    public abstract void ReceiveRequest(RequestBase request);

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