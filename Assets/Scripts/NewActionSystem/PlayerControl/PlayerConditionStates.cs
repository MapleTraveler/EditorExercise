using System;
using UnityEngine;

public class PlayerConditionStates : MonoBehaviour
{
    public static PlayerConditionStates Instance;
    //通用组件
    [SerializeField] private Rigidbody2D playerRb2D;
    
    // 地面检测
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private Vector2 groundCheckOffset;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        groundCheckPoint = transform;
    }

    public bool PlayerIsOnGround()
    {
        return Physics2D.OverlapCircle((Vector2)groundCheckPoint.position + groundCheckOffset, groundCheckRadius, groundLayer) && playerRb2D.velocity.y < 1f;
    }
    
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        

        // 绘制射线盒运动的完整范围
        Gizmos.DrawWireSphere((Vector2)groundCheckPoint.position + groundCheckOffset,groundCheckRadius);
    }
    
    
    
}