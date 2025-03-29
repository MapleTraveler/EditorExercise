using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFrameworkManager : MonoBehaviour
{
    public CommandMapConfig commandMapConfig;
    public RequestMapConfig requestMapConfig;
    public ActionMapConfig actionMapConfig;
    
    public Animator animator;
    
    private PlayerInput _playerInput;
    private InputHandler _inputHandler;
    private RequestHandlerBase _requestHandler;
    private RequestReceiverBase _requestReceiver;
    private BehaviourBase _behaviour;
    private IStateHandler _stateHandler;
    private void Awake()
    {
        InitActionSystem();
    }


    private void InitActionSystem()
    {
        _playerInput =  new PlayerInput();
        _stateHandler = new StateHandlerBase(animator);
        _behaviour = new BehaviourBase(_stateHandler,actionMapConfig);
        _requestHandler = new RequestHandlerTest();
        _requestReceiver = new RequestReceiverTest(_requestHandler,_behaviour,requestMapConfig);
        _inputHandler = new InputHandler(_playerInput,_requestReceiver,commandMapConfig);
        Debug.Log("框架初始化成功");
    }

    private void Update()
    {
        _playerInput.Update();
        _requestHandler.Update();
        _behaviour.Update();
    }

    private void FixedUpdate()
    {
        _playerInput.FixedUpdate();
        _requestHandler.FixedUpdate();
        _behaviour.FixedUpdate();
    }

    private void LateUpdate()
    {
        _behaviour.LateUpdate();
    }
}
