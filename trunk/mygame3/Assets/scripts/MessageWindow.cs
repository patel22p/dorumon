using UnityEngine;
using System.Collections;
public class MessageWindow : WindowBase
{
    
    
    public z0Vk.user user;
    public override void Dispose()
    {
        _Vkontakte.windows.Remove(user.uid);
        base.Dispose();
    }
    void Start()
    {
        title = user.nick;
        size = new Vector2(400, 300);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            if (input != "")
            {
                input = input.Trim();                
                    _vk.SendMsg(user.uid, input);
                    output = localuser.nick + ":" + input + "\r\n" + output;
                input = "";
            }
    }
    protected override void Window(int i)
    {
        if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
        {
                Dispose();
                Destroy(this);
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label(user.texture, GUILayout.Height(40));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.TextArea(output, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
        
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
