using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BehaviorTree
{
    public enum BehaviourState
    {
        未执行,成功,失败,执行中
    }

    public enum NodeType
    {
        无,根节点,组合节点,条件节点,行为节点
    }
    #region 根数据
    public abstract class BtNodeBase
    {
        [FoldoutGroup("@NodeName"),LabelText("名称")]
        public string NodeName;
        [ReadOnly,FoldoutGroup("@NodeName"),LabelText("标识")]
        public string Guid;
        [ReadOnly,FoldoutGroup("@NodeName"),LabelText("位置")]
        public Vector2 Position;
        [ReadOnly,FoldoutGroup("@NodeName"),LabelText("类型")]
        public NodeType NodeType;
        [ReadOnly,FoldoutGroup("@NodeName"),LabelText("状态")]
        public BehaviourState NodeState;

        public abstract BehaviourState Tick();
    }

    public abstract class BtComposite : BtNodeBase
    {
        [FoldoutGroup("@NodeName"),LabelText("子节点")]
        public List<BtNodeBase> ChildrenNodes = new List<BtNodeBase>();
    }

    public abstract class BtPrecondition : BtNodeBase
    {
        [FoldoutGroup("@NodeName"), LabelText("子节点")]
        public BtNodeBase ChildNode;
    }
    public abstract class BtActionNode : BtNodeBase { }
    #endregion

    
    /// <summary>
    /// 顺序执行节点
    /// </summary>
    public class Sequence : BtComposite
    {
        [LabelText("执行index"), FoldoutGroup("@NodeName")]
        public int CurrentNode;
        public override BehaviourState Tick()
        {
            if (ChildrenNodes.Count == 0)
            {
                NodeState = BehaviourState.失败;
                return BehaviourState.失败;
            }
            
            var state = ChildrenNodes[CurrentNode].Tick();
            switch (state)
            {
                case BehaviourState.成功:
                    CurrentNode++;
                    if (CurrentNode > ChildrenNodes.Count - 1)
                    {
                        CurrentNode = 0;
                        return BehaviourState.成功;
                    }
                    return BehaviourState.执行中;
                case BehaviourState.失败:
                    CurrentNode = 0;
                    return BehaviourState.失败;
                case BehaviourState.执行中:
                    return state;
               
            }

            return BehaviourState.未执行;
        }
    }
    
    /// <summary>
    /// 选择节点
    /// </summary>
    public class Selector : BtComposite
    {
        [FoldoutGroup("@NodeName"),LabelText("选择的index")]
        public int CurrentNode;
        public override BehaviourState Tick()
        {
            var state = ChildrenNodes[CurrentNode].Tick();
            switch (state)
            {
                case BehaviourState.成功:
                    CurrentNode = 0;
                    return state;
                case BehaviourState.失败:
                    CurrentNode++;
                    if (CurrentNode > ChildrenNodes.Count - 1)
                    {
                        CurrentNode = 0;//如果失败，从子集第一个节点重新判断，如果不加，则从失败的地方继续尝试执行
                        return BehaviourState.失败;
                    }
                    break;
                default:
                    return state;
                
            }

            return BehaviourState.失败;
        }
    }
    
    
    
    
    
    
    /// <summary>
    /// 延时执行节点
    /// </summary>
    public class Delay : BtPrecondition
    {
        [LabelText("延时"), SerializeField,FoldoutGroup("@NodeName")] 
        private float timer;
        
        private float currentTime;

        public override BehaviourState Tick()
        {
            currentTime += Time.deltaTime;
            if (currentTime >= timer)
            {
                currentTime = 0f;
                ChildNode.Tick();
                return NodeState = BehaviourState.成功;
            }
            return NodeState = BehaviourState.执行中;
        }
    }
    /// <summary>
    /// 条件节点
    /// </summary>
    public class ConditionalJudgment : BtPrecondition
    {
        [FoldoutGroup("@NodeName"),LabelText("是否活动"),SerializeField]
        private bool isActive;
        public override BehaviourState Tick()
        {
            if (isActive)
            {
                return ChildNode.Tick();
            }

            return BehaviourState.失败;
        }
    }
    
    public class SetObjectActive : BtActionNode
    {
        [LabelText("是否启用"),SerializeField,FoldoutGroup("@NodeName")]
        private bool isActive;

        [LabelText("启用对象"), SerializeField, FoldoutGroup("@NodeName")]
        private GameObject particle;

        public override BehaviourState Tick()
        {
            particle.SetActive(isActive);
            Debug.Log($"{NodeName}节点{(isActive ? "启用了" : "禁用了")}");
            return BehaviourState.成功;
        }
    }
}
