using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;

public class GameGui : Bs {

	void Start () {
	
	}
	
    string ip = "127.0.0.1";
    
    void OnGUI()
    {    
        if (!Screen.lockCursor)
        {
            _Game.playerName = gui.TextField(_Game.playerName);
            ip=gui.TextField(ip);
            if (gui.Button("Connect"))
                Network.Connect(ip, port);
            if (gui.Button("Host"))
                Network.InitializeServer(6, port, !Network.HavePublicAddress());
            if (gui.Button("Play Offline"))
                Screen.lockCursor = true;
            
        }        

    }
}
