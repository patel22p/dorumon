using UnityEngine;
using System.Collections;

public class MessageWindow : Base
{

    public Rect rect;
    int id;
    void Start()
    {
        id = Random.Range(0, int.MaxValue);        
        rect = new Rect(0, Screen.height - 200, 200, 200);
    }
    public Vkontakte.user user;
    public override void Dispose()
    {
        _Vkontakte.windows.Remove(user.uid);
        base.Dispose();
    }
    void OnGUI()
    {
        
        rect = GUILayout.Window(id, rect, Window, user.nick);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            if (input != "")
            {
                _vk.SendMsg(user.uid + ":" + input.Trim());
                input = "";
            }
    } 
    void Window(int i)
    {
        if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
        {
            Dispose();
            Destroy(this);
        }
        GUILayout.Label(user.texture);
        
        input = GUILayout.TextArea(input, GUILayout.ExpandHeight(false));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.TextArea(output, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
        GUI.DragWindow();
    }
    Vector2 scrollPosition;
    public string input ="";
    public string output = "";


    internal void Write(string p)
    {
        input += "\r\n" + user.nick + ":" + p;
        
    }
}
