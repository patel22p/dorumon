using UnityEngine;
using System.Collections;
public class MessageWindow : Base
{
    public bool chat;
    public Rect rect;
    int id;
    void Start()
    {
        id = Random.Range(0, int.MaxValue);        
        rect = new Rect(0, Screen.height - 200, 0, 0);
    }
    public Vk.user user;
    public override void Dispose()
    {        
            _Vkontakte.windows.Remove(user.uid);
        base.Dispose();
    }

    void OnGUI()
    {
        rect = GUILayout.Window(id, rect, Window, chat ? "Chat Window" : user.nick, GUILayout.Height(300), GUILayout.Width(400));
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            if (input != "")
            {
                input = input.Trim();
                if (chat) _vk.SendChatMsg(localuser.nick + ":" + input);
                else
                {
                    _vk.SendMsg(user.uid, input);
                    output = localuser.nick + ":" + input + "\r\n" + output;
                }
                input = "";                
            }
    } 
    void Window(int i)
    {
        if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
        {
            if (chat)
                enabled = false;
            else
            {
                Dispose();
                Destroy(this);
            }
        }
        if (!chat) GUILayout.Label(user.texture, GUILayout.Height(40));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.TextArea(output, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
        input = GUILayout.TextArea(input, GUILayout.ExpandHeight(false));        
        GUI.DragWindow();
    }
    Vector2 scrollPosition;
    public string input ="";
    public string output = "";


    internal void Write(string p)
    {
        output = p + "\r\n" + output;        
    }
}
