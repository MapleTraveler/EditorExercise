using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionManager : MonoBehaviour
{
    static ConditionManager instance;
    public int curInputData = 0;//输入由此获取
    public StateMachine playerStateMachine;
    public StateBase curState => playerStateMachine.currentStateBase;
    public static ConditionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType(typeof(ConditionManager)) as ConditionManager;
            }

            return instance;
        }
    }

    public List<ConditionValueBase> conditions;
    public Dictionary<string, ConditionValueBase> conditionsDic;
    private void Awake()
    {
        conditionsDic = new Dictionary<string, ConditionValueBase>();
        foreach (var condition in conditions)
        {
            conditionsDic.Add(condition.name,condition);
        }
    }
    public void SetIntConditionsValue(string conditionName, int value)
    {
        ConditionValueBase baseCondition = GetConditions(conditionName);
        if (baseCondition)
        {
            if (baseCondition is IntCondition resCondition)
            {
                resCondition.currentValue = value;
            }
            else
            {
                Debug.LogError($"值{conditionName}类型不对");
            }
        }
        else
        {
            Debug.LogError($"值{conditionName}不存在");
        }
    }
    public void SetFloatConditionsValue(string conditionName, float value)
    {
        ConditionValueBase baseCondition = GetConditions(conditionName);
        if (baseCondition)
        {
            if (baseCondition is FloatCondition resCondition)
            {
                resCondition.currentValue = value;
            }
            else
            {
                Debug.LogError($"值{conditionName}类型不对");
            }
        }
        else
        {
            Debug.LogError($"值{conditionName}不存在");
        }
    }
    public void SetBoolConditionsValue(string conditionName, bool value)
    {
        ConditionValueBase baseCondition = GetConditions(conditionName);
        if (baseCondition)
        {
            if (baseCondition is BoolCondition resCondition)
            {
                resCondition.currentValue = value;
            }
            else
            {
                Debug.LogError($"值{conditionName}类型不对");
            }
        }
        else
        {
            Debug.LogError($"值{conditionName}不存在");
        }
    }
    

    public ConditionValueBase GetConditions(string conditionName)
    {
        conditionsDic.TryGetValue(conditionName, out var res);
        return res;
    }
}
