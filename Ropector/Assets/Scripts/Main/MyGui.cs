using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;
public enum MenuAction { StartServer, JoinGame }
public enum Wind { Menu, SelectLevel, PopUp, ExitToMenuWindow, Disconnected, EditorTools, EditorProps }
public class MyGui : bs
{
    
    public Wind curwindow;
    public void Show(Wind window)
    {
        enabled = true;
        this.curwindow = window;
    }
    public void Hide()
    {
        this.enabled = false;
    }

    void Start()
    {
        string[] levels = new string[Application.levelCount - 1];
        for (int i = 0; i < Application.levelCount - 1; i++)
            levels[i] = "Level " + (i + 1);
        _MyGui.levels = levels;
    }
    void OnGUI()
    {
        Screen.lockCursor = false;
        Draw(new Vector2(200, 200), MenuWindow, Wind.Menu);
        Draw(new Vector2(200, 400), SelectLevel, Wind.SelectLevel);
        Draw(new Vector2(400, 200), PoUpWindow, Wind.PopUp);
        Draw(new Vector2(400, 200), ExitToMenuWindow, Wind.ExitToMenuWindow);
        Draw(new Vector2(400, 200), LostConnection, Wind.Disconnected);
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
        if (gui.Button("Join Game"))
            _Menu.Action(MenuAction.JoinGame);
        gui.Label("Quality:");
        gui.BeginHorizontal();
        if (gui.Button("<"))
            QualitySettings.DecreaseLevel();
        gui.Label(QualitySettings.currentLevel + "");
        if (gui.Button(">"))
            QualitySettings.IncreaseLevel();        
        gui.EndHorizontal();
        if (gui.Button("Level Editor"))
        {
            this.Hide();
            Application.LoadLevel((int)Scene.mapEditor);
        }
    }
    public string[] levels;
    public int SelectedLevel;
    internal bool LoopSameLevel = false;
    public void SelectLevel(int id)
    {
        LoopSameLevel = gui.Toggle(LoopSameLevel, "Loop Same Level");
        if (gui.Button("back"))
            Show(Wind.Menu);
        SelectedLevel = gui.SelectionGrid(SelectedLevel, levels, 1);
        if (gui.Button("Start Game"))
            _Menu.Action(MenuAction.StartServer);

    }
    public string popupText = "";
    public string popupTitle = "window";
    public void PoUpWindow(int id)
    {
        windowTitle = popupTitle;
        gui.Label(popupText);
    }
    public void ExitToMenuWindow(int id)
    {
        gui.Label("Exit To Menu?");
        if (gui.Button("Cancel"))
            Hide();
        if (gui.Button("Ok"))
            Network.Disconnect();
    }
    public string disconnectedtext;
    public void LostConnection(int id)
    {
        gui.Label(disconnectedtext);
        if (gui.Button("Ok"))
            _MyGui.Show(Wind.Menu);
    }
}