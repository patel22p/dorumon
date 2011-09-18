using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;

public class LoaderGui : Bs
{
    const string csgame = "csgame";
    string ip = "127.0.0.1";
    string label = "";
    public override void Awake()
    {
        if (Application.loadedLevel != 0)
        {
            enabled = false;
            return;
        }
        Refresh();
        base.Awake();
    }
    
    public void OnGUI()
    {
        //todo map select
        var c = new Vector3(Screen.width, Screen.height) / 2f;
        var s = new Vector3(200, 400) / 2f;
        var v1 = c - s;
        var v2 = c + s;
        GUI.Window((int)WindowEnum.ConnectionGUI, Rect.MinMaxRect(v1.x, v1.y, v2.x, v2.y), ServerList, "Server List");
    }
    bool SelectMap;
    void ServerList(int id)
    {
        if (SelectMap)
        {
            if (gui.Button("<<Back"))
                SelectMap = false;
            foreach (var a in _Loader.maps)
                if (gui.Button(a))
                    Host(a);
        }
        else
        {
            if (Screen.lockCursor) return;
            gui.Label("Name:");
            _Loader.playerName = gui.TextField(_Loader.playerName);
            gui.Label("Ip Address:");
            ip = gui.TextField(ip);
            gui.BeginHorizontal();
            if (gui.Button("Connect"))
                Network.Connect(ip, port);
            if (gui.Button("Host"))
                SelectMap = true;
            if (gui.Button("Refresh"))
                Refresh();
            gui.EndHorizontal();
            gui.Label(label);
            foreach (HostData host in MasterServer.PollHostList())
                if (gui.Button("Join to " + host.gameName + (host.useNat ? "(NAT)" : "")))
                {
                    Print("Trying Connect to " + host.gameName);
                    var er = Network.Connect(host);
                    if (er != NetworkConnectionError.NoError)
                        Print(er + "");
                }
        }
    }

    private void Host(string mapName)
    {
        Print("Loading Map");
        SelectMap = false;
        Network.InitializeServer(6, port, !Network.HavePublicAddress());
        MasterServer.RegisterHost(csgame, _Loader.playerName + "'s game");
        _Loader.LoadLevel(mapName);
    }

    private void Refresh()
    {
        MasterServer.ClearHostList();
        MasterServer.RequestHostList(csgame);
    }


    public void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        Print(msEvent + "");
    }
    public void OnFailedToConnectToMasterServer(NetworkConnectionError info)
    {
        Print(info + "");
    }

    public void Print(string text)
    {
        label = text;
    }
}
