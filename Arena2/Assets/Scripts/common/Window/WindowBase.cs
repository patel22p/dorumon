using System;
using UnityEngine;


public class WindowBase : Base
{
    protected bool AlwaysOnTop;
    [FindAsset("mouseover")]
    public AudioClip mouseOver;
    [FindAsset("click")]
    public AudioClip mouseclick;
    [FindAsset]
    public GUISkin Skin;
    
    void Awake()
    {        
        enabled = false;
    }
    public void Action(object name, params object[] param)
    {
        controller.SendMessage("Action", name, SendMessageOptions.DontRequireReceiver);
    }
    public void Action(object name)
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
        GUI.skin = Skin;
    }
    public virtual void ResetValues() { Debug.Log("Reset Values"); }
    public virtual void Show()
    {
        Screen.lockCursor = false;
        HideAll();
        enabled = true;        
    }

    public void HideAll()
    {
        foreach (var a in transform.parent.GetComponentsInChildren<Transform>())
            a.gameObject.SendMessage("HideWindow", SendMessageOptions.DontRequireReceiver);
    }
    public void ShowDontHide(MonoBehaviour controller)
    {
        enabled = true;
        this.controller = controller;
    }

    public void onOver()
    {
        if (transform.root.audio != null)
            transform.root.audio.PlayOneShot(mouseOver);
    }
    public void onButtonClick()
    {
        if (transform.root.audio != null)
            transform.root.audio.PlayOneShot(mouseclick);
    }
    public void Hide()
    {
        enabled = false;
    }
    protected void HideWindow()
    {
        if(!AlwaysOnTop)
            enabled = false;
    }
}