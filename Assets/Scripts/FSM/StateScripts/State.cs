using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class State : StateBase
{
    [SerializeField] private List<ActionBase> configAtomActions; //行为表
    protected Queue<ActionBase> atomActionsQueue;//行为执行队列

    protected ActionBase currentAction;//当前正在执行的行为
    
    /// <summary>
    /// 进入状态时初始化行为队列，并进入第一个行为
    /// </summary>
    public override void Enter()
    {
        atomActionsQueue = new Queue<ActionBase>();//初始化行为队列
        //遍历并初始化行为，将行为入队
        foreach (var action in configAtomActions)
        {
            //action.Init(this,m_player);
            atomActionsQueue.Enqueue(action);
        }
        
        //执行完进入行为后，队列不空则peek首个行为，并enter
        if (atomActionsQueue.Count > 0)
        {
            currentAction = atomActionsQueue.Peek();
            currentAction.Enter();
        }
        else
        {
            currentAction = null;
        }
        Debug.Log($"进入状态：{name}");
        
    }

    /// <summary>
    /// 逻辑行为的执行，每次执行前检查当前行为是否执行完毕
    /// </summary>
    protected override void OnLogicExecute()
    {
        if (TrySwitchAction())
        {
            currentAction.LogicAction();
        }

        Debug.Log($"Execute:{currentAction?.name}");
    }

    protected override void OnPhysicsExecute()
    {
        currentAction?.PhysicsAction();
    }
    //检测当前行为是否执行完毕，执行完毕则回到队尾，执行Exit，将peek下个行为
    protected virtual bool TrySwitchAction()
    {
        if (currentAction)
        {
            // if (currentAction.IsDone())
            // {
            //     currentAction.Exit();
            //     atomActionsQueue.Enqueue(atomActionsQueue.Dequeue());
            //     currentAction = atomActionsQueue.Peek();
            //     currentAction.Enter();
            // }

            return true;
        }
        else
        {
            return false;
        }
        
    }

    protected override void BeforeLogicExecute()
    {
        // foreach (var switchNode in switchNodes)
        // {
        //     StateBase nextState = switchNode.SwitchStateJudge(this);
        //     if (nextState.GetType() != GetType())
        //     {
        //         m_StateMachine.SwitchState(nextState.GetType());
        //         return;
        //     }
        // }
        // Debug.Log($"正在执行状态：{name}");
    }

    public override void Exit()
    {
        Debug.Log($"离开状态：{name}");
    }
}
