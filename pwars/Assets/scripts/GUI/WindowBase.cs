using System;
using UnityEngine;

public interface IWindow {
    void Action(string name, params object[] param);
}
public class WindowBase : Base2
{
    
    public void Action(string name, params object[] param)
    {
        controller.Action(name, param);
    }
    public void Action(string name)
    {
        controller.Action(name);
    }
    //public void ActionAll(string name)
    //{
    //    foreach (Base a in GameObject.FindObjectsOfType(typeof(Base)))
    //        a.SendMessage(name, SendMessageOptions.DontRequireReceiver);
    //}    
    //new public bool enabled { get { return base.enabled; } set { if (enabled) Show(); else Hide(); } }

    public IWindow controller;


    public void Toggle(IWindow obj)
    {
        
        if (!enabled)        
            Show(obj);
        else
            Hide();

    }
    public void Show(IWindow controller)
    {
        lockCursor = false;
        SendMessageUpwards("HideWindow", SendMessageOptions.DontRequireReceiver);        
        enabled = true;
        this.controller = controller;                
    }
    public void ShowDontHide(IWindow controller)
    {        
        enabled = true;
        this.controller = controller;
    }

    public void ShowOnTop(IWindow controller)
    {        
        enabled = true;
        this.controller = controller;
    }

    [FindAsset("mouseover")]
    public AudioClip mouseOver;
    [FindAsset("click")]
    public AudioClip mouseclick;
    public void onOver()
    {
        _Loader.audio.PlayOneShot(mouseOver);
    }
    public void onButtonClick()
    {
        _Loader.audio.PlayOneShot(mouseclick);
    }

    public void Hide()
    {
        enabled = false;
    }
    void HideWindow()
    {
        enabled = false;
    }
}