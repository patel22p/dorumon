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
        if (Game.Wall > 0)
            this.WriteInfo("B :Build wall " + Game.Wall);
        if (Game.WallDynamic > 0)
            this.WriteInfo("N :Build Dynamic wall " + Game.WallDynamic);
        if (Game.WallSticky > 0)
            this.WriteInfo("V :Build Sticky wall " + Game.WallSticky);
        if (Game.SecondRope)
            this.WriteInfo("RB :Use Second Rope");
        
    }
}