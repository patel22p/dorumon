using System.Linq;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using doru;
//using System.Net;

public class Menu : bs
{


    TimerA timer = new TimerA();
    public GUIText guibuild;
    public void Start()
    {
        _Loader.EditorTest = false;
        string[] levels = new string[Application.levelCount-1];
        for (int i = 0; i < Application.levelCount - 1; i++)
            levels[i] = "Level " + (i + 1);
        _MyGui.levels = levels;
        _MyGui.curwindow = Wind.Menu;
        _MyGui.enabled = false;
    }
    
    public override void OnEditorGui()
    {
        guibuild.text = "Version Build:" + DateTime.Now.ToShortDateString();
        base.OnEditorGui();
    }
    void Update()
    {
        UpdateHostList();
        UpdateOther();
        timer.Update();
    }
    void UpdateHostList()
    {        
        if (MasterServer.PollHostList().Length != 0 && joingame)
        {
            joingame = false;
            timer.Clear();
            _MyGui.Show(Wind.PopUp);
            _MyGui.popupTitle = "Searching for games";
            Debug.Log("Host List Received");
            HostData[] hostData = MasterServer.PollHostList();
            string[] ips = hostData.SelectMany(a => a.ip).ToArray();
            _MyGui.popupText = "Server List Received " + hostData.Length + ", Connecting";
            Network.Connect(ips, 5300);
            for (int i = 0; i < hostData.Length; i++)
            {
                _MyGui.popupText = "Trying Connect to " + hostData[i].gameName;
                Network.Connect(hostData[i]);
            }
            MasterServer.ClearHostList();
            
        }
    }
    private void UpdateOther()
    {

        if (Input.GetKeyDown(KeyCode.Space))
            _MyGui.enabled = !_MyGui.enabled;   
    }

    bool joingame;
    public void Action(MenuAction a)
    {
        Debug.Log("Action:" + a);
        if (a == MenuAction.StartServer)
        {
            HostGame();
        }
        if (a == MenuAction.JoinGame)
        {
            SetTimeOut();
            joingame = true;
            MasterServer.RequestHostList("Ropector");
            _MyGui.popupTitle = "Searching for games";
            _MyGui.popupText = "Searching for games";
            _MyGui.Show(Wind.PopUp);
            LocalConnect();            
        }
    }

    

    private void HostGame()
    {
        bool useNat = !Network.HavePublicAddress();
        Network.InitializeServer(8, 5300, useNat);
        MasterServer.RegisterHost("Ropector " + guibuild, _Loader.nick + "'s game", "Level " + _MyGui.SelectedLevel);
        _Loader.LoadLevel(_MyGui.SelectedLevel+1);
    }

    private void SetTimeOut()
    {
        timer.AddMethod(15000, delegate { _MyGui.popupText = "connection time out"; timer.AddMethod(1000, delegate { _MyGui.Show(Wind.Menu); }); });
    }
    

}