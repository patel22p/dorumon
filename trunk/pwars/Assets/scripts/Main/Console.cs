using System;
using UnityEngine;
using System.Text;
public class Console  : Base ,IGUI
{
    public static StringBuilder log = new StringBuilder();
    Rect r;
    protected override void Awake()
    {
        base.Awake();
        Application.RegisterLogCallback(onLog);
    }
    int errorcount;
    void onLog(string condition, string stackTrace, LogType type)
    {
        try
        {
            if (type == LogType.Exception || type == LogType.Error) errorcount++;
            string s = "fps:" + _Console.fps + "Type:" + type + "\r\n" + condition + "\r\n" + stackTrace + "\r\n";
            Console.log.AppendLine(s);
        }
        catch { }
    }

    public void OnGUI() 
    {         
        if (e)
        {
            GUI.skin = _Loader._skin;
            r = new Rect(0, 0, Screen.width, Screen.height);
            GUI.Window(-1, r, Window, "Console");            
        }
        GUILayout.Label("фпс: " + fps + " Ошибки:" + errorcount);            
    }
    public int fps;
    void Window(int id)
    {
        GUI.Box(r, "");
        GUI.TextField(r, log.ToString(),GUI.skin.customStyles[8]);
        GUI.BringWindowToFront(-1);
        GUI.FocusWindow(-1);
    }
    bool e;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            e = !e;
        if (_TimerA.TimeElapsed(500))
            fps = (int)_TimerA.GetFps();
    }
    

    //public new static void write(string s)
    //{
    //    UnityEngine.Debug.Log(s);        
    //    text = s + "\r\n" + text;
    //}

}

