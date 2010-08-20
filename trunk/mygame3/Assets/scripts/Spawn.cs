using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;


public class Spawn : Base
{
    public bool zmatch;
    public Transform _Player;
    public Transform Zombie;
    public List<IPlayer> iplayers = new List<IPlayer>();
    public List<Zombie> zombies = new List<Zombie>();
    public List<Base> dynamic = new List<Base>();

    
    void Awake()
    {
        _Spawn = this;
    }
    void Start()
    {

        AudioListener.volume = .1f;
        if (Network.isServer)
        {            
            Network.incomingPassword = "started";
            MasterServer.UnregisterHost();
        }


    }
    public Transform effects;
    internal int ZombieStageLimit = 20;
    int zombilesleft = 5;
    float ZombieSpeed = 2;
    float waittime = 2;
    internal int stage = 1;
    
    
    [RPC]
    void Update()
    {
        if (zmatch)
        {
            waittime -= Time.deltaTime;
            if (waittime < 0 && _localiplayer != null && Network.isServer)
            {
                if (_TimerA.TimeElapsed(2000) && zombilesleft != 0)
                {
                    Transform zsp = transform.Find("zsp");
                    Transform a = zsp.GetChild(UnityEngine.Random.Range(0, zsp.GetChildCount() - 1));
                    CreateZombie(a.position, a.rotation, 5 + ZombieSpeed * stage + UnityEngine.Random.Range(-2 * stage, 2 * stage), Network.AllocateViewID());
                    zombilesleft--;
                }
                if (zombilesleft == 0 && zombies.Count == 0)
                {
                    waittime = 2;
                    zombilesleft = ZombieStageLimit;
                    stage++;
                }
            }
        }
    }
    [RPC]
    public void CreateZombie(Vector3 p, Quaternion r, float zombiespeed, NetworkViewID nwid)
    {        
        CallRPC(true, p, r, zombiespeed,nwid);
        Zombie z = ((Transform)Instantiate(Zombie, p, r)).GetComponent<Zombie>();        
        z.networkView.viewID =nwid; 
        z.speed = zombiespeed;
    }
    public Dictionary<NetworkPlayer, Player> players = new Dictionary<NetworkPlayer, Player>();
    
    public void OnTeamSelect(Team team)
    {

        if (Network.peerType == NetworkPeerType.Disconnected)
            Network.InitializeServer(32, 5300);
        Transform t;
        if (_LocalPlayer == null)
            t = (Transform)Network.Instantiate(_Player, Vector3.zero, Quaternion.identity, (int)Group.Player);
        else
            t = _LocalPlayer.transform;
        t.GetComponent<Player>().RPCSetTeam((int)team);

    }
    
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        foreach (box a in GameObject.FindObjectsOfType(typeof(box)))
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
        CallRPC(false, v);
        NetworkView nw = NetworkView.Find(v);
        nw.viewID = NetworkViewID.unassigned;
        nw.enabled = false;
        Component.Destroy(nw);
    }

}
public enum Group { PlView, RPCSetID, Default, RPCAssignID, Life, Spawn, Nick, SetOwner, SetMovement, Player, Zombie }