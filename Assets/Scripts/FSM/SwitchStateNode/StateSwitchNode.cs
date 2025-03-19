using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "StateSwitchNode", menuName = "StateSwitchNode")]
public class StateSwitchNode : ScriptableObject
{
    [Header("要切换的状态")]
    public StateBase willSwitchToState;
    [Header("通用条件")]
    public List<ConditionValueBase> configNormalConditions;
    [Header("输入条件")]
    public List<PlayerInputCondition> configInputConditions;//第一次检测时使用

    public void Init()
    {

    }
    public StateBase SwitchStateJudge(StateBase currentState,bool checkNormal = false,bool checkInput = false){
        if (configNormalConditions.Count == 0 && checkNormal)
            return willSwitchToState;
        if (configInputConditions.Count == 0 && checkInput)
            return willSwitchToState;
        if (checkInput)
        {
            foreach (var condition in configInputConditions)
            {
                //条件有一个满足就跳转，根据实际再修改
                if (condition.SwitchJudge())
                {
                    return willSwitchToState;
                }
            }
        }

        if (checkNormal)
        {
            foreach (var condition in configNormalConditions)
            {
                //条件有一个满足就跳转，根据实际再修改
                if (condition.SwitchJudge())
                {
                    return willSwitchToState;
                }
            }
        }
        
        return currentState;
    }
}
