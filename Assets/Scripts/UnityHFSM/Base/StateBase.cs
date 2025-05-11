using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace UnityHFSM
{
    /// <summary>
    /// 所有状态的基类。
    /// </summary>
    public class StateBase<TStateId>//TODO:原项目这里实现了有关可视化的接口，暂时不要
    {
    
        public readonly bool needsExitTime;
        public readonly bool isGhostState;
        public TStateId name;

        public IStateTimingManager fsm;//上级状态机
        /// <summary>
        /// 初始化 StateBase 类的新实例。
        /// </summary>
        /// <param name="needsExitTime">
        ///		指定该状态在转换时是否允许立即退出（false），
        ///		如果为 true，状态机将在状态准备好退出前等待。</param>
        /// <param name="isGhostState">
        ///		如果为 true，则该状态将变为“幽灵状态”，即状态机不会停留在此状态，
        ///		一旦进入该状态，会立即尝试所有可能的出边转换，而不是等到下一次 OnLogic 调用。</param>
        public StateBase(bool needsExitTime, bool isGhostState = false)
        {
            this.needsExitTime = needsExitTime;
            this.isGhostState = isGhostState;
        }

        /// <summary>
        /// 初始化状态，在 <c>name</c> 和 <c>fsm</c> 等字段赋值之后调用。
        /// </summary>
        public virtual void Init()
        {
        
        }
    
        /// <summary>
        /// 当状态机过渡到此状态（进入此状态）时调用。
        /// </summary>
        public virtual void OnEnter()
        {

        }

        /// <summary>
        /// 当该状态为活动状态时，由状态机的逻辑函数每帧调用的函数。
        /// </summary>
        public virtual void OnLogic()
        {
            
        }
    
        /// <summary>
        /// 当状态机从此状态过渡到另一状态（退出此状态）时调用。
        /// </summary>
        public virtual void OnExit()
        {

        }
        
    
        /// <summary>
        /// （仅当 <c>needsExitTime</c> 为 true 时才会调用）：
        ///		当从当前状态切换到其他状态的请求发生时，会调用此方法。
        ///		如果此状态已经可以退出，应立即调用 <c>fsm.StateCanExit()</c>。
        ///		如果此状态当前还不能退出（例如还在播放动画），
        ///		则应稍后（如在 <c>OnLogic()</c> 中）再调用 <c>fsm.StateCanExit()</c> 通知状态机。
        /// </summary>
        public virtual void OnExitRequest()
        {

        }
    
        /// <summary>
        /// 返回状态机层级中所有活动状态的路径字符串，
        /// 例如：<c>"/Move/Jump/Falling"</c>。
        /// 与状态机的 <c>ActiveStateName</c> 属性不同，它只返回当前活动状态的名称，
        /// 而不会包含嵌套状态的信息。
        /// </summary>
        /// <returns></returns>
        public virtual string GetActiveHierarchyPath()
        {
            return name.ToString();
        }
    
    }
    /// <inheritdoc />
    public class StateBase : StateBase<string>
    {
        /// <inheritdoc />
        public StateBase(bool needsExitTime, bool isGhostState = false)
            : base(needsExitTime, isGhostState)
        {
        }
    }
}


