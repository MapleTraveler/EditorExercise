using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestReceiverTest : RequestReceiverBase
{
    public RequestReceiverTest(RequestHandlerBase requestHandler, BehaviourBase behaviour) : base(requestHandler, behaviour)
    {
        
    }
    
    public override void ReceiverRequest(int requestID)
    {
        if (m_requestDic.TryGetValue(requestID, out var value))
            m_requestHandler.ReceiveRequest(value);
    }

    public override void ReceiverRequest(int requestID, params object[] data)
    {
        if (m_requestDic.TryGetValue(requestID, out var value))
        {
            m_requestDic[requestID].ExternalInit(data); 
            m_requestHandler.ReceiveRequest(value);
        }
            
    }
}
