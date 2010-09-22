using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using doru;
using System.IO;
using System.Xml.Serialization;

public class SrvData
{
    public string[] ips;
    public GameMode gamemode = GameMode.DeathMatch;
    public GameLevels level = GameLevels.z4game;
    public int frags = 20;
}

public enum GameLevels : int { z4game, z5castle }
public enum GameMode { DeathMatch, TeamDeathMatch, TeamZombieSurvive, ZombieSurive }
public class HostWindow : WindowBase
{    
    
    
    public SrvData srvdata = new SrvData();
    internal GameMode gameMode { get { return srvdata.gamemode; } set { srvdata.gamemode=value; } }

    internal int fraglimit { get { return srvdata.frags; } set { srvdata.frags = value; } }
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
    
   
    
    public XmlSerializer xml = new XmlSerializer(typeof(SrvData), new Type[] { typeof(GameMode), typeof(GameLevels) });
    public void InitServer()
    {
        StringWriter sw = new StringWriter();        
        xml.Serialize(sw, srvdata);
        Network.useNat = false;
        Network.InitializeServer(32, _menu.port);
        if (_menu.masterip != "") MasterServer.ipAddress = _menu.masterip;
        MasterServer.RegisterHost(_menu.gamename, Application.loadedLevelName + lc.version.ToString() + version, sw.ToString());
    }

    public GameLevels selectedlevel { get { return srvdata.level; } set { srvdata.level = value; } }
    protected override void Window(int id)
    {
        selectedlevel = (GameLevels)GUILayout.Toolbar((int)selectedlevel, Enum.GetNames(typeof(GameLevels)));
        gameMode = (GameMode)GUILayout.Toolbar((int)gameMode, new string[] { lc.deathmatch.ToString(), lc.teamdeathmatch.ToString(), lc.teamzombiesurive.ToString(), lc.zombisr.ToString() });
        GUILayout.BeginHorizontal();
        GUILayout.Label(lc.zombiefrag.ToString(), GUILayout.ExpandWidth(false));
        int.TryParse(GUILayout.TextField(fraglimit.ToString(), 2, GUILayout.Width(60)), out srvdata.frags);
        GUILayout.EndHorizontal();
        if (GUILayout.Button(lc.startgame.ToString())|| skip)
        {
            InitServer();
            enabled = false;
        }
        GUI.DragWindow();
    }
    

}
