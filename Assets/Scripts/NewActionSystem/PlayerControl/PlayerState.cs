using UnityEngine;

public class PlayerState : PlayerStateBase
{
    public Vector3 moveDirection;
    public bool isGrounded;
    public bool isJumping;
    public bool isAttacking;
    public float moveSpeed;
    public float jumpForce;
    
    public PlayerState()
    {
        moveDirection = Vector3.zero;
        isGrounded = true;
        isJumping = false;
        isAttacking = false;
        moveSpeed = 5f;
        jumpForce = 5f;
    }
} 