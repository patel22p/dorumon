using System.Linq;
using System;
using UnityEngine;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;
public class Console : bs
{
    static StringBuilder log = new StringBuilder();
    Rect r;
    internal int errorcount;
    internal int exceptionCount;
    public Console()
    {

    }
    public override void Awake()
    {
        Application.RegisterLogCallback(onLog);                
        base.Awake();
        
    }
    void onLog(string condition, string stackTrace, LogType type)
    {
        try
        {
            if (type == LogType.Error) errorcount++;
            if (type == LogType.Exception) exceptionCount++;
            if (new LogType[] { LogType.Error, LogType.Exception, LogType.Warning }.Contains(type))
                log.AppendLine(stackTrace + condition);
            else
            {
                Match m = Regex.Match(stackTrace, @"^\w+\:\w+", RegexOptions.Multiline);
                if (m.Success)
                    log.AppendLine(string.Format("{0,-50}{1}", m.Value, condition));
                else
                    log.AppendLine(condition);
            }

            if (log.Length > 6000)
                log.Remove(0, log.Length - 6000);            
        }
        catch { }
    }
    public void OnGUI()
    {
        GUI.skin = _Loader.Skin;
        r = new Rect(0, 0, Screen.width, Screen.height);
        GUI.Window(-1, r, Window, "Console");
        
    }
    void Window(int id)
    {        
        //GUI.Box(r, "");                
        GUILayout.Label("Warnings:" + errorcount + "\tErrors:" + exceptionCount + "\tRPC:" + _Loader.rpcCount + "\tVersion " + _Loader.version);        
        GUI.TextField(new Rect(0, 50, r.width, r.height - 50), log.ToString(), GUI.skin.customStyles[8]);
        GUI.BringWindowToFront(-1);
        GUI.FocusWindow(-1);
    }
}