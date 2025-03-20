using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface IStateHandler{}

public class PlayerStateBase
{
    
}

public class StateHandlerBase : IStateHandler
{
    public int currentStateHash { get; protected set;}
    public int currentTagHash { get; protected set;}
    public bool isInTransition { get; private set;}

    protected List<PlayerStateBase> m_stateData;
    protected Dictionary<int ,Func<bool>> m_stateConitions;
    protected Animator m_anim;
    private bool m_isInit = false;



    public StateHandlerBase(Animator anim)
    {
        m_anim = anim;
        m_stateData = new List<PlayerStateBase>();
        m_stateConitions = new Dictionary<int, Func<bool>>();
        //读取配置
        // StateConfiguration();
        // StateConditionConfiguration();
        Init();
        Debug.Log("动画机初始化成功");
    }
    


    public T GetState<T>() where T : PlayerStateBase
    {
        for(int i = 0; i < m_stateData.Count;i++)
        {
            if(m_stateData[i].GetType() == typeof(T))
            {
                return (T)m_stateData[i];
            }

        }
        return null;
    }

    public Func<bool> GetStateConditions(int stateID)
    {
        return m_stateConitions[stateID];
    }

    //子类继承实现
    protected virtual void StateConfiguration(){}

    protected virtual void StateConditionConfiguration(){}



    protected virtual void OnInit(){}
    protected virtual void OnUpdate(){}
    protected virtual void OnFixedUpdate(){}

    private void UpdateAnimatorHash()
    {
        if(CheckAnimIsTransition())
        {
            isInTransition = true;
            return;
        }
        isInTransition = false;
        currentStateHash = m_anim.GetCurrentAnimatorStateInfo(0).shortNameHash;//使用Animator.StringToHash生成的哈希值，传递的字符串不包含父层的名字
        currentTagHash = m_anim.GetCurrentAnimatorStateInfo(0).tagHash;//该状态的标签
        
    }

    private bool CheckAnimIsTransition()
    {
        for(int i = 0;i < m_anim.layerCount;i++)
        {
            if(m_anim.IsInTransition(i)) return true;
        }
        return false;
    }

    private void Init()
    {
        m_isInit = true;
        //执行初始化逻辑
        OnInit();
    }
    public void Update() 
    {
        OnUpdate();    
    }

    public void FixedUpdate()
    {
        //处理动画
        UpdateAnimatorHash();
        OnFixedUpdate();
    }


}
