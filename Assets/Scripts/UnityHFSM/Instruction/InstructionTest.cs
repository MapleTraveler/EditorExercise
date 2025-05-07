using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionTest : InstructionBase
{
    public InstructionTest(StateMachine stateMachine,BehaviourBase mBehaviourBase) : base(stateMachine,mBehaviourBase)
    {
        
    }

    public override void ReceiveRequest(RequestBase request)
    {
        if (!m_foreRequestsQueue.Contains(request))
        {
            EnqueueFore(request);
        }
    }
    
    // TODO 废弃
    protected override void ParsingInstruction(int inputData)
    {
        var curSwitchNode = m_stateMachine.currentStateBase.switchNodes;
        ConditionManager.Instance.curInputData = inputData;
        foreach (var switchNode in curSwitchNode)
        {
            var nextState = switchNode.SwitchStateJudge(m_stateMachine.currentStateBase,checkInput: true);
            RequestBase nextRequestNode = requestDic[nextState.GetType()];
            
            //nextState已经在PlayerControl中初始化，此处作废
            //nextRequestNode.nextState = nextState;
            
            ReceiveRequest(nextRequestNode);
        }
    }
    protected override void OnUpdate()
    {
        //每一帧把仍然存活的队列从前端移至后端，在下一帧把后端队列数据移入前端队列。
        for(int i = 0; i < m_backRequestsQueue.Count; i++)
        {
            EnqueueFore(DequeueBack());
        }
        
        ParsingInstruction(PlayerInputData.SentInputData());//当前指令队列输入
        bool haveExeRequest = false;
        int foreRequestsQueueCount = m_foreRequestsQueue.Count;
        Debug.Log($"前端队列请求数量：{m_foreRequestsQueue.Count}");
        for(int i = 0; i < foreRequestsQueueCount; i++)
        {
            Debug.Log($"当前请求序号：{i}");
            var curRequest = m_foreRequestsQueue.Peek();
            //bool canExe = curRequest.ExecuteJudge();
            bool over = curRequest.RequestUpdate();

            if (!haveExeRequest)
            {
                //如果有生命周期有两帧的话会一直执行，故执行后立即出队
                // if (canExe)
                // {
                //     curRequest.RequestAction();
                //     over = true;
                //     haveExeRequest = true;
                // }
            }
            
            
            if(over)
            {
                DequeueFore();
                continue;
            }
            EnqueueBack(DequeueFore());
        }
    }

    protected override void OnFixedUpdate()
    {
            
    }
}
