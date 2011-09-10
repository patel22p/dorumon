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
        if (startUp != StartUp.OfflineMode)
            DestroyImmediate(_Player.gameObject);
    }
	void Start () {
        

        if (startUp != StartUp.ShowMenu)
            _GameGui.enabled = false;
        

        if (startUp == StartUp.AutoHost)
        {            
            if (Network.InitializeServer(6, 80, false) != NetworkConnectionError.NoError)
                Network.Connect("127.0.0.1", 80);
            return;
        }
	}
    

    void OnConnected()
    {
        var nw = PlayerPrefab.AddComponent<NetworkView>();
        nw.stateSynchronization = NetworkStateSynchronization.Unreliable;
        nw.observed = PlayerPrefab.GetComponent<Player>();
        Network.Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity, (int)group.Player);
    }
	void Update () {
        if (Input.GetKeyDown(KeyCode.Tab))
            Screen.lockCursor = !Screen.lockCursor;
	}


    void OnRenderObject()
    {
        LineMaterial.SetPass(0);
        //GL.PushMatrix();
        GL.LoadOrtho();
        GL.Begin(GL.LINES);
        GL.Color(Color.green);

        Vector3 c = new Vector3(Screen.width, Screen.height) / 2;

        foreach (var a in list)
        {
            GL.Vertex(Vector3.one / 2 + new Vector3(a.x / Screen.width, a.y / Screen.height));
        }
        GL.End();
        //GL.PopMatrix();
    }

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
