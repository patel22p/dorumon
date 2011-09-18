using Object = UnityEngine.Object;
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
    public enum StartUp { AutoHost, Offline }
    public StartUp startUp;
    public new Player _Player;
    public Team team = Team.Spectators;
    public enum group { Player }
    public Player[] players = new Player[36];
    public ObsCamera Obs;
    public GUIText chatInput;
    public GUIText chatOutput;
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

    public bool GameStarted;
    public int PlayerMoney = 16000;
    public int PlayerScore;
    public int PlayerDeaths;
    public int PlayerPing;
    public int TScore;
    public int CTScore;
    private float mouseClickTime = float.MinValue;
    private float ResetGameTime = float.MaxValue;
    internal float GameTime = TimeSpan.FromMinutes(18).Milliseconds;
    public Camera MiniMapCamera;

    public override void Awake()
    {
        Debug.Log("Game Awake");
        if (!Offline)
            OnConnected();
        _LoaderGui.enabled = false;        
    }
    public void Start()
    {
        //MiniMapCamera.enabled= true;
        timer.AddMethod(5000, delegate
        {
            MiniMapCamera.enabled= false;
        });

        if (startUp == StartUp.AutoHost && Offline)
        {
            if (Network.InitializeServer(6, port, false) != NetworkConnectionError.NoError)
                Network.Connect("127.0.0.1", port);
            return;
        }
    }
    
    public void Update()
    {        
        //todo select model
        UpdateChat();
        if (Network.isServer && timer.TimeElapsed(2000))
            CheckForWins();
        UpdateOther();
    }

    private void UpdateChat()
    {
        if (chatInput.enabled)
            foreach (char c in Input.inputString)
            {
                if (c == "\b"[0] && chatInput.text.Length != 0)
                    chatInput.text = chatInput.text.Substring(0, chatInput.text.Length - 1);
                else if (c == "\n"[0] || c == "\r"[0])
                {

                    CallRPC(RPCChat, RPCMode.All, _Loader.playerName + ": " + chatInput.text.Substring(4));
                    chatInput.enabled = false;
                    Screen.lockCursor = true;
                }
                else
                    chatInput.text += c;
            }
        if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Y))
        {
            Screen.lockCursor = false;
            chatInput.enabled = !chatInput.enabled;
            chatInput.text = "say:";
        }
   
    }

    [RPC]
    private void RPCChat(string s)
    {
        timer.AddMethod(5000, delegate { chatOutput.text = string.Join("\r\n", chatOutput.text.Split("\r\n").Skip(1).ToArray()); });
        chatOutput.text += s + "\r\n";
    }

    private void UpdateOther()
    {
        GameTime -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.M))
        {
            _TeamSelectGui.enabled = !_TeamSelectGui.enabled;
            Screen.lockCursor = !_TeamSelectGui.enabled;
        }
        _Hud.ScoreBoard.text = "";
        if (Input.GetKey(KeyCode.Tab))
            DrawScoreBoard();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _GameGui.enabled = !_GameGui.enabled;
            Screen.lockCursor = !_GameGui.enabled;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - mouseClickTime < .5f)
                Screen.lockCursor = true;
            mouseClickTime = Time.time;
        }        

        timer.Update();
    }

    private void CheckForWins()
    {
        if (!GameStarted)
        {
            if (Terror.Count() > 0 && CounterTerror.Count() > 0 && ResetGameTime == float.MaxValue)
            {
                CallRPC(RPCPrint, RPCMode.All, "Game Started");
                ResetGameTime = Time.time;
            }
            if (Time.time - ResetGameTime > 3)
                CallRPC(StartGame, RPCMode.All);
        }
        else if(GameStarted)
        {
            if (Terror.Count() == 0 || CounterTerror.Count() == 0)
                CallRPC(SetGameStarted,RPCMode.All,false);
            else
            {
                if (ResetGameTime == float.MaxValue)
                {
                    if (Terror.Where(a => !a.dead).Count() == 0)
                    {
                        CallRPC(SetCTScore, RPCMode.All, CTScore + 1);
                        ResetGameTime = Time.time;
                        CallRPC(RPCPrint, RPCMode.All, "Terrorists Win");
                    }
                    if (CounterTerror.Where(a => !a.dead).Count() == 0)
                    {
                        CallRPC(SetTScore, RPCMode.All, TScore + 1);
                        ResetGameTime = Time.time;
                        CallRPC(RPCPrint, RPCMode.All, "Counter Terrorists Win");
                    }
                }
                if (Time.time - ResetGameTime > 3)
                        CallRPC(StartGame, RPCMode.All);
            }
        }
    }
    [RPC]
    private void RPCPrint(string s)
    {
        Debug.Log(s);
        _Hud.PrintPopup(s);
    }

    
    private void DrawScoreBoard()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Counter Strike 1.6                               " + (GameStarted ? "Game Started" : ""));
        sb.AppendLine();
        const string tabl = "                         x       Score          Deaths         Ping     FPS  ";
        var templ = CreateTable(tabl);
        sb.AppendLine(tabl);
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("Terrorists  " + TScore);
        sb.AppendLine("_______________________________________________________________________________");
        PrintPls(sb, templ, Terror);
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("Counter - Terrorists  " + CTScore);
        sb.AppendLine("_______________________________________________________________________________");
        //sb.AppendLine(tabl);
        PrintPls(sb, templ, CounterTerror);
        sb.AppendLine();
        sb.AppendLine("Spectators");
        foreach (var p in Spectators)
            sb.AppendLine(p.PlayerName);
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
        if (Network.isServer)
            foreach (Player p in GameObject.FindObjectsOfType(typeof(Player)))
            {
                Network.RemoveRPCs(p.networkView.viewID);
                Network.Destroy(p.gameObject);
            }

        Debug.Log("StartGame");
        ResetGameTime = float.MaxValue;
        GameStarted = true;
        foreach (Transform a in Fx)
            Destroy(a.gameObject);
        
        CreatePlayer();
        _Player.audio.PlayOneShot(go.Random());
    }
    [RPC]
    private void SetGameStarted(bool value)
    {
        ResetGameTime = float.MaxValue;
        GameStarted = value;
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
    [RPC]
    public void SetTScore(int score)
    {        
        TScore = score;
    }
    [RPC]
    public void SetCTScore(int score)
    {        
        CTScore = score;
    }
    void OnConnected()
    {
        Debug.Log("Connected " + name);
        CreatePlayer();
        _TeamSelectGui.enabled = true;
    }
    public void OnPlayerDisconnected(NetworkPlayer player)
    {
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }
    internal void OnTeamSelected()
    {
        if (_Player != null && !_Player.dead)
            _Player.CallRPC(_Player.RPCSetLife, RPCMode.All, 0, _Player.id); 

        if (!GameStarted)        
            CreatePlayer();
        _Player.CallRPC(_Player.SetTeam, RPCMode.All, (int)_Game.team); 
    }
    public void OnPlayerConnected(NetworkPlayer player)
    {
        if (!Network.isServer) return;
        CallRPC(SetGameStarted, player, GameStarted);
        CallRPC(SetCTScore, player, CTScore);
        CallRPC(SetTScore, player, TScore);
    }
    public void OnServerInitialized() { OnConnected(); }
    public void OnConnectedToServer() { OnConnected(); }
}
