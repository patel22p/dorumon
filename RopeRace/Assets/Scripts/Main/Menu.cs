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
        InitLoader();        
        _Menu.RefreshServerList();
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
        UpdateOther();
        timer.Update();
    }

    public void RefreshServerList()
    {
        MasterServer.RequestHostList(_Menu.guibuild.text);
    }
    private void UpdateOther()
    {

        if (Input.GetKeyDown(KeyCode.Return))
            _MenuGui.enabled = !_MenuGui.enabled;   
    }

    public void Action(MenuAction a)
    {
        Debug.Log("Action:" + a);
        if (a == MenuAction.StartServer)
        {
            _Popup.ShowPopup("Loading Map");
            HostGame();
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