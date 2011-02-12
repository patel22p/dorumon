using System;
using UnityEngine;


public class WindowBase : Base2
{
    protected bool AlwaysOnTop;
    [FindAsset("mouseover")]
    public AudioClip mouseOver;
    [FindAsset("click")]
    public AudioClip mouseclick;
    void Awake()
    {
        enabled = false;
    }
    public void Action(string name, params object[] param)
    {
        controller.SendMessage("Action", name, SendMessageOptions.DontRequireReceiver);
    }
    public void Action(string name)
    {
        controller.SendMessage("Action", name, SendMessageOptions.DontRequireReceiver);
    }
    public MonoBehaviour controller;
    public void Toggle(MonoBehaviour obj)
    {
        
        if (!enabled)        
            Show(obj);
        else
            Hide();

    }
    public void Close()
    {
        Hide();
        Action("Close");
    }
    public void Show(MonoBehaviour controller)
    {
        this.controller = controller;
        Show();
    }
    public virtual void OnGUI()
    {
    }
    public virtual void ResetValues() { Debug.Log("Reset Values"); }
    public virtual void Show()
    {
        lockCursor = false;
        foreach(var a in transform.parent.GetComponentsInChildren<Transform>())
            a.gameObject.SendMessage("HideAll", SendMessageOptions.DontRequireReceiver);
        enabled = true;        
    }
    public void ShowDontHide(MonoBehaviour controller)
    {
        enabled = true;
        this.controller = controller;
    }

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
    protected void HideAll()
    {
        if(!AlwaysOnTop)
            enabled = false;
    }
}