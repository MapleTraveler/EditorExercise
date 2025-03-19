using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConditionValueBase : ScriptableObject
{
    [SerializeField]private string conditionName;
    public abstract bool SwitchJudge();
}
