using System;
using UnityEngine;

public class GameGui:bs
{
    [FindTransform]
    public GUIText info;
    [FindTransform]
    public GUIText scores;
    [FindTransform]
    public GUIText time;
    [FindTransform]
    public GUIText CenterTime;
    public void WriteInfo(string s)
    {
        info.text += s + "\r\n";
    }
    public void Update()
    {
        info.text = "";        
        
    }
}