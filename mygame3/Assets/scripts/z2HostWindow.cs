using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using doru;
using System.IO;
using System.Xml.Serialization;



public enum GameLevels : int { zDemo }//z4game, z5castle }
public enum GameMode { DeathMatch, TeamDeathMatch, TeamZombieSurvive, ZombieSurive }
public class z2HostWindow : WindowBase
{

    internal GameMode gameMode = GameMode.ZombieSurive;
    internal GameLevels selectedlevel;


    internal int fraglimit=20;
    protected override void Awake()
    {
        base.Awake();
    }
    
    void Start()
    {
        
        _hw = this;
        enabled = false;
        
        size = new Vector2(700, 100);
        title = lc.hostwind.ToString();
    }
               
    public void InitServer()
    {
        print("init Server");
        Network.InitializeServer(32, _menu.port,false);
        if (_menu.masterip != "") MasterServer.ipAddress = _menu.masterip;
        RegisterHost();
    }

    public static void RegisterHost()
    {
        MasterServer.RegisterHost(_menu.gamename, z2Menu.Nick + "'s Game" + lc.version.ToString() + version);
    }

    
    protected override void Window(int id)
    {
        
        GUILayout.Label(lc.map.ToString());
        selectedlevel = (GameLevels)GUILayout.Toolbar((int)selectedlevel, Enum.GetNames(typeof(GameLevels)));
        GUILayout.Space(10);
        GUILayout.Label(lc.mod.ToString());
        gameMode = (GameMode)GUILayout.Toolbar((int)gameMode, new string[] { lc.deathmatch.ToString(), lc.teamdeathmatch.ToString(), lc.teamzombiesurive.ToString(), lc.zombisr.ToString() });
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label(lc.zombiefrag.ToString(), GUILayout.ExpandWidth(false));
        int.TryParse(GUILayout.TextField(fraglimit.ToString(), 2, GUILayout.Width(60)), out fraglimit);
        GUILayout.EndHorizontal();
        if (GUILayout.Button(lc.startgame.ToString())|| skip)
        {
            InitServer();
            enabled = false;
        }
        GUI.DragWindow();
    }
    

}
