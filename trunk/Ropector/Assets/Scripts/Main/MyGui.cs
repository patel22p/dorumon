using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;
public enum MenuAction { wait, join, single }
public class MyGui : bs {

	void Start () {
	    
	}
    public enum Wind { Menu, SelectLevel, Popup, ExitToMenu }
    public Wind window;
    void OnGUI()
    {
        Vector2 s = new Vector2(Screen.width, Screen.height) / 2;
        Vector2 w = new Vector2(200, 100) / 2;
        Vector2 p1 = s - w;
        Vector2 p2 = s + w;
        if (window == Wind.Menu)
            gui.Window(0, Rect.MinMaxRect(p1.x, p1.y, p2.x, p2.y), MenuWindow, "Menu");
    }
    
    public void MenuWindow(int id)
    {
        if (gui.Button("Start New Game"))
        {

        }
        gui.Button("Join Game");
    }
    private void SendAction(MenuAction a)
    {
        transform.parent.SendMessage("Action", a);
    }

	void Update () {
	
	}
}
