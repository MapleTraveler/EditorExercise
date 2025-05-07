using UnityEngine;

public class InputHandler
{
    readonly PlayerInput playerInput;
    readonly IRequestReceiver requestReceiver;
    
    private readonly CommandMapConfig _config;
    
    public InputHandler(PlayerInput playerInput,IRequestReceiver requestReceiver, CommandMapConfig config)
    {
        this.playerInput = playerInput;
        this.requestReceiver = requestReceiver;
        _config = config;
        this.playerInput.OnCommandIssue += TrySendRequest;
        Debug.Log("输入处理层初始化完成");
    }
   

    public void TrySendRequest(CallType callType,InputData data = new InputData())
    {
        //Debug.Log("发送指令");
        foreach (var mapping in _config.commandMappings)
        {
            if (mapping.callType == callType)
            {
                
               requestReceiver.ReceiverRequest(mapping.requestType,data);
               //Debug.Log($"发送{callType}指令");
            }
        }
        
    }

    public void SendInputToRequestLevel()
    {
        
    }
    public void SendInputToRequestLevel<TData>(TData data)
    {
        
    }
}