using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Input/Request Map")]
public class RequestMapConfig : ScriptableObject
{
   [Serializable]
   public struct RequestConfig
   {
      public RequestType requestType;
      public RequestBase requestBase;
   }
   
   public List<RequestConfig> requestMappings = new();
}
