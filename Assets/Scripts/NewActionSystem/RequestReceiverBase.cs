using System.Collections.Generic;
using UnityEngine;


public interface IRequestReceiver
{
    public void ReceiverRequest(RequestType requestType);
    public void ReceiverRequest(RequestType requestType, object data);
}
public class RequestReceiverBase : IRequestReceiver
{
    protected Dictionary<RequestType, RequestBase> m_requestDic;
    protected RequestHandlerBase m_requestHandler;
    protected RequestMapConfig _config;
    protected BehaviourBase m_behaviour;
    
    public RequestReceiverBase(RequestHandlerBase requestHandler,BehaviourBase behaviour,RequestMapConfig config)
    {
        m_requestHandler = requestHandler;
        m_behaviour = behaviour;
        
        m_requestDic = new Dictionary<RequestType, RequestBase>();
        _config  = config;
        foreach (var requestConfig in _config.requestMappings)
        {
            requestConfig.requestBase.Init(m_behaviour);
            m_requestDic.Add(requestConfig.requestType, requestConfig.requestBase);
        }
        
        Debug.Log("请求接收层处理完成");
    }

    public virtual void ReceiverRequest(RequestType requestType)
    {
        if (m_requestDic.ContainsKey(requestType))
        {
            //接受请求
        }
        else
        {
            UnityEngine.Debug.LogWarning("没有对应的ID请求，ID为" + requestType.ToString());
        }
    }

    public virtual void ReceiverRequest(RequestType requestType, object data)
    {
        if (m_requestDic.ContainsKey(requestType))
        {
            //接受请求
        }
        else
        {
            UnityEngine.Debug.LogWarning("没有对应的ID请求，ID为" + requestType.ToString());
        }
        // TODO 处理Data
    }
    

    public void RegisterRequest(RequestType requestType, RequestBase request)
    {
        if(m_requestDic.ContainsKey(requestType))
        {
            m_requestDic[requestType] = request;
        }
        else
        {
            m_requestDic.Add(requestType, request);
        }
    }

    public void UnregisterRequest(RequestType requestType) 
    {
        if (m_requestDic.ContainsKey(requestType))
        {
            m_requestDic.Remove(requestType);
        }
    }
}