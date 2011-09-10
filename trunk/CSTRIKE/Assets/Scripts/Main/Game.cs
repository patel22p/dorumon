using UnityEngine;
using System.Collections;

public class Game : Bs {
    public enum StartUp { OfflineMode, AutoHost, ShowMenu }
    public enum group { Player }
    public StartUp startUp;
    public GameObject PlayerPrefab;
    public override void Awake()
    {
        Debug.Log("Game Awake");                    
    }
	void Start () {
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
        Destroy(_Player.gameObject);
        if (PlayerPrefab.networkView == null)
        {
            var nw = PlayerPrefab.AddComponent<NetworkView>();
            nw.stateSynchronization = NetworkStateSynchronization.Unreliable;
            nw.observed = PlayerPrefab.GetComponent<Player>();
        }
        Network.Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity, (int)group.Player);
    }
	void Update () {
        if (Input.GetMouseButtonDown(0) && Network.peerType != NetworkPeerType.Disconnected)
            Screen.lockCursor = true;
        if (Input.GetMouseButtonDown(1))
            Screen.lockCursor = false;
        if (Input.GetKeyDown(KeyCode.Escape))
            Screen.lockCursor = false;
        if (Input.GetKeyDown(KeyCode.Tab))
            Screen.lockCursor = !Screen.lockCursor;
	}
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Debug.Log("Disconnected");        
        _GameGui.enabled = true;
    }
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }

    void OnRenderObject()
    {
        LineMaterial.SetPass(0);
        GL.LoadOrtho();
        GL.Begin(GL.LINES);
        GL.Color(Color.green);
        foreach (var a in list)
            GL.Vertex(Vector3.one / 2 + new Vector3(a.x / Screen.width, a.y / Screen.height));
        GL.End();
    }
    
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////7
    
    const float d = 3, len = 5;
    Vector3[] list = new[]{ 
            Vector3.left *d, Vector3.left*(d+len),
            Vector3.up *d, Vector3.up*(d+len),
            Vector3.right *d, Vector3.right*(d+len),
            Vector3.down *d, Vector3.down*(d+len)};

    void OnServerInitialized() { OnConnected(); }
    void OnConnectedToServer() { OnConnected(); }

    static Material lineMaterial;

    static Material LineMaterial
    {
        get
        {
            if (!lineMaterial)
            {
                lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
                    "SubShader { Pass { " +
                    "    Blend SrcAlpha OneMinusSrcAlpha " +
                    "    ZWrite Off Cull Off Fog { Mode Off } " +
                    "    BindChannels {" +
                    "      Bind \"vertex\", vertex Bind \"color\", color }" +
                    "} } }");
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            }
            return lineMaterial;
        }
    }
}
