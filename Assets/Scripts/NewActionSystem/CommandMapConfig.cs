using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Input/Command Map")]
public class CommandMapConfig : ScriptableObject
{
   [Serializable]
   public struct CommandMapping
   {
      public CallType callType;
      public RequestType requestType;
   }
   
   public List<CommandMapping> commandMappings = new();
}
