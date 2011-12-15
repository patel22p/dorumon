using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gui = UnityEngine.GUILayout;
using doru;

public class MasterServer { }

public class LoaderGui : Bs
{
    //bug implement create join failed
    string gameName = "";
    //string label = "";
    public string version;
    public bool ConnectToMasterServer;

    Timer timer = new Timer();
    public void Start()
    {
        print("LoadGUI Start");
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.playerName = "Guest" + Random.Range(0, 99);
    }
    public void Update()
    {
        timer.Update();
    }
    public void OnGUI()
    {
        GUI.skin = _Loader.skin;
        var c = new Vector3(Screen.width, Screen.height) / 2f;
        var s = new Vector3(400, 400) / 2f;
        var v1 = c - s;
        var v2 = c + s;
        GUI.Window((int)WindowEnum.ConnectionGUI, Rect.MinMaxRect(v1.x, v1.y, v2.x, v2.y), Window, version);
    }

    enum Menu { MainMenu, SelectMap, Options }
    private Menu menu;
    private Vector2 scrollPosition;

    public bool EnableBlood { get { return PlayerPrefs.GetInt("EnableBlood", 1) == 1; } set { PlayerPrefs.SetInt("EnableBlood", value ? 1 : 0); } }
    public bool EnableHighQuality { get { return PlayerPrefs.GetInt("EnableHighQuality", 0) == 1; } set { PlayerPrefs.SetInt("EnableHighQuality", value ? 1 : 0); } }
    public float SensivityX { get { return PlayerPrefs.GetFloat("sx", 1); } set { PlayerPrefs.SetFloat("sx", value); } }
    public float SensivityY { get { return PlayerPrefs.GetFloat("sy", 1); } set { PlayerPrefs.SetFloat("sy", value); } }
    public float SoundVolume  { get { return PlayerPrefs.GetFloat("SoundVolume ", 1); } set { PlayerPrefs.SetFloat("SoundVolume ", value); } }
    public float maxPlayers = 2;
    void Window(int id)
    {
        if (menu == Menu.Options)
        {
            if (gui.Button("<<Back"))
                menu = Menu.MainMenu;

            EnableHighQuality = gui.Toggle(EnableHighQuality, "High Quality");
            EnableBlood = gui.Toggle(EnableBlood, "Enable Blood");
            gui.BeginHorizontal();
            gui.Label("MaxPlayers:" + (int)maxPlayers, gui.ExpandWidth(false));
            maxPlayers = gui.HorizontalSlider(maxPlayers, 1, 8);
            gui.EndHorizontal();

            gui.BeginHorizontal();
            gui.Label("SensivityX", gui.ExpandWidth(false));
            SensivityX = gui.HorizontalSlider(SensivityX, .1f, 2);
            gui.EndHorizontal();
            gui.BeginHorizontal();
            gui.Label("SensivityY", gui.ExpandWidth(false));
            SensivityY = gui.HorizontalSlider(SensivityY, .1f, 2);
            gui.EndHorizontal();
            gui.BeginHorizontal();
            gui.Label("Sound Vol", gui.ExpandWidth(false));
            SoundVolume = gui.HorizontalSlider(SoundVolume, 0, 1);
            gui.EndHorizontal();
            if (gui.Button("Reset Settings"))
                PlayerPrefs.DeleteAll();

            AudioListener.volume = SoundVolume;
        }
        else if (menu == Menu.SelectMap)
        {
            if (gui.Button("<<Back"))
                menu = Menu.MainMenu;
            scrollPosition = gui.BeginScrollView(scrollPosition);
            foreach (var map in _Loader.maps)
            {
                int p = GetLevelLoad(map);
                if (gui.Button(map + (p < 100 ? (" " + p + "%") : "")))
                    if (p == 100)
                        Host(map);
                    else
                        print("Map Not Loaded yet");
            }
            gui.EndScrollView();
        }
        else
        {
            if (lockCursor) return;
            gui.Label(PhotonNetwork.connectionState + "");
            gui.Label("Name:");
            _Loader.playerName = gui.TextField(_Loader.playerName, 25);
            gui.Label("GameName:");
            gameName = gui.TextField(gameName);
            gui.BeginHorizontal();
            if (gui.Button("Connect"))
            {
                ShowLoading(true);
                PhotonNetwork.JoinRoom(gameName == "" ? "test" : gameName);
                _Loader.timer.AddMethod(6000, delegate { ShowLoading(false); });
            }
            if (gui.Button("Host"))
                menu = Menu.SelectMap;
            if (gui.Button("Options"))
                menu = Menu.Options;

            gui.EndHorizontal();
            scrollPosition = gui.BeginScrollView(scrollPosition);
            //bug implement masterserver
            if (PhotonNetwork.GetRoomList().Length == 0)
                GUILayout.Label("..no games available..");
            else
                foreach (Room host in PhotonNetwork.GetRoomList())
                {
                    int p = GetLevelLoad(host.name.Split("/")[0]);
                    if (gui.Button(host.name +
                        (" " + host.playerCount + "/" + host.maxPlayers) +
                        (p < 100 ? (" " + p + "%") : "")))
                    {
                        if (p == 100)
                        {
                            Print("Trying Connect to " + host.name);
                            ShowLoading(true);
                            _Loader.timer.AddMethod(6000, delegate { ShowLoading(false); });
                            PhotonNetwork.JoinRoom(host.name);
                        }
                        else
                            print("Map Not Loaded yet");

                    }
                }
            gui.EndScrollView();
        }
    }
    public void ShowLoading(bool show)
    {
        this.enabled = !show;
    }
    private int GetLevelLoad(string a)
    {
        return (int)(Application.GetStreamProgressForLevel(a) * 100);
    }

    public string mapName;
    private void Host(string mapName)
    {
        this.mapName = mapName;
        ShowLoading(true);
        Debug.LogWarning("Loading Map");
        menu = Menu.MainMenu;
        //PhotonNetwork.JoinRoom((int)maxPlayers - 1, port, !PhotonNetwork.HavePublicAddress());
        PhotonNetwork.CreateRoom(mapName + "/" + _Loader.playerName, ConnectToMasterServer, true, (byte)(maxPlayers - 1));        
        //if (ConnectToMasterServer)
        //    MasterServer.RegisterHost(_LoaderGui.version, _Loader.playerName + "'s game", mapName);
        
    }
    public void OnCreatedRoom()    {
        //if (!enabled) return;
        _Loader.LoadLevel(mapName);
    }
    public void OnPhotonCreateGameFailed()
    {
        Print("Same Room Already Exists");
        ShowLoading(false);
    }

    public void OnPhotonRandomJoinFailed()
    {
        Print("Failled to connect to room");
        ShowLoading(false);
    }
    public void OnPhotonJoinGameFailed()
    {
        Print("Failled to connect to room");
        ShowLoading(false);
    }

    public void OnFailedToConnectToPhoton(NetworkConnectionError info)
    {
        Print(info + "");
    }
    public void Print(string text)
    {
        Debug.LogWarning(text);
        //label = text;
    }
}
