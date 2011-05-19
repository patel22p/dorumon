using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;

public class Popup : bs {

    public override void Awake()
    {
        this.enabled = false;
        base.Awake();
    }
    void OnGUI()
    {
        Draw(new Vector2(400, 200), Window);
    }
    void Draw(Vector2 w, GUI.WindowFunction f)
    {
        w /= 2;
        Vector2 s = new Vector2(Screen.width, Screen.height) / 2;
        Vector2 p1 = s - w;
        Vector2 p2 = s + w;
        gui.Window(100, Rect.MinMaxRect(p1.x, p1.y, p2.x, p2.y), f, "PopUp");
    }
    internal string popupText = "";
    //public string popupTitle = "Popup";
    void Window(int id)
    {
        GUI.BringWindowToFront(100);
        gui.Label(popupText);
        if (gui.Button("Ok"))
            this.enabled = false; ;
    }
    public void ShowPopup(string text)
    {
        popupText = text;
        enabled = true;
    }
}
