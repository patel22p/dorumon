using UnityEngine;
using System.Collections;
public class MessageWindow : WindowBase
{
    public bool chat;
    
    public Vk.user user;
    public override void Dispose()
    {
        _Vkontakte.windows.Remove(user.uid);
        base.Dispose();
    }

    protected override void OnGUI()
    {
        
        rect = GUILayout.Window(id, rect, Window, chat ? "Chat Window" : user.nick, GUILayout.Height(300), GUILayout.Width(400));
        base.OnGUI();
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
        GUILayout.BeginHorizontal();
        if (!chat) GUILayout.Label(user.texture, GUILayout.Height(40));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.TextArea(output, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
        if (chat) GUILayout.Label(_Vkontakte.chatuserstext, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
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
