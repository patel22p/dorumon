using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;


public class Spawn : Base
{
    
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
        zombiesleft = _Loader.fraglimit;
        AudioListener.volume = .1f;
        if (Network.isServer)
        {            
            Network.incomingPassword = "started";
            MasterServer.UnregisterHost();
        }


    }
    public Transform effects;
    internal int zombiesleft;
    float ZombieSpeed = 1;    
    internal int stage = 1;
    bool wait;  
    void Update()
    {
        if (zombi)
        {            
            if (players.Count != 0 && Network.isServer)
            {
                if (_TimerA.TimeElapsed(2000) && zombiesleft != 0)
                {
                    Transform zsp = transform.Find("zsp");
                    Transform a = zsp.GetChild(UnityEngine.Random.Range(0, zsp.GetChildCount() - 1));
                    CreateZombie(a.position);                    
                    zombiesleft--;
                }
                if (zombiesleft == 0 && zombies.Count == 0 && !wait)
                {
                    wait = true;
                    _TimerA.AddMethod(5000, RPCNextStage);                    
                }                
                
            }            
        }
    }

    private void CreateZombie(Vector3 a)
    {
        
        Zombie z = ((Transform)Network.Instantiate(Zombie, a, Quaternion.identity, (int)Group.Zombie)).GetComponent<Zombie>();
        z.RPCSetup(5 + ZombieSpeed * stage + UnityEngine.Random.Range(-ZombieSpeed * stage, ZombieSpeed * stage),
            100 + 10 * stage);
        wait = false;
    }
    [RPC]
    private void RPCNextStage()
    {
        
        CallRPC(true);
        
        stage++;
        foreach (Zombie z in GameObject.FindObjectsOfType(typeof(Zombie)))
        {            
            Vector3 pos = z.transform.position;
            z.Destroy();
            if(Network.isServer)
                CreateZombie(pos);
        }
        foreach (Player p in players.Values)
            if (p.dead)
                p.RPCSpawn();
    }
    
    public new Dictionary<int, Player> players = new Dictionary<int, Player>();
    
    public void OnTeamSelect(Team team)
    {
        lockCursor = true;
        if (Network.peerType == NetworkPeerType.Disconnected)
            Network.InitializeServer(32, 5300);
        Transform t;
        if (_LocalPlayer == null)
            t = (Transform)Network.Instantiate(_Player, Vector3.zero, Quaternion.identity, (int)Group.Player);
        else
            t = _LocalPlayer.transform;
        if (tdm || zombi)
            t.GetComponent<Player>().RPCSetTeam((int)team);

    }
    internal void Spectator()
    {
        if (_LocalPlayer.car == null)
        {
            lockCursor = true;
            _LocalPlayer.RPCShow(false);            
        }
    }


    void OnPlayerDisconnected(NetworkPlayer player2)
    {
        int player = player2.GetHashCode();
        players[player].Destroy();
        foreach (box a in GameObject.FindObjectsOfType(typeof(box)))
        {
            if (a.OwnerID == player)
                a.RPCResetOwner();

            foreach (NetworkView v in a.GetComponents<NetworkView>())
                if (v.owner.GetHashCode() == player) Destroy(v.viewID);
        }        
        Network.DestroyPlayerObjects(player2);
        Network.RemoveRPCs(player2);
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