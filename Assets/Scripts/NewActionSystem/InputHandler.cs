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
    }
   

    public void TrySendRequest(CallType callType,InputData data = new InputData())
    {
        
        foreach (var mapping in _config.commandMappings)
        {
            if (mapping.callType == callType)
            {
                
               requestReceiver.ReceiverRequest((int)mapping.requestType,data);
                
            }
        }
        
        
    }
}