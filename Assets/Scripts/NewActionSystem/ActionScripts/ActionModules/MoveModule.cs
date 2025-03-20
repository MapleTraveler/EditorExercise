using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "ActionModule/MoveModule")]
public class MoveModule : ActionModule
{
    public override void Execute()
    {
        
    }

    public override void Execute(InputData inputData)
    {
        PlayerControl.Instance.Move(inputData.movementInput);
    }
}
