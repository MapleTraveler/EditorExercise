using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CanBeSeletedState
{
    PlayerState_Idle,PlayerState_Move,PlayerState_Jump,PlayerState_OnSky
}
[CreateAssetMenu(fileName = "PlayerStateCondition",menuName = "SwitchCondition/PlayerStateCondition")]
public class PlayerStateCondition : ConditionValueBase
{
    //暂定不在某些状态
    private Type curStateType => ConditionManager.Instance.curState.GetType();
    [SerializeField]public List<CanBeSeletedState> configStates;
    public override bool SwitchJudge()
    {
        foreach (var configState in configStates)
        {
            Type stateType = EnumMappingState(configState);
            if(stateType == null) continue;
            if (stateType == curStateType)
            {
                return false;
            }
        }
        return true;
    }

    private Type EnumMappingState(CanBeSeletedState state)
    {
        switch (state)
        {
            case CanBeSeletedState.PlayerState_Idle:
                return typeof(IdleState);
            case CanBeSeletedState.PlayerState_Move:
                return typeof(PlayerState_Move);
            case CanBeSeletedState.PlayerState_Jump:
                return typeof(PlayerState_StartJump);
            case CanBeSeletedState.PlayerState_OnSky:
                return typeof(PlayerState_OnSky);
            default:
                Debug.LogWarning($"不存在该状态:{state}");
                return null;
        }
    }
}
