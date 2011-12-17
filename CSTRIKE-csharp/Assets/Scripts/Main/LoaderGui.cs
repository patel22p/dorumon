using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gui = UnityEngine.GUILayout;
using doru;

public class MasterServer { }

public class LoaderGui : Bs
{
    //bug implement create join failed
    //string gameName = "";
    //string label = "";
    private string version = "c";
    public bool ConnectToMasterServer;

    Timer timer = new Timer();
    public void Start()
    {
        print("LoadGUI Start");
        if (PhotonNetwork.connectionState == ConnectionState.Disconnected)
            PhotonNetwork.ConnectUsingSettings();
        //if (PhotonNetwork.playerName == "")
        //    PhotonNetwork.playerName = "Guest" + Random.Range(0, 99);
        
    }
    public void Update()
    {
        PhotonNetwork.playerName = _Loader.playerName;
        timer.Update();
    }
    public void OnGUI()
    {
        
        GUI.skin = _Loader.skin;
        var c = new Vector3(Screen.width, Screen.height) / 2f;
        var s = new Vector3(400, 400) / 2f;
        var v1 = c - s;
        var v2 = c + s;
        GUI.Window((int)WindowEnum.ConnectionGUI, Rect.MinMaxRect(v1.x, v1.y, v2.x, v2.y), Window, version + " www.csmini.tk");
    }

    enum Menu { MainMenu, SelectMap, Options }
    private Menu menu;
    private Vector2 scrollPosition;


    public float maxPlayers
    {
        get { return PlayerPrefs.GetFloat("maxPlayers", 2); }
        set { PlayerPrefs.SetFloat("maxPlayers", value); }
    }

    void Window(int id)
    {
        if (menu == Menu.Options)
        {
            if (gui.Button("<<Back"))
                menu = Menu.MainMenu;


            _Loader.EnableHighQuality = gui.Toggle(_Loader.EnableHighQuality, "High Quality");
            _Loader.EnableBlood = gui.Toggle(_Loader.EnableBlood, "Enable Blood");
            gui.BeginHorizontal();
            gui.Label("MaxPlayers:" + (int)maxPlayers, gui.ExpandWidth(false));
            maxPlayers = gui.HorizontalSlider(maxPlayers, 1, 8);
            gui.EndHorizontal();

            gui.BeginHorizontal();
            gui.Label("SensivityX", gui.ExpandWidth(false));
            _Loader.SensivityX = gui.HorizontalSlider(_Loader.SensivityX, .1f, 2);
            gui.EndHorizontal();
            gui.BeginHorizontal();
            gui.Label("SensivityY", gui.ExpandWidth(false));
            _Loader.SensivityY = gui.HorizontalSlider(_Loader.SensivityY, .1f, 2);
            gui.EndHorizontal();
            gui.BeginHorizontal();
            gui.Label("Sound Vol", gui.ExpandWidth(false));
            _Loader.SoundVolume = gui.HorizontalSlider(_Loader.SoundVolume, 0, 1);
            gui.EndHorizontal();
            if (gui.Button("Reset Settings"))
                PlayerPrefs.DeleteAll();

            AudioListener.volume = _Loader.SoundVolume;
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
            gui.Label(PhotonNetwork.connectionState + " " + PhotonNetwork.GetRoomList().Length);
            gui.Label("Name:");
            _Loader.playerName = gui.TextField(_Loader.playerName, 25);
            //gui.Label("GameName:");
            //gameName = gui.TextField(gameName);
            gui.BeginHorizontal();
            //if (gui.Button("Connect"))
            //{
            //    ShowLoading(true);
            //    PhotonNetwork.JoinRoom(gameName == "" ? "test" : gameName);
            //    _Loader.timer.AddMethod(6000, delegate { ShowLoading(false); });
            //}
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
                    var sets = host.name.Split("/");
                    if(sets.Length!=3) continue;
                    string mapName = sets[0];                    
                    if (sets[1] != version) continue;                    
                    int p = GetLevelLoad(mapName);                    
                    if (gui.Button(host.name +
                        (" " + host.playerCount + "/" + host.maxPlayers) +
                        (p < 100 ? (" " + p + "%") : "")))
                    {
                        if (p == 100)
                        {
                            Print("Trying Connect to " + host.name);
                            ShowLoading(true);
                            _Loader.timer.AddMethod(6000, delegate { ShowLoading(false); });
                            this.mapName = mapName;
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

        PhotonNetwork.CreateRoom(mapName + "/" + version + "/" + _Loader.playerName, mapName != "1", true, (byte)(maxPlayers));
        
        //if (ConnectToMasterServer)
        //    MasterServer.RegisterHost(_LoaderGui.version, _Loader.playerName + "'s game", mapName);

    }
    
    public void OnCreatedRoom()
    {
        //if (!enabled) return;
        _Loader.LoadLevel(mapName);
    }

    public void OnJoinedRoom() { _Loader.LoadLevel(mapName); }
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
