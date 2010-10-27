using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net;


public class z4Game : Base
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
    public AudioSource au;
    void OnPlayerConnected(NetworkPlayer p)
    {
        //if (_Level == Level.z4game)
        //{
        //    autostart = 3;
        //    _Loader.RPCLoadLevel(Level.z3labby.ToString(), RPCMode.All);            
        //}
    }
    void Start()
    {
        _cw.music.Start("music.txt");        
        //_cw.audio.clip = null;
        //_cw.music.enabled = false;
        //_Loader.audio.Stop();

        if (build)
        {
            
            //AudioClip a = (AudioClip)Resources.Load("centaspike1");
            //if (a != null)
            //    au.PlayOneShot(a);
            //else print("mp3 null");
        }
        localuser.frags = localuser.deaths = 0;
        _Vkontakte.enabled = _vk.enabled = false;
        zombiesleft = _hw.fraglimit;

        //if (Network.isServer)
        //{

        //    if (Network.connections.Length > 0)
        //    {
        //        print("unregistering host");
        //        MasterServer.UnregisterHost();
        //    }
        //    //Network.incomingPassword = "started";            
        //}
        if (skip)
            OnTeamSelect(Team.ata);
    }

    

    public Transform effects;
    internal int zombiesleft;
    float ZombieSpeed = 1;
    internal int stage = 1;
    bool wait;
    bool win;
    void CheckWin()
    {
        if (Network.isServer && _TimerA.TimeElapsed(1000) && !win)
        {
            if (dm)
                DMCheck();
            else if (zombi)
                ZombiTDMCheck();
            else if (tdm)
                TDMCheck();
            else if (zombisurive)
                ZombieSuriveCheck();
        }
    }

    private void ZombieSuriveCheck()
    {
        int live = 0;
        foreach (Player p in players.Values)
            if (!p.dead) live++;
        if (live == 0 && players.Values.Count != 0)
        {
            rpcwrite(String.Format(lc.udl.ToString(), stage));
            win = true;
            _TimerA.AddMethod(5000, WinGame);
        }
    }

    private void DMCheck()
    {
        foreach (Player pl in players.Values)
            if (pl.OwnerID != -1) 
            {
                if (Network.isServer && !win && pl.frags >= _hw.fraglimit)
                {
                    rpcwrite(pl.nick + " Win");
                    win = true;
                    _TimerA.AddMethod(5000, WinGame);
                }
            }
    }

    private void TDMCheck()
    {
        BlueFrags = RedFrags = 0;
        foreach (z0Vk.user pl in TP(Team.ata))
            RedFrags += pl.frags;

        foreach (z0Vk.user pl in TP(Team.def))
            BlueFrags += pl.frags;

        if ((BlueFrags >= _hw.fraglimit || RedFrags >= _hw.fraglimit))
        {
            rpcwrite((BlueFrags > RedFrags ? "Blue" : "Red") + " Team Win");
            win = true;
            _TimerA.AddMethod(5000, WinGame);
        }
    }

    private void ZombiTDMCheck()
    {
        bool BlueteamLive = false, RedteamLive = false;
        int rcount = 0, bcount = 0;
        foreach (Player p in TP2(Team.def))
        {
            rcount++;
            if (!p.dead) BlueteamLive = true;
        }
        foreach (Player p in TP2(Team.ata))
        {
            bcount++;
            if (!p.dead) RedteamLive = true;
        }

        if (Network.isServer && rcount > 0 && bcount > 0 && (!RedteamLive || !BlueteamLive))
        {
            rpcwrite((!RedteamLive ? "Blue" : "Red") + " Team Win");
            win = true;
            _TimerA.AddMethod(5000, WinGame);
        }
    }


    public int RedFrags = 0, BlueFrags = 0;

    void Update()
    {
        CheckWin();
        if (zombi || zombisurive)
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
    

    private void WinGame()
    {
    
        _Loader.RPCLoadLevel(Level.z3labby.ToString(),RPCMode.AllBuffered);
    }
    private void CreateZombie(Vector3 a)
    {

        Zombie z = ((Transform)Network.Instantiate(Zombie, a, Quaternion.identity, (int)Group.Zombie)).GetComponent<Zombie>();
        z.RPCSetup(5 + ZombieSpeed * stage + UnityEngine.Random.Range(-ZombieSpeed * stage, ZombieSpeed * stage), 100);
        wait = false;
    }
    [RPC]
    private void RPCNextStage()
    {

        CallRPC(false);

        stage++;
        foreach (Zombie z in GameObject.FindObjectsOfType(typeof(Zombie)))
        {
            Vector3 pos = z.transform.position;
            z.Destroy();
            if (Network.isServer)
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

        if (_LocalPlayer == null)
            Network.Instantiate(_Player, Vector3.zero, Quaternion.identity, (int)Group.Player);
        else if (!zombi && !zombisurive)
            _LocalPlayer.RPCSpawn();

        this.team = team;
    }
    internal Team team;
    internal void Spectator()
    {
        if (_LocalPlayer.car == null)
        {
            lockCursor = true;
            _LocalPlayer.RPCShow(false);
            _LocalPlayer.team = Team.Spectator;
        }
    }


    void OnPlayerDisconnected(NetworkPlayer player2)
    {
        int player = player2.GetHashCode();
        players[player].Destroy();
        foreach (box a in GameObject.FindObjectsOfType(typeof(box)))
        {
            if (a.id == player)
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