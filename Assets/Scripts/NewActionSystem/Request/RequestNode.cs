using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "RequestNode",menuName = "RequestNode")]
public class RequestNode : RequestBase
{
    protected override bool OnRequestAction()
    {
        Debug.Log("请求执行行为"+actionType);
        // TODO 行为层判断还没准备好
        return m_behaviour.TrySwitchActionWithData(actionType,data);
    }
    

    protected override bool OnRequestOver()
    {
        return true;
    }
}
