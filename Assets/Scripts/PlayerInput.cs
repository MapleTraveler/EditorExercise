using System;
using UnityEngine;

public enum CallType
{
    Move,       // 移动
    JumpPress,  // 跳跃按下
    JumpRelease,// 跳跃释放
    Roll,
    LeftMousePress,
    LeftMouseRelease,
    RightMousePress,
    RightMouseRelease,
    Item1Press,
    Item2Press
}

public struct InputData
{
    public Vector2 movementInput;
    public double jumpHoldTime;
    public bool jumpInput;
    public bool rollInput;
    public bool leftMouseButtonInput;
    public bool rightMouseButtonInput;
    public bool item1Input;
    public bool item2Input;

} 
public class PlayerInput
{
    public event Action<CallType, InputData> OnCommandIssue;
    
    PlayerInputActions inputActions;
    private Vector2 playerMovementInput;
    
    private double jumpHoldTime;
    
    
    private bool jumpInput;
    private bool rollInput;
    
    private bool leftMouseButtonInput;
    private bool rightMouseButtonInput;
    
    private bool item1Input;
    private bool item2Input;


    public PlayerInput()
    {
        Init();
        Debug.Log("玩家输入处理初始化成功");
    }
    private void Init()
    {
        inputActions ??= new PlayerInputActions();

        RegisterInputCallbacks();
        inputActions.Enable();
    }

    private void RegisterInputCallbacks()
    {
        inputActions.GamePlay.Move.performed += i =>
        {
            playerMovementInput = i.ReadValue<Vector2>();
            OnCommandIssue?.Invoke(CallType.Move, new InputData { movementInput = playerMovementInput });
        };
        inputActions.GamePlay.Move.canceled += i =>
        {
            playerMovementInput = Vector2.zero;
            OnCommandIssue?.Invoke(CallType.Move, new InputData { movementInput = Vector2.zero });
        };

        
        inputActions.GamePlay.Jump.performed += i =>
        {
            // TODO 如何获取按住跳跃的时间？是否需要实时发送？
            jumpHoldTime = i.duration;
            jumpInput = i.ReadValueAsButton();
            OnCommandIssue?.Invoke(CallType.JumpPress, new InputData{});
        };
        inputActions.GamePlay.Jump.canceled += i =>
        {
            OnCommandIssue?.Invoke(CallType.JumpRelease,new InputData{});
        };

        
        // TODO 同理，鼠标的按住时间？
        inputActions.GamePlay.LeftHand.performed += i =>
        {
            leftMouseButtonInput = i.ReadValueAsButton();
            OnCommandIssue?.Invoke(CallType.LeftMousePress, new InputData{});
        };
        inputActions.GamePlay.LeftHand.canceled += i =>
        {
            leftMouseButtonInput = false;
            OnCommandIssue?.Invoke(CallType.LeftMouseRelease, new InputData{});
        };
        
        inputActions.GamePlay.RightHand.performed += i =>
        {
            rightMouseButtonInput = i.ReadValueAsButton();
            OnCommandIssue?.Invoke(CallType.RightMousePress, new InputData{});
        };
        inputActions.GamePlay.RightHand.canceled += i =>
        {
            rightMouseButtonInput = false;
            OnCommandIssue?.Invoke(CallType.RightMouseRelease, new InputData{});
        };

        inputActions.GamePlay.Roll.performed += i =>
        {
            rollInput = i.ReadValueAsButton();
            OnCommandIssue?.Invoke(CallType.Roll, new InputData{});
        };
        
        inputActions.GamePlay.Item1.performed += i =>
        {
            item1Input = i.ReadValueAsButton();
            OnCommandIssue?.Invoke(CallType.Item1Press, new InputData{});
        };
        // inputActions.GamePlay.Item1.canceled += i =>
        // {
        //     item1Input = false;
        // };
        
        inputActions.GamePlay.Item2.performed += i =>
        {
            item2Input = i.ReadValueAsButton();
            OnCommandIssue?.Invoke(CallType.Item2Press, new InputData{});
        };
        // inputActions.GamePlay.Item2.canceled += i =>
        // {
        //     item2Input = false;
        // };
    }
     
    
    public void Disable()
    {
        inputActions.Disable();
    }
    
}