using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    
    public static PlayerControl Instance;
    


    #region 角色组件

    public Rigidbody2D rb2d;

    #endregion

    #region 配置属性字段
    
    public float moveSpeed = 5f;
    [Tooltip("Acceleration and deceleration")]
    public float speedChangeRate = 10.0f;

    #endregion

    #region 内置属性

    // player
    private float _speed;
    

    #endregion
    
    
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }
    

    public void Move(Vector2 movement)
    {
        float targetSpeed = moveSpeed;
        if (movement == Vector2.zero) targetSpeed = 0.0f;

        // 平滑加速和减速
        float currentSpeed = rb2d.velocity.x;
        if (Mathf.Abs(currentSpeed - targetSpeed) > 0.1f)
        {
            _speed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f; // 四舍五入到3位小数
        }
        else
        {
            _speed = targetSpeed;
        }

        // 计算最终移动方向
        Vector2 moveDirection = movement.normalized * _speed;
        rb2d.velocity = moveDirection;

        // 控制角色朝向（仅左右翻转）
        if (movement.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(movement.x), 1, 1);
        }
        
    }

    public void Jump()
    {
        
    }
    
    
    
}
