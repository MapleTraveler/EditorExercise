using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "StateMachine",menuName = "PlayerStateMachine/StateMachine",order = 1)]
public class StateMachine: StateBase
{
    private Dictionary<Type, StateBase> statesTable;//状态字典表
    [SerializeField] private List<StateBase> needConfigStates;//外部配置状态表
    public StateBase currentStateBase;//目前的状态


    protected override void OnLogicExecute()
    {
        currentStateBase.LogicExecute();
    }

    protected override void OnPhysicsExecute()
    {
        currentStateBase.PhysicsExecute();
    }

    //切换状态
    public void SwitchState(Type nextStateType)
    {
        statesTable.TryGetValue(nextStateType, out var nextState);
        if (nextState != null)
        {
            currentStateBase.Exit();
            nextState.Enter();
            currentStateBase = nextState;
        }
    }

    //设置默认状态
    public void SetDefaultState()
    {
        currentStateBase = statesTable[typeof(IdleState)];
        currentStateBase.Enter();
    }
    //初始化
    public override void Initialize(StateMachine stateMachine, GameObject player)
    {
        base.Initialize(stateMachine,player);
        
        statesTable = new Dictionary<Type, StateBase>();
        foreach (var state in needConfigStates)
        {
            state.Initialize(this,m_player);
            statesTable.Add(state.GetType(),state);
        }
        //Debug.Log(needConfigStates.Count);
        SetDefaultState();
    }
}
