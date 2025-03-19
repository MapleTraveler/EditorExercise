using UnityEngine;
[CreateAssetMenu(fileName = "GroundCheckCondition", menuName = "Conditions/Ground Check")]
public class GroundCheckCondition : ConditionBase
{
    public override bool Evaluate()
    {
        return PlayerConditionStates.Instance.PlayerIsOnGround();
    }
}