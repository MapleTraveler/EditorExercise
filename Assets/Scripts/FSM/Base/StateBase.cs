using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

public abstract class StateBase : ScriptableObject
{
    protected StateMachine m_StateMachine; //上级状态机
    protected GameObject m_player;//玩家
    [SerializeField] public List<StateSwitchNode> switchNodes;//关系转换表 
    
    public virtual void Enter(){}//进入 状态or状态机 时的执行
/// <summary>
/// 逻辑执行
/// </summary>
    public void LogicExecute()
    {
        BeforeLogicExecute();
        OnLogicExecute();
        AfterLogicExecute();
    }
/// <summary>
/// 物理执行
/// </summary>
    public void PhysicsExecute()
    {
        BeforePhysicsExecute();
        OnPhysicsExecute();
        AfterPhysicsExecute();
    }
    public virtual void Exit(){}

    //初始化，同时初始化切换节点
    public virtual void Initialize(StateMachine stateMachine, GameObject player)
    {
        m_StateMachine = stateMachine;
        m_player = player;
        foreach (var switchNode in switchNodes)
        {
            switchNode.Init();
        }
    }
    protected virtual void BeforeLogicExecute(){}
    protected virtual void OnLogicExecute(){}
    protected virtual void AfterLogicExecute(){}
    
    protected virtual void BeforePhysicsExecute(){}
    protected virtual void OnPhysicsExecute(){}
    protected virtual void AfterPhysicsExecute(){}
    
}
