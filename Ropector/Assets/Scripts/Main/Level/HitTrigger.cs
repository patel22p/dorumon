using UnityEngine;
using System.Collections;

public class HitTrigger : bs {
    public Animation[] animationToPlay;
    void OnCollisionEnter(Collision coll)
    {
        PlayAnimation();
        //networkView.RPC("PlayAnimation", RPCMode.All);
    }
    [RPC]
    private void PlayAnimation()
    {
        foreach (var a in animationToPlay)
            a.Play();
    }
}
