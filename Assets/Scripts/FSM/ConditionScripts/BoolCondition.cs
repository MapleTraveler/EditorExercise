using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BoolCondition",menuName = "SwitchCondition/BoolCondition")]
public class BoolCondition : ConditionValueBase
{
    [SerializeField]public bool currentValue;//设定值
    [SerializeField]protected bool targetValue;//目标值
    
    public override bool SwitchJudge()
    {
       return currentValue == targetValue;
    }
}
