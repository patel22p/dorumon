using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;
public enum WindowEnum { TeamSelect }
public class ConnectionGui : Bs
{
    string ip = "127.0.0.1";

    public void OnGUI()
    {
        if (Screen.lockCursor) return;
        _Game.playerName = gui.TextField(_Game.playerName);
        ip = gui.TextField(ip);
        if (gui.Button("Connect"))
            Network.Connect(ip, port);
        if (gui.Button("Host"))
            Network.InitializeServer(6, port, !Network.HavePublicAddress());
    }

}
