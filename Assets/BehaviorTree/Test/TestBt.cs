using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorTree;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class TestBt : SerializedMonoBehaviour
{
    [OdinSerialize]
    public BtNodeBase RootNode;

    private void Update()
    {
        RootNode.Tick();
    }
}
