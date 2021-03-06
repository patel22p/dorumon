using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
public enum Group { Player, Bomb }
public class Game : Bs
{
    public bool predict = true;
    public Vector2 sensivity = Vector2.one;
    public float sensivityMove = .2f;
    private Vector2 LeftTap;
    private Vector2 RightTap;
    public bool immortal;
    public bool EnableSound;
    public LevelEditor levelEditor;
    internal Timer timer = new Timer();
    public PlType[] EnableZombies = new[] { PlType.Monster, PlType.Fatty };
    public ObsCamera Obs;
    public GUIText chatInput;
    public GUIText chatOutput;
    public AudioClip[] go;
    public AudioClip twin;
    public AudioClip ctwin;
    public PlayerView pv { get { return playerViews[NetworkPlayerID]; } }
    internal Dictionary<int, Shared> shareds = new Dictionary<int, Shared>();
    internal Dictionary<int, PlayerView> playerViews = new Dictionary<int, PlayerView>();
    public IEnumerable<Shared> Shareds { get { return shareds.Where(a => a.Value != null).Select(a => a.Value); } }
    public IEnumerable<Player> Players { get { return shareds.Where(a => a.Value != null && a.Value is Player).Select(a => (Player)a.Value); } }
    public IEnumerable<PlayerView> PlayerViews { get { return playerViews.Where(a => a.Value != null).Select(a => a.Value); } }
    public GameObject PlayerPrefab;
    public GameObject MonsterPrefab;
    public GameObject FattyPrefab;
    public Transform Fx;
    public Transform CTSpawn { get { return GameObject.Find("SpawnCT").transform; } }
    public Transform TSpawn { get { return GameObject.Find("SpawnT").transform; } }
    public Transform BombPlace;
    public bool GameStarted;
    public int TScore;
    public int CTScore;
    private float mouseClickTime = float.MinValue;
    private float ResetGameTime = float.MaxValue;
    internal float GameTime = TimeSpan.FromMinutes(18).Milliseconds;
    public Camera MiniMapCamera;
    public int zombies = 10;
    float afkTime;
    public bool RandomName;
    public override void Awake()
    {
        _Loader.timer.Clear();
        _TeamSelectGui.enabled = true;
        afkTime = Time.time;
        if (RandomName && Application.isEditor) PhotonNetwork.playerName = _Loader.playerName = "User" + Random.Range(0, 99);
        IgnoreAll("IgnoreColl");
        IgnoreAll("BotBox");
        IgnoreAll("Dead", "Level", "Dead");
        Debug.Log("Game Awake");
        if (Offline)
            PhotonNetwork.ConnectUsingSettings();
    }
    public void Start()
    {
        if (!Offline)
            OnConnected();
        if (!EnableSound && isEditor)
            AudioListener.volume = 0;
        timer.AddMethod(5000, delegate { _Hud.PrintPopup("Press M to select Team"); });
        timer.AddMethod(10000, delegate { _Hud.PrintPopup("Press C to switch camera views"); });
        timer.AddMethod(5000, delegate { MiniMapCamera.enabled = false; });
    }
    public void OnJoinedLobby()
    {
        if (PhotonNetwork.GetRoomList().Length == 0)
            PhotonNetwork.CreateRoom("Test", false, true, 10);
    }
    public void OnPhotonCreateGameFailed()
    {
        PhotonNetwork.JoinRoom("Test");
    }
    public void Update()
    {
        if (Offline) return;
        UpdateTouch();
        UpdateChat();
        if (PhotonNetwork.isMasterClient && timer.TimeElapsed(2000))
            CheckForWins();
        UpdateOther();
        timer.Update();
    }
    private void UpdateOther()
    {
        if (Input.anyKey)
            afkTime = Time.time;
        if (afkTime - Time.time > 120 && PhotonNetwork.isNonMasterClientInGame && _Player != null)
            PhotonNetwork.Disconnect();
        GameTime -= Time.deltaTime;
        _Hud.ScoreBoard.text = "";
        if (Input.GetKey(KeyCode.Tab))
            DrawScoreBoard();
        if (Input.GetKeyDown(KeyCode.Backslash))
            lockCursor = !lockCursor;
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M) && lockCursor)
        {
            _TeamSelectGui.enabled = !_TeamSelectGui.enabled;
            lockCursor = !_TeamSelectGui.enabled;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - mouseClickTime < .3f)
                lockCursor = true;
            mouseClickTime = Time.time;
        }
    }
    private void UpdateChat()
    {
        if (chatInput.enabled)
            foreach (char c in Input.inputString)
            {
                if (c == '\b')
                {
                    if (chatInput.text.Length > 4)
                        chatInput.text = chatInput.text.Substring(0, chatInput.text.Length - 1);
                    else
                    {
                        chatInput.enabled = false;
                        lockCursor = true;
                    }
                }
                else if (c == '\n' || c == '\r')
                {
                    CallRPC(Chat, PhotonTargets.All, _Loader.playerName + ": " + chatInput.text.Substring(4));
                    chatInput.enabled = false;
                    lockCursor = true;
                }
                else
                    chatInput.text += c;
            }
        else
            if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Return))
            {
                lockCursor = false;
                chatInput.enabled = true;
                chatInput.text = "say:";
            }
    }
    [RPC]
    public void StartGame()
    {
        Debug.Log("StartGame");
        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.RemoveAllInstantiatedObjects();
        }
        ResetGameTime = float.MaxValue;
        GameStarted = true;
        foreach (Transform a in Fx)
            Destroy(a.gameObject);
        if (pv.team != Team.Spectators)
        {
            Debug.Log("Creating Self " + pv.team);
            InstanciateShared(NetworkPlayerID, PlayerPrefab);
        }
        _ObsCamera.audio.PlayOneShot(go.Random());
        if (PhotonNetwork.isMasterClient)
            foreach (var a in playerViews)
                if (a.Value != null && a.Value.bot)
                    InstanciateShared(a.Key, GetPrefab(a.Value.plType));
    }
    [RPC]
    public void AddPlayerView(int id, string _name)
    {
        print("AddPlayerView" + id + " " + _name);
        var v = playerViews[id] = new PlayerView();
        v.id = id;
        v.PlayerName = _name;
    }
    [RPC]
    public virtual void RpcKillText(string txt)
    {
        print(txt);
        _Hud.KillText.text += "\r\n" + txt;
        _Game.timer.AddMethod(15000, delegate { _Hud.KillText.text = RemoveFirstLine(_Hud.KillText.text); });
    }
    [RPC]
    public void RemovePlayerView(int id)
    {
        if (playerViews[id] != null)
        {
            playerViews[id] = null;
        }
    }
    [RPC]
    private void Chat(string s)
    {
        timer.AddMethod(15000, delegate { chatOutput.text = RemoveFirstLine(chatOutput.text); });
        chatOutput.text += "\r\n" + s;
    }
    private void CheckForWins()
    {
        if (!GameStarted)
        {
            if (PlayerViews.Any(a => a.team == Team.Terrorists) &&
                PlayerViews.Any(a => a.team == Team.CounterTerrorists) && ResetGameTime == float.MaxValue)
            {
                CallRPC(RPCPrint, PhotonTargets.All, "Game Started");
                ResetGameTime = Time.time;
            }
            if (Time.time - ResetGameTime > 3)
                CallRPC(StartGame, PhotonTargets.All);
        }
        else if (GameStarted)
        {
            if (!PlayerViews.Any(a => a.team == Team.Terrorists) || !PlayerViews.Any(a => a.team == Team.CounterTerrorists))
                CallRPC(SetGameStarted, PhotonTargets.All, false);
            else
            {
                if (ResetGameTime == float.MaxValue)
                {
                    if (!Shareds.Any(a => a.pv.team == Team.CounterTerrorists) || (_Bomb != null && _Bomb.bombTime < 0))
                    {
                        CallRPC(SetTScore, PhotonTargets.All, TScore + 1);
                        CallRPC(TerWin, PhotonTargets.All);
                        ResetGameTime = Time.time;
                    }
                    if (!Shareds.Any(a => a.pv.team == Team.Terrorists) || (_Bomb != null && _Bomb.difused))
                    {
                        CallRPC(SetCTScore, PhotonTargets.All, CTScore + 1);
                        ResetGameTime = Time.time;
                        CallRPC(CTerWin, PhotonTargets.All);
                    }
                }
                if (Time.time - ResetGameTime > 3)
                    CallRPC(StartGame, PhotonTargets.All);
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
            sb.AppendLine(string.Format(templ, p.PlayerName, shareds[p.id] == null ? "Dead" : "", p.PlayerScore, p.PlayerDeaths, p.PlayerPing, p.PlayerFps, "", ""));
    }
    [RPC]
    private void SetGameStarted(bool value)
    {
        ResetGameTime = float.MaxValue;
        GameStarted = value;
    }
    public void RemoveBot(PlType type, Team team)
    {
        var bot = PlayerViews.FirstOrDefault(a => a.team == team && a.plType == type);
        shareds[bot.id].CallRPC(shareds[bot.id].Die, PhotonTargets.All);
        RemovePlayerView(bot.id);
    }
    public void CreateBot(PlType type, Team team)
    {
        var id = _Game.GetNextFreeSlot();
        if (id > 50) Debug.LogWarning("Max Bot Limit");
        else
        {
            Debug.Log("Create " + type + id);
            CallRPC(AddPlayerView, PhotonTargets.AllBuffered, id, type + "" + id);
            playerViews[id].plType = type;
            playerViews[id].team = team;
            playerViews[id].skin = Random.Range(0, 3);
            InstanciateShared(id, GetPrefab(type));
        }
    }
    private void InstanciateShared(int PlayerID, Object playerPrefab)
    {
        print("InstShared" + _Game.pv.team);
        Vector3 vector3 = (playerViews[PlayerID].team == Team.CounterTerrorists ? CTSpawn.position : TSpawn.position) + ZeroY(Random.onUnitSphere);
        var pl = PhotonNetwork.Instantiate(playerPrefab, vector3, Quaternion.identity, (int)Group.Player).GetComponent<Shared>();
        pl.CallRPC(pl.SetID, PhotonTargets.AllBuffered, PlayerID);
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
    public void OnJoinedRoom() { OnConnected(); }
    public void OnLeftRoom()
    {
        Debug.Log("Disconnected " + name);
        lockCursor = false;
        Application.LoadLevel(0);
    }
    private void OnConnected()
    {
        Debug.Log("Connected " + name + _Loader.DebugLevelMode);
        _TeamSelectGui.enabled = true;
        if (isEditor)
            _Loader.playerName = "Player" + PhotonNetwork.player.ID;
        CallRPC(AddPlayerView, PhotonTargets.AllBuffered, PhotonNetwork.player.ID, _Loader.playerName);
    }
    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        if (!PhotonNetwork.isMasterClient) return;
        CallRPC(SetGameStarted, player, GameStarted);
        CallRPC(SetCTScore, player, CTScore);
        CallRPC(SetTScore, player, TScore);
        RpcKillText(player.name + " Connected");
    }
    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        print("Disconnected" + player.name);
        RemovePlayerView(player.ID);
        RpcKillText(player.name + " disconnected");
        PhotonNetwork.DestroyPlayerObjects(player);
    }
    internal void OnTeamSelected()
    {
        if (_Player != null)
        {
            PhotonNetwork.RemoveRPCs(_Player.photonView);
            PhotonNetwork.Destroy(_Player.gameObject);
        }
        if (!GameStarted && _Game.pv.team != Team.Spectators)
            InstanciateShared(NetworkPlayerID, PlayerPrefab);
    }
    public int GetNextFreeSlot()
    {
        var i = 30;
        while (shareds.ContainsKey(++i) && shareds[i] != null) { }
        return i;
    }
    public GameObject GetPrefab(PlType type)
    {
        if (type == PlType.Player || type == PlType.Bot)
            return PlayerPrefab;
        if (type == PlType.Monster)
            return MonsterPrefab;
        if (type == PlType.Fatty)
            return FattyPrefab;
        return PlayerPrefab;
    }
    public new Vector3 GetMove()
    {
        if (!(lockCursor || Android)) return Vector3.zero;
        Vector3 v = Vector3.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += Vector3.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v += Vector3.back;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) v += Vector3.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightAlt)) v += Vector3.right;
        v = v.normalized;
        v += new Vector3(LeftTap.x, 0, LeftTap.y).normalized;
        return v;
    }
    public new Vector3 GetMouse()
    {
        if (Android && RightTap.magnitude < 15) return new Vector3(-RightTap.y * Mathf.Abs(RightTap.y) * _Game.sensivity.y * _Loader.SensivityY, RightTap.x * Mathf.Abs(RightTap.x) * _Game.sensivity.x * _Loader.SensivityX, 0);
        return lockCursor ? new Vector3(-Input.GetAxis("Mouse Y") * _Loader.SensivityY, Input.GetAxis("Mouse X") * _Loader.SensivityX, 0) : Vector3.zero;
    }
    public void OnMasterClientSwitched(PhotonPlayer bla)
    {
        if (PhotonNetwork.isMasterClient) RpcKillText(bla.name + " is host now");
    }
    public void UpdateTouch()
    {
        RightTap = Vector2.zero;
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Moved)
            {
                if (Input.GetTouch(i).position.x > 400)
                {
                    if (Input.GetTouch(i).deltaPosition != Vector2.zero)
                        RightTap = Input.GetTouch(i).deltaPosition;
                }
                else if (Input.GetTouch(i).deltaPosition.magnitude > 3)
                    LeftTap = Input.GetTouch(i).deltaPosition;
            }
            if ((Input.GetTouch(i).phase == TouchPhase.Ended || Input.GetTouch(i).phase == TouchPhase.Canceled) && Input.GetTouch(i).position.x < 400)
                LeftTap = Vector2.zero;
        }
    }
}
