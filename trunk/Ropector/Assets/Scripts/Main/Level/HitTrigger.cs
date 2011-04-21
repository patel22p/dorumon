using UnityEngine;
using System.Collections;

public class HitTrigger : bs {
    public PhysAnimObj[] animationToPlay;
    void OnCollisionEnter(Collision coll)
    {
        //PlayAnimation();
        networkView.RPC("PlayAnimation", RPCMode.All);
    }
    [RPC]
    private void PlayAnimation()
    {
        foreach (var a in animationToPlay)
            a.AnimObj.animation.Play();
    }
}
