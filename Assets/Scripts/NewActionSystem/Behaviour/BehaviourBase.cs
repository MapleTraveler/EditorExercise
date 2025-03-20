using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

// TODO 动画机交互逻辑待实现
public class BehaviourBase
{
    readonly IStateHandler stateHandler;
    readonly ActionMapConfig actionMapConfig;   
    readonly Dictionary<ActionType, ActionBase> actionsDic;
    
    ActionBase currentAction;

    
    public BehaviourBase(IStateHandler stateHandler,ActionMapConfig actionMapConfig)
    {
        actionsDic = new Dictionary<ActionType, ActionBase>();
        this.stateHandler = stateHandler;
        this.actionMapConfig = actionMapConfig;
        InitActionMap();
        Debug.Log("行为层初始化完成");

    }

    private void InitActionMap()
    {
        foreach (var actionMap in actionMapConfig.actionMappings)
        {
            actionMap.action.Init(stateHandler);
            actionsDic.Add(actionMap.actionType, actionMap.action);
        }
    }
    public void Update()
    {
        currentAction?.LogicAction();
    }

    public void FixedUpdate()
    {
        currentAction?.PhysicsAction();
    }

    public void LateUpdate()
    {
        if (currentAction != null)
        {
            if (currentAction.IsOver())
            {
                currentAction.Exit();
                currentAction = null;
            }
        }
        
    }
    public bool TrySwitchAction(ActionType actionID)
    {
        if (actionsDic.TryGetValue(actionID, out ActionBase action))
        {
            // 由于SO是引用调用，如果当前状态是将要执行的状态，则这一步已经完成参数更新
            
            if (currentAction != action)
            {
                if (currentAction != null && action.priorityIndex > currentAction.priorityIndex)
                {
                    return false;
                }
                else
                {
                    currentAction?.Exit();
                    var result = action.CheckConditions();
                    if (result)
                    {
                        currentAction = action;
                        currentAction.Enter();
                    }

                    return result;
                }
            }
            else
            {
                return false;// 动作相同返回True
            }
        }
        else
        {
            Debug.LogWarning($"动作表中未找到Type为：{actionID}的动作");
            return false;
        }
    }

    public bool TrySwitchActionWithData(ActionType actionID, object data)
    {
        if (actionsDic.TryGetValue(actionID, out ActionBase action))
        {
            // 由于SO是引用调用，如果当前状态是将要执行的状态，则这一步已经完成参数更新
            action.ExtraInitWithData(data);
            
            if (currentAction != action)
            {
                if (currentAction != null && action.priorityIndex > currentAction.priorityIndex)
                {
                    return false;
                }
                else
                {
                    currentAction?.Exit();
                    var result = action.CheckConditions();
                    if (result)
                    {
                        currentAction = action;
                        currentAction.Enter();
                    }

                    return result;
                }
            }
            else
            {
                return false;// 动作相同返回True
            }
        }
        else
        {
            Debug.LogWarning($"动作表中未找到Type为：{actionID}的动作");
            return false;
        }
    }
    
    
}
