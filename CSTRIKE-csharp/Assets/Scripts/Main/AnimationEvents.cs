using UnityEngine;
using System.Collections;

public class AnimationEvents : Bs
{
    public void OnReloaded()
    {
        transform.parent.SendMessage("OnReloaded");
    }
    public void Walk()
    {
        transform.parent.SendMessage("WalkSound");
    }
    public void ClipOut()
    {
        transform.parent.SendMessage("ClipOutSound");
    }
    public void ClipIn()
    {
        transform.parent.SendMessage("ClipInSound");
    }
    public void Draw()
    {
        transform.parent.SendMessage("DrawSound");
    }

    public void BoltPull()
    {
        transform.parent.SendMessage("BoltPullSound");
    }
    public void Hit()
    {
        transform.parent.SendMessage("Attack");
    }
}
