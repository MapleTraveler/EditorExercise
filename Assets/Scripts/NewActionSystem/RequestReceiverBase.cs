using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public interface IRequestReceiver
{
    public void ReceiverRequest(int requestID);
    public void ReceiverRequest(int requestID, params object[] data);
}
public class RequestReceiverBase : IRequestReceiver
{
    protected Dictionary<int, RequestBase> m_requestDic;
    protected RequestHandlerBase m_requestHandler;
    protected BehaviourBase m_behaviour;
    
    public RequestReceiverBase(RequestHandlerBase requestHandler, BehaviourBase behaviour)
    {
        m_requestDic = new Dictionary<int, RequestBase>();
        m_requestHandler = requestHandler;
        m_behaviour = behaviour;
    }

    public virtual void ReceiverRequest(int requestID)
    {
        if (m_requestDic.ContainsKey(requestID))
        {
            //接受请求
        }
        else
        {
            UnityEngine.Debug.LogWarning("没有对应的ID请求，ID为" + requestID.ToString());
        }
    }

    public virtual void ReceiverRequest(int requestID, params object[] data)
    {
        if (m_requestDic.ContainsKey(requestID))
        {
            //接受请求
        }
        else
        {
            UnityEngine.Debug.LogWarning("没有对应的ID请求，ID为" + requestID.ToString());
        }
        // TODO 处理Data
    }

    public virtual void ReceiverRequestWithData(int requestID,params object[] data)
    {

    }

    public void RegisterRequest(int requestID, RequestBase request)
    {
        if(m_requestDic.ContainsKey(requestID))
        {
            m_requestDic[requestID] = request;
        }
        else
        {
            m_requestDic.Add(requestID, request);
        }
    }

    public void UnregisterRequest(int requestID) 
    {
        if (m_requestDic.ContainsKey(requestID))
        {
            m_requestDic.Remove(requestID);
        }
    }
}