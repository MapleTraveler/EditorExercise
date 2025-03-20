using UnityEngine;

public class RequestHandlerTest : RequestHandlerBase
{
    
    public override void ReceiveRequest(RequestBase request)
    {
        if (!m_foreRequestsQueue.Contains(request) && !m_backRequestsQueue.Contains(request))
        {
            Debug.Log("把请求添加到前端队列");
            EnqueueFore(request);
        }
    }

    
    // TODO 此处只做优先级处理，判断放在Action层
    protected override void OnUpdate()
    {
        // 比出来优先级最高的行为，其他的放到后端队列去
        RequestBase curFlamePriorityRequest = null;
        //每一帧把仍然存活的队列从前端移至后端，在下一帧把后端队列数据移入前端队列。
        for(int i = 0; i < m_backRequestsQueue.Count; i++)
        {
            EnqueueFore(DequeueBack());
        }
        
        
        int foreRequestsQueueCount = m_foreRequestsQueue.Count;
        Debug.Log($"前端队列请求数量：{m_foreRequestsQueue.Count}");
        for(int i = 0; i < foreRequestsQueueCount; i++)
        {
            //Debug.Log($"当前请求序号：{i}");
            var curRequest = DequeueFore();
            
            bool over = curRequest.RequestUpdate();
            // TODO 不破坏原有次序的同时使其被取代时正确进队？
            if (curFlamePriorityRequest == null || curRequest.priorityIndex <= curFlamePriorityRequest.priorityIndex)
            {
                if (curFlamePriorityRequest != null && !curFlamePriorityRequest.IsOver())
                {
                    EnqueueBack(curFlamePriorityRequest);
                }
                    
                
                curFlamePriorityRequest = curRequest;
                
            }
            else
            {
                if(!over)
                {
                    EnqueueBack(curRequest);
                }
            }
        }
        
        // TODO RequestAction 判断待完善 目前为当前优先级请求不为空（及自己）同时自己执行失败
        bool willEnqueue = curFlamePriorityRequest != null && !curFlamePriorityRequest.RequestAction();
        
        // 判断失败的请求是否到期
        if(willEnqueue && !curFlamePriorityRequest.IsOver())
            EnqueueBack(curFlamePriorityRequest);
        
    }

    protected override void OnFixedUpdate()
    {
        
    }
}