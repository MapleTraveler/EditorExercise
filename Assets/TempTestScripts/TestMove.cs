using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class TestMove : MonoBehaviour
{
    // 暴露与原始代码完全相同的参数
    [SerializeField] float moveSpeed = 5f; 
    [SerializeField] float speedChangeRate = 10f;
    
    private Rigidbody2D rb2d;
    private float _speed; // 保持与原始代码相同的私有变量

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        // 旧输入系统获取输入（严格模拟原框架输入）
        float horizontal = Input.GetAxisRaw("Horizontal");
        Vector2 input = new Vector2(horizontal, 0);

        // 直接调用原始Move方法
        Move(input);
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
}
