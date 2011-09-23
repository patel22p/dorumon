using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
[Serializable]
public class PlayerView
{
    public bool bot;
    public int id;
    public int PlayerPing;
    public int PlayerFps;
    public int PlayerMoney=16000;
    public int PlayerScore;
    public int PlayerDeaths;
    public string PlayerName = "";
    public int skin;
    public Team team;
}
public class Game : Bs
{
    public LevelEditor levelEditor;
    public enum StartUp { AutoHost, Offline }
    internal Timer timer = new Timer();
    public StartUp startUpMode;
    public enum group { Player }
    public ObsCamera Obs;
    public GUIText chatInput;
    public GUIText chatOutput;
    public AudioClip[] go;
    public AudioClip twin;
    public AudioClip ctwin;
    public PlayerView pv { get { return playerViews[NetworkPlayerID]; } }
    internal Player[] players = new Player[36];
    internal PlayerView[] playerViews = new PlayerView[36];
    public IEnumerable<Player> Players { get { return players.Where(a => a != null); } }
    public IEnumerable<PlayerView> PlayerViews { get { return playerViews.Where(a => a != null); } }
    
    public GameObject PlayerPrefab;
    public Transform Fx;
    public Transform CTSpawn;
    public Transform TSpawn;
    public bool GameStarted;        
    public int TScore;
    public int CTScore;
    private float mouseClickTime = float.MinValue;
    private float ResetGameTime = float.MaxValue;
    internal float GameTime = TimeSpan.FromMinutes(18).Milliseconds;
    public Camera MiniMapCamera;
    
    public override void Awake()
    {
        Debug.Log("Game Awake");        
        if (!Offline || startUpMode == StartUp.Offline)
            OnConnected();
        _LoaderGui.enabled = false;        
    }

    public void Start()
    {
        _Hud.PrintPopup("Press F1/F2 to switch camera views");
        timer.AddMethod(5000, delegate { MiniMapCamera.enabled = false; });
        if (startUpMode == StartUp.AutoHost && Offline)
        {
            if (Network.InitializeServer(6, port, false) != NetworkConnectionError.NoError)
                Network.Connect("127.0.0.1", port);
            return;
        }
    }
    [RPC]
    public void AddPlayerView(int id, string name)
    {
        var v = playerViews[id] = new PlayerView();
        v.id = id;
        v.PlayerName = name;
    }
    [RPC]
    public void RemovePlayerView(int id)
    {
        playerViews[id] = null;
    }

