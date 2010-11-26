using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class Door : Base
{
    public int score;
    public bool opened;
    //bool bought { get { return animation[0].time != 0; } }
    float tm;
    
    void Start()
    {
        name = "door," + score;
    }
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        networkView.RPC("RPCOpen", np, opened);
        base.OnPlayerConnected1(np);
    }
     
    [RPC]
    public void RPCOpen()
    {
        CallRPC();
        animation.Play();
        PlaySound("dooropen",10);
        opened = true;
    }
}
