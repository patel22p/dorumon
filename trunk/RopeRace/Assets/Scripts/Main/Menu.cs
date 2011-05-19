using System.Linq;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using doru;


public class Menu : bs
{
    TimerA timer = new TimerA();
    public GUIText guibuild;
    public void Start()
    {
        
        _Loader.EditorTest = false;        
        _MenuGui.curwindow = Wind.Menu;
        _MenuGui.enabled = false;
        if (debug) _MenuGui.enabled = true;
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
            Debug.Log("Host List Received");
            HostData[] hostData = MasterServer.PollHostList();
            //string[] ips = hostData.SelectMany(a => a.ip).ToArray();
            _Popup.ShowPopup("Server List Received " + hostData.Length + ", Connecting");
            //Network.Connect(ips, 5300);
            for (int i = 0; i < hostData.Length; i++)
            {
                _Popup.ShowPopup("Trying Connect to " + hostData[i].gameName);
                Network.Connect(hostData[i]);
            }
            MasterServer.ClearHostList();
            
        }
    }
    private void UpdateOther()
    {

        if (Input.GetKeyDown(KeyCode.Space))
            _MenuGui.enabled = !_MenuGui.enabled;   
    }

    bool joingame;
    public void Action(MenuAction a)
    {
        Debug.Log("Action:" + a);
        if (a == MenuAction.StartServer)
        {
            _Popup.ShowPopup("Loading Map");
            HostGame();
        }
        if (a == MenuAction.JoinGame)
        {
            //SetTimeOut();
            joingame = true;            
            MasterServer.RequestHostList(guibuild.text);
            _Popup.ShowPopup("Searching for games");
            _Popup.enabled = true;
            LocalConnect();
        }
    }
    private void HostGame()
    {
        bool useNat = !Network.HavePublicAddress();
        Network.InitializeServer(8, 5300, useNat);
        MasterServer.RegisterHost(guibuild.text, _Loader.nick + "'s game", "Level " +  _MenuGui.SelectedLevel);
        _Loader.LoadLevel((int)_MenuGui.SelectedLevel);
    }


    

    //private void SetTimeOut()
    //{
        //timer.AddMethod(8000, delegate { _Popup.ShowPopup("connection time out"); timer.AddMethod(1000, delegate { _MyGui.Show(Wind.Menu); }); });
    //}
    

}