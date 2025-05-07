using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

enum PressKey
{
    A = 1 << 30,
    D = 1 << 29,
    Space = 1 << 28,
    Q = 1 << 26,
    E = 1 << 25,
    LeftShift = 1 << 27,
    LeftMouseButton = 1 << 24,
    RightMouseButton = 1 << 23
}

[CreateAssetMenu(fileName = "InputCondition",menuName = "SwitchCondition/InputCondition")]
public class PlayerInputCondition : ConditionValueBase
{
    private int curInput => ConditionManager.Instance.curInputData;
    [SerializeField] private bool isNone;
    [SerializeField] private List<PressKey> keyDetections;
    public override bool SwitchJudge()
    {
        if (isNone)
        {
            Debug.Log($"判定None的结果和输入{curInput == 0},{curInput}");
            return curInput == 0;
        }
        foreach (var key in keyDetections)
        {
            Debug.Log($"判定正常输入的结果和输入{((int)key & curInput) != 0},{curInput},{key}");
            if (((int)key & curInput) == 0)
            {
                return false;
            }
        }

        return true;
    }

    
}
