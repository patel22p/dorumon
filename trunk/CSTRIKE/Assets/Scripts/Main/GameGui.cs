using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;

public class GameGui : Bs {

	void Start () {
	
	}
	
    string ip = "127.0.0.1";
    void OnGUI()
    {
        gui.TextField(ip);
        if (gui.Button("Connect"))
            Network.Connect(ip, 80);
        if (gui.Button("Host"))
            Network.InitializeServer(6, 80, !Network.HavePublicAddress());
        
    }
}
