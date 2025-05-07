using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IntNumericalRelationship
{
    Less,Greater,Equals,NotEqual
}
[CreateAssetMenu(fileName = "IntCondition",menuName = "SwitchCondition/IntCondition")]
public class IntCondition : ConditionValueBase
{
    
    [SerializeField]public int currentValue;//设定值
    [SerializeField]private IntNumericalRelationship m_relationship;
    [SerializeField]private int targetValue;//目标值
    
    public override bool SwitchJudge()
    {
        switch (m_relationship)
        {
            case IntNumericalRelationship.Less:
                return currentValue < targetValue;
                break;
            case IntNumericalRelationship.Greater:
                return currentValue > targetValue;
                break;
            case IntNumericalRelationship.Equals:
                return currentValue == targetValue;
                break;
            case IntNumericalRelationship.NotEqual:
                return currentValue != targetValue;
                break;
            default:
                return false;
        }
    }
    
}
