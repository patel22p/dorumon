using System.Linq;
using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Text;
using doru;

public class Game : Bs
{
    public Timer timer = new Timer();
    public enum StartUp { AutoHost, ShowMenu }
    public StartUp startUp;
    public new Player _Player;
    public Team team = Team.Spectators;
    public enum group { Player }
    public Player[] players = new Player[36];
    public ObsCamera Obs;
    public bool GameStarted;
    public AudioClip[] go;
    public IEnumerable<Player> Players { get { return players.Where(a => a != null); } }
    public IEnumerable<Player> AlivePlayers { get { return Players.Where(a => !a.dead); } }
    public IEnumerable<Player> Terror { get { return Players.Where(a => a.team == Team.Terrorists); } }
    public IEnumerable<Player> CounterTerror { get { return Players.Where(a => a.team == Team.CounterTerrorists); } }
    public IEnumerable<Player> Spectators { get { return Players.Where(a => a.team == Team.Spectators); } }
    public GameObject PlayerPrefab;
    public Transform Fx;
    public Transform CTSpawn;
    public Transform TSpawn;
    internal string playerName { get { return PlayerPrefs.GetString("PlayerName", "Guest" + Random.Range(0, 99)); } set { PlayerPrefs.SetString("PlayerName", value); } }
    public int PlayerMoney = 16000;
    public int PlayerScore;
    public int PlayerDeaths;
    public int PlayerPing;
    private float mouseClickTime = float.MinValue;
    private float ResetGameTime = float.MaxValue;
    internal float GameTime = TimeSpan.FromMinutes(18).Milliseconds;
    public override void Awake()
    {
        Application.targetFrameRate = 300;
        Debug.Log("Game Awake");
    }
    public void Start()
    {

        if (startUp != StartUp.ShowMenu)
            _GameGui.enabled = false;

        if (startUp == StartUp.AutoHost)
        {
            if (Network.InitializeServer(6, port, false) != NetworkConnectionError.NoError)
                Network.Connect("127.0.0.1", port);
            return;
        }
    }
    void OnConnected()
    {
        Debug.Log("Connected");
        _GameGui.enabled = false;

        if (_Player != null)
            DestroyImmediate(_Player.gameObject);
        if (PlayerPrefab.networkView == null)
        {
            var nw = PlayerPrefab.AddComponent<NetworkView>();
            nw.stateSynchronization = NetworkStateSynchronization.Unreliable;
            nw.observed = PlayerPrefab.GetComponent<Player>();
        }

        if (_Game.team != Team.Spectators)
            CreatePlayer();
        else
            _TeamSelectGui.enabled = true;
    }
    public void Update()
    {
        //todo chat
        CheckForWins();
        UpdateMouseLock();
        UpdateOther();
    }

    private void UpdateOther()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            _TeamSelectGui.enabled = !_TeamSelectGui.enabled;
            Screen.lockCursor = !_TeamSelectGui.enabled;
        }
        _Hud.ScoreBoard.text = "";
        if (Input.GetKey(KeyCode.LeftShift))
            DrawScoreBoard();
        timer.Update();
    }

    private void CheckForWins()
    {
        GameTime -= Time.deltaTime;
        if (!GameStarted)
        {
            if (Terror.Count() > 0 && CounterTerror.Count() > 0 && ResetGameTime == float.MaxValue)
            {
                _Hud.PrintPopup("Game Started");
                ResetGameTime = Time.time;
            }
            if (Time.time - ResetGameTime > 3 && Network.isServer)
                CallRPC(StartGame, RPCMode.All);
        }
        else
        {
            if (Terror.Count() == 0 || CounterTerror.Count() == 0)
                GameStarted = false;
            else
            {
                //todo add team score
                if (Terror.Where(a => !a.dead).Count() == 0 && ResetGameTime == float.MaxValue)
                {
                    ResetGameTime = Time.time;
                    _Hud.PrintPopup("Counter Terrorists Win");
                }
                if (CounterTerror.Where(a => !a.dead).Count() == 0 && ResetGameTime == float.MaxValue)
                {
                    ResetGameTime = Time.time;
                    _Hud.PrintPopup("Terrorists Win");
                }
                if (Time.time - ResetGameTime > 3 && Network.isServer)
                    CallRPC(StartGame, RPCMode.All);
            }
        }
    }

    private void UpdateMouseLock()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - mouseClickTime < .5f)
                Screen.lockCursor = true;
            mouseClickTime = Time.time;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
            Screen.lockCursor = !Screen.lockCursor;
        if (Input.GetKeyDown(KeyCode.Tab))
            Screen.lockCursor = !Screen.lockCursor;
    }
    private void DrawScoreBoard()
    {
        //Debug.Log("ShowScoreBoard");
        var sb = new StringBuilder();
        sb.AppendLine("Counter Strike 1.6                                              ");
        const string tabl = "                         x       Score          Deaths         Ping     FPS  ";
        sb.AppendLine("_________________________________________________________________________");
        var templ = CreateTable(tabl);
        sb.AppendLine(tabl);
        sb.AppendLine();
        sb.AppendLine("Terrorists  ");
        sb.AppendLine("_________________________________________________________________________");
        PrintPls(sb, templ, Terror);
        sb.AppendLine();
        sb.AppendLine("Counter - Terrorists  ");
        sb.AppendLine("_________________________________________________________________________");
        //sb.AppendLine(tabl);
        PrintPls(sb, templ, CounterTerror);
        sb.AppendLine();
        sb.AppendLine("Spectators");
        foreach (var p in Spectators)
            sb.AppendLine(p.name);
        _Hud.ScoreBoard.text = sb.ToString();
    }
    private static void PrintPls(StringBuilder sb, string templ, IEnumerable<Player> pls)
    {
        foreach (var p in pls)
            sb.AppendLine(string.Format(templ, p.PlayerName, p.dead ? "Dead" : "", p.PlayerScore, p.PlayerDeaths, p.PlayerPing, p.PlayerFps, "", ""));
    }
    [RPC]
    private void StartGame()
    {
        ResetGameTime = float.MaxValue;
        GameStarted = true;
        foreach (Transform a in Fx)
            Destroy(a.gameObject);

        if (_Player != null)
        {
            Network.RemoveRPCs(_Player.networkView.viewID);
            Network.Destroy(_Player.gameObject);
        }
        CreatePlayer();
        _Player.audio.PlayOneShot(go.Random());
    }
    private void CreatePlayer()
    {
        var r = Random.onUnitSphere;
        r.y = 0;
        if (Offline)
            Instantiate(PlayerPrefab);
        else
            Network.Instantiate(PlayerPrefab, (_Game.team == Team.CounterTerrorists ? CTSpawn.position : TSpawn.position) + r, Quaternion.identity, (int)group.Player);
    }
    public void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Debug.Log("Disconnected");
        _GameGui.enabled = true;
    }
    public void OnPlayerDisconnected(NetworkPlayer player)
    {
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }
    internal void OnTeamSelected()
    {
        if (_Player != null && !_Player.dead && _Player.team != _Game.team)
        {
            if (GameStarted)
                _Player.CallRPC(_Player.SetLife, RPCMode.All, 0, _Player.id);
            _Player.CallRPC(_Player.SetTeam, RPCMode.All, (int)_Game.team);
        }
        if (_Player == null)
            CreatePlayer();
    }
    public void OnServerInitialized() { OnConnected(); }
    public void OnConnectedToServer() { OnConnected(); }
}
