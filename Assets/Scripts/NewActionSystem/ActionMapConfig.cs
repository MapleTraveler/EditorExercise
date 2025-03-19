using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Input/Action Map")]
public class ActionMapConfig : ScriptableObject
{
    [Serializable]
    public struct ActionMapping
    {
        public ActionType actionType;
        public ActionBase action;
    }  
    public List<ActionMapping> actionMappings = new();
}