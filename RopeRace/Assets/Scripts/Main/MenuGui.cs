using System.Linq;
using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;
using System;
public enum MenuAction { StartServer, JoinGame }
public enum Wind { Menu, SelectLevel, ExitToMenuWindow, EditorTools, EditorProps}

public class MenuGui : bs
{
    
    public Wind curwindow;
    public Scene SelectedLevel;
    public bool LoopSameLevel;
    public void Show(Wind window)
    {
        enabled = true;
        this.curwindow = window;
    }
    public void Hide()
    {
        this.enabled = false;
    }

    void OnGUI()
    {
        Screen.lockCursor = false;
        Draw(new Vector2(200, 200), MenuWindow, Wind.Menu);
        Draw(new Vector2(200, 400), SelectLevelWindow, Wind.SelectLevel);        
        Draw(new Vector2(400, 200), ExitToMenuWindow, Wind.ExitToMenuWindow);
        
    }
    string windowTitle;
    void Draw(Vector2 w, GUI.WindowFunction f, Wind wnd)
    {
        if (curwindow == wnd)
        {
            w /= 2;
            Vector2 s = new Vector2(Screen.width, Screen.height) / 2;
            Vector2 p1 = s - w;
            Vector2 p2 = s + w;
            gui.Window((int)curwindow, Rect.MinMaxRect(p1.x, p1.y, p2.x, p2.y), f, windowTitle);
            windowTitle = curwindow + "";
        }
    }
    public void MenuWindow(int id)
    {
        gui.Label("Nick:");
        _Loader.nick = gui.TextField(_Loader.nick, 30);
        if (gui.Button("Start New Game"))
            Show(Wind.SelectLevel);
        if (gui.Button("Refresh Server List"))
            _Menu.RefreshServerList();
        foreach (HostData host in MasterServer.PollHostList())
            if (gui.Button("Join to " + host.gameName))
            {
                _Popup.ShowPopup("Trying Connect to " + host.gameName);
                var er = Network.Connect(host);
                if (er != NetworkConnectionError.NoError)
                    _Popup.ShowPopup(er + "");
            }
        
        gui.Label("Quality:");
        gui.BeginHorizontal();
        if (gui.Button("<"))
            QualitySettings.DecreaseLevel();
        gui.Label(QualitySettings.currentLevel + "");
        if (gui.Button(">"))
            QualitySettings.IncreaseLevel();                
        
        gui.EndHorizontal();
    }

    


    
    public void SelectLevelWindow(int id)
    {
        
        if (gui.Button("back"))
            Show(Wind.Menu);
        gui.Label("Select Level");
        var names = Enum.GetNames(typeof(Scene));
        SelectedLevel = (Scene)Mathf.Clamp(gui.SelectionGrid((int)SelectedLevel, names, 1), 1, names.Length - 1);
        if (gui.Button("Start Game"))
            _Menu.Action(MenuAction.StartServer);

    }
        
    public void ExitToMenuWindow(int id)
    {
        gui.Label("Exit To Menu?");
        if (gui.Button("Cancel"))
            Hide();
        if (gui.Button("Ok"))
            Network.Disconnect();
    }    
}