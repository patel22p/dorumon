using UnityEngine;
using System.Collections;
using System;

public class Spawn : Base
{
    public Transform _Player;


    void Start()
    {

        if (started || !levelLoaded) return;
        started = true;
        print("test");
        AudioListener.volume = .1f;
        if (Network.isServer)
        {
            Network.incomingPassword = "started";
            MasterServer.UnregisterHost();
        }
        
        
    }
    void OnGUI()
    {
        if (!teamselected)
            GUI.Window(4, CenterRect(.6f, .4f), TeamSelectWindow, "Team Select");
    }
    bool teamselected;
    public void TeamSelectWindow(int a)
    {
        if (teamselected) return;
        GUI.BringWindowToFront(a);
        bool ata, def;
        if ((ata = GUILayout.Button("ata")) || (def = GUILayout.Button("def")))
        {
            if (Network.peerType == NetworkPeerType.Disconnected)
            {
                Network.InitializeServer(32, 5300);
                levelLoaded = true;
                foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
                    go.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
            }
            Transform t = (Transform)Network.Instantiate(_Player, Vector3.zero, Quaternion.identity, (int)Group.Player);
            t.GetComponent<Player>().team = ata ? Team.ata : Team.def;
            teamselected = true;            
        }
        
    }
    

    void Update()
    {        
        _TimerA.Update();        
    }
    
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        foreach (CarController a in GameObject.FindObjectsOfType(typeof(CarController)))
        {
            if (a.OwnerID == player)
                a.RPCResetOwner();
            
            foreach (NetworkView v in a.GetComponents<NetworkView>())
                if (v.owner == player) Destroy(v.viewID);
        }

        Network.DestroyPlayerObjects(player);
        Network.RemoveRPCs(player);        
    }
    [RPC]
    private void Destroy(NetworkViewID v)
    {
        CallRPC(false,v);
        NetworkView nw = NetworkView.Find(v);
        nw.viewID = NetworkViewID.unassigned;
        nw.enabled = false;
        Component.Destroy(nw);
    }
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Base.levelLoaded = false;
    }    
}
public enum Group { PlView, RPCSetID, Default, RPCAssignID, Life, Spawn, Nick, SetOwner, SetMovement, Player }