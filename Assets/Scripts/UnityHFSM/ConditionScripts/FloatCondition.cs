using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FloatNumericalRelationship
{
    Less,Greater
}
public class FloatCondition : ConditionValueBase
{
    [SerializeField]public float currentValue;//设定值
    [SerializeField]private FloatNumericalRelationship m_relationship;
    [SerializeField]private float targetValue;//目标值
    
    public override bool SwitchJudge()
    {
        switch (m_relationship)
        {
            case FloatNumericalRelationship.Less:
                return currentValue < targetValue;
            case FloatNumericalRelationship.Greater:
                return currentValue > targetValue;
            default:
                return false;
        }
    }
}
