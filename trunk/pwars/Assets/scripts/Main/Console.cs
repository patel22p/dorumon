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
        base.Awake();

    }
    public void onLog(string c, string stackTrace, LogType type)
    {

        var condition = c.Replace("\r\n", " ") + "\r\n";
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
        } catch (Exception e) { log.AppendLine(e + ""); }
    }
    public void OnGUI()
    {
        GUI.skin = _Loader.Skin;
        r = new Rect(0, 0, Screen.width, Screen.height);
        GUI.Window(4000, r, Window, "Console");
        
    }
    void Window(int id)
    {        
        //GUI.Box(r, "");                
        GUILayout.Label("Warnings:" + errorcount + "\tErrors:" + exceptionCount + "\tRPC:" + _Loader.rpcCount + "\tVersion " + _Loader.Version);        
        GUI.TextField(new Rect(0, 50, r.width, r.height - 50), log.ToString(), GUI.skin.customStyles[8]);
        GUI.BringWindowToFront(4000);
        GUI.FocusWindow(4000);
    }
}