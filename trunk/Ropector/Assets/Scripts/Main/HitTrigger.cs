using UnityEngine;
using System.Collections;

public class HitTrigger : bs {
    public Animation[] animationToPlay;
    void OnCollisionEnter(Collision coll)
    {
        foreach(var a in animationToPlay)
            a.Play();        
    }
}
