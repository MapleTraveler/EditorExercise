using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInputData
{
    public static Keyboard keyboard;
    public static Mouse mouse;
    public static float keyA => -keyboard.aKey.ReadValue();//A 30
    public static float keyD => keyboard.dKey.ReadValue();//D 29
    public static bool isJump => keyboard.spaceKey.wasPressedThisFrame; //Space 28
    public static bool isRoll => keyboard.leftShiftKey.wasPressedThisFrame; //Shift 27
    public static bool item1 => keyboard.qKey.wasPressedThisFrame; // Q 26
    public static bool item2 => keyboard.eKey.wasPressedThisFrame; // E 25
    public static bool leftHand => mouse.leftButton.wasPressedThisFrame; //Left Mouse 24
    public static bool rightHand => mouse.rightButton.wasPressedThisFrame; //Right Mouse 23
    

    static PlayerInputData()
    {
        keyboard = Keyboard.current;
        mouse = Mouse.current;
    }
    

    public static int SentInputData()
    {
        int data = 0;
        //A，D按键输入映射
        if (keyA != 0)
            data |= 1 << 29;
        if (keyD != 0)
            data |= 1 << 30;
        //Space输入映射
        if (isJump)
            data |= 1 << 28;
        //Left Shift
        if (isRoll)
            data |= 1 << 27;
        //Q
        if (item1)
            data |= 1 << 26;
        //E
        if (item2)
            data |= 1 << 25;
        //Mouse Left
        if (leftHand)
            data |= 1 << 24;
        //Mouse Right
        if (rightHand)
            data |= 1 << 23;
        
        return data;
    }
}