    public void Update()
    {
        if (Offline) return;
        UpdateChat();
        if (Network.isServer && timer.TimeElapsed(2000))
            CheckForWins();
        UpdateOther();
        timer.Update();
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

        if (Input.GetKeyDown(KeyCode.Backslash))
            Screen.lockCursor = !Screen.lockCursor;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _GameGui.enabled = !_GameGui.enabled;
            Screen.lockCursor = !_GameGui.enabled;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - mouseClickTime < .3f)
                Screen.lockCursor = true;
            mouseClickTime = Time.time;
        }
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
        timer.AddMethod(15000, delegate { chatOutput.text = RemoveFirstLine(chatOutput.text); });
        chatOutput.text += s + "\r\n";
    }
    private void CheckForWins()
    {
        if (!GameStarted)
        {
            if (PlayerViews.Any(a => a.team == Team.Terrorists) && PlayerViews.Any(a => a.team == Team.CounterTerrorists) && ResetGameTime == float.MaxValue)
            {
                CallRPC(RPCPrint, RPCMode.All, "Game Started");
                ResetGameTime = Time.time;
            }
            if (Time.time - ResetGameTime > 3)
                CallRPC(StartGame, RPCMode.All);
        }
        else if (GameStarted)
        {
            if (!PlayerViews.Any(a => a.team == Team.Terrorists) || !PlayerViews.Any(a => a.team == Team.CounterTerrorists))
                CallRPC(SetGameStarted, RPCMode.All, false);
            else
            {
                if (ResetGameTime == float.MaxValue)
                {
                    if (!Players.Any(a => a.pv.team == Team.CounterTerrorists))
                    {
                        //todo use playerViewer
                        CallRPC(SetTScore, RPCMode.All, TScore + 1);
                        ResetGameTime = Time.time;
                        CallRPC(TerWin, RPCMode.All);
                    }
                    if (!Players.Any(a => a.pv.team == Team.Terrorists))
                    {
                        CallRPC(SetCTScore, RPCMode.All, CTScore + 1);
                        ResetGameTime = Time.time;
                        CallRPC(CTerWin, RPCMode.All);
                    }
                }
                if (Time.time - ResetGameTime > 3)
                        CallRPC(StartGame, RPCMode.All);
            }
        }
    }
    [RPC]
    private void TerWin()
    {
        RPCPrint("Terrorists Win");
        _ObsCamera.audio.PlayOneShot(twin);
    }
    [RPC]
    private void CTerWin()
    {
        RPCPrint("Counter Terrorists Win");
        _ObsCamera.audio.PlayOneShot(ctwin);
    }

    [RPC]
    private void RPCPrint(string s)
    {
        Debug.Log(s);
        _Hud.PrintPopup(s);
    }
    public void OnLevelWasLoaded(int level)
    {
        Debug.Log(name + " Level Loaded " + level);
        foreach (Player p in Object.FindObjectsOfType(typeof(Player)))
            DestroyImmediate(p.gameObject);
        Network.isMessageQueueRunning = true;
        Network.SetSendingEnabled(0, true);
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
        PrintPls(sb, templ, PlayerViews.Where(a => a.team == Team.Terrorists));
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("Counter - Terrorists  " + CTScore);
        sb.AppendLine("_______________________________________________________________________________");
        //sb.AppendLine(tabl);
        PrintPls(sb, templ, PlayerViews.Where(a => a.team == Team.CounterTerrorists));
        sb.AppendLine();
        sb.AppendLine("Spectators");
        foreach (var p in PlayerViews.Where(a => a.team == Team.Spectators))
            sb.AppendLine(p.PlayerName);
        _Hud.ScoreBoard.text = sb.ToString();
    }
    private void PrintPls(StringBuilder sb, string templ, IEnumerable<PlayerView> pls)
    {
        foreach (var p in pls)
            sb.AppendLine(string.Format(templ, p.PlayerName, players[p.id] == null ? "Dead" : "", p.PlayerScore, p.PlayerDeaths, p.PlayerPing, p.PlayerFps, "", ""));
    }

    [RPC]
    private void SetGameStarted(bool value)
    {
        ResetGameTime = float.MaxValue;
        GameStarted = value;
    }

    [RPC]
    private void StartGame()
    {
        Debug.Log("StartGame");
        if (Network.isServer)
            foreach (Player p in GameObject.FindObjectsOfType(typeof(Player)))
            {
                Network.RemoveRPCs(p.networkView.viewID);
                Network.Destroy(p.gameObject);
            }

        ResetGameTime = float.MaxValue;
        GameStarted = true;
        foreach (Transform a in Fx)
            Destroy(a.gameObject);

        if (pv.team != Team.Spectators)
            CreatePlayer();
        _ObsCamera.audio.PlayOneShot(go.Random());

        if (Network.isServer)
        {
            for (int i = 0; i < playerViews.Length; i++)
                if (playerViews[i] != null)
                {
                    var a = playerViews[i];
                    if (a.bot)
                        CreateBot(i);
                }
        }
    }

    private void CreatePlayer()
    {        
        if(pv.team == Team.Spectators)
            Debug.LogWarning("Player is Spec");
        InstanciatePlayer(NetworkPlayerID);
    }

    private void InstanciatePlayer(int PlayerID)
    {
        var vector3 = (playerViews[PlayerID].team == Team.CounterTerrorists ? CTSpawn.position : TSpawn.position) + ZeroY(Random.onUnitSphere);
        var pl = ((GameObject)Network.Instantiate(PlayerPrefab, vector3, Quaternion.identity, (int)group.Player)).GetComponent<Player>();
        pl.CallRPC(pl.SetID, RPCMode.AllBuffered, PlayerID);
    }

    internal void CreateBot(int id)
    {
        Debug.Log("Create Bot " + id);
        playerViews[id].bot = true;
        playerViews[id].skin = Random.Range(0, 3);        
        InstanciatePlayer(id);
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
        Debug.Log("Connected " + name + _Loader.DebugLevelMode);
        CallRPC(AddPlayerView, RPCMode.AllBuffered, Network.player.GetHashCode(), _Loader.playerName);
        _TeamSelectGui.enabled = true;
    }
    public void OnPlayerConnected(NetworkPlayer player)
    {
        if (!Network.isServer) return;        
        CallRPC(SetGameStarted, player, GameStarted);
        CallRPC(SetCTScore, player, CTScore);
        CallRPC(SetTScore, player, TScore);
    }

    

    public void OnPlayerDisconnected(NetworkPlayer player)
    {                
        CallRPC(RemovePlayerView, RPCMode.All, player.GetHashCode());
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }
    internal void OnTeamSelected()
    {
        if (_Player != null)
        {
            Network.RemoveRPCs(_Player.networkView.viewID);
            Network.Destroy(_Player.gameObject);
        }

        if (!GameStarted)
        {
            
            if (_Game.pv.team != Team.Spectators) CreatePlayer();
        }
    }
    
    public int GetNextFreeSlot()
    {
        var i = 16;
        while (players[++i] != null) { }
        return i;
    }



    public void OnServerInitialized() { OnConnected(); }
    public void OnConnectedToServer() { OnConnected(); }
    

}
