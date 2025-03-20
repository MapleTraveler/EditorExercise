using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestReceiverTest : RequestReceiverBase
{
    public RequestReceiverTest(RequestHandlerBase requestHandler, BehaviourBase behaviour, RequestMapConfig config) : base(requestHandler, behaviour, config)
    {
    }

    public override void ReceiverRequest(RequestType requestType)
    {
        if (m_requestDic.TryGetValue(requestType, out var value))
        {
            m_requestHandler.ReceiveRequest(value);
            Debug.Log($"成功拿到id为{requestType}的请求");
        }
            
    }

    public override void ReceiverRequest(RequestType requestType, object data)
    {
        if (m_requestDic.TryGetValue(requestType, out var value))
        {
            m_requestDic[requestType].ExternalInit(data); 
            m_requestHandler.ReceiveRequest(value);
            Debug.Log($"成功拿到id为{requestType}的请求");
        }
            
    }
}
