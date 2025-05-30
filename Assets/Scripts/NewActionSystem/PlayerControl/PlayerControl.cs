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
    private float _verticalVelocity;
    
    private float _elapsedTime = 0f;
    private float _startSpeed = 0f;
    private float _accelDuration = 0.5f; // 加速时间，可调整
    private bool _isAccelerating = false;

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
        float currentSpeed = Math.Abs(rb2d.velocity.x) ;
        if (Mathf.Abs(currentSpeed - targetSpeed) > 0.2f)
        {
            _speed = Mathf.Lerp(currentSpeed, targetSpeed, Mathf.Clamp01(Time.fixedDeltaTime * speedChangeRate));
            _speed = Mathf.Round(_speed * 1000f) / 1000f; // 四舍五入到3位小数
            //Debug.Log($"currentSpeed:{currentSpeed}");
            //Debug.Log($"targetSpeed:{targetSpeed}");
        }
        else
        {
            _speed = targetSpeed;
        }

        // 计算最终移动方向
        float xMoveSpd = movement.x * _speed;
        rb2d.velocity = new Vector2(xMoveSpd, rb2d.velocity.y);

        // 控制角色朝向（仅左右翻转）
        if (movement.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(movement.x), 1, 1);
        }
        
    }

    public void Jump()
    {
        // 下落速度逻辑？
        if (_verticalVelocity < 0.0f)
        {
            _verticalVelocity = -2f;
        }
    }
    
    
    
}
