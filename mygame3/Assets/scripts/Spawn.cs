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
        localuser.frags = localuser.deaths = 0;
        _Vkontakte.enabled = _vk.enabled = false;
        zombiesleft = _cw.fraglimit;
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

            rpcwrite("You died on " + stage + " level");
            win = true;
            _TimerA.AddMethod(5000, WinGame);
        }
    }

    private void DMCheck()
    {
        foreach (Player pl in players.Values)
            if (pl.OwnerID != -1)
            {
                if (Network.isServer && !win && pl.frags >= _cw.fraglimit)
                {
                    rpcwrite(pl.Nick + " Win");
                    win = true;
                    _TimerA.AddMethod(5000, WinGame);
                }
            }
    }

    private void TDMCheck()
    {
        BlueFrags = RedFrags = 0;
        foreach (Vk.user pl in TP(Team.ata))
            RedFrags += pl.frags;

        foreach (Vk.user pl in TP(Team.def))
            BlueFrags += pl.frags;

        if ((BlueFrags >= _cw.fraglimit || RedFrags >= _cw.fraglimit))
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
        foreach (Vk.user user in userviews.Values)
        {
            if (dm || tdm)
            {

                user.totaldeaths += user.deaths;
                user.totalkills += user.frags;
            }
            else
            {
                user.totalzombiedeaths += user.deaths;
                user.totalzombiekills += user.frags;
            }
        }
        
        _Loader.RPCLoadLevel(Level.z3labby.ToString());
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

        CallRPC(true);

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
        if (Network.peerType == NetworkPeerType.Disconnected)
            Network.InitializeServer(32, 5300);
        Transform t;
        if (_LocalPlayer == null)
            t = (Transform)Network.Instantiate(_Player, Vector3.zero, Quaternion.identity, (int)Group.Player);
        else
            t = _LocalPlayer.transform;
        if (tdm || zombi)
            this.team = team;

    }
    internal Team team;
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