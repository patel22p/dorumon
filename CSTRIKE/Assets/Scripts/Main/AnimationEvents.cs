using UnityEngine;
using System.Collections;

public class AnimationEvents : Bs {
    
    public void Walk()
    {
        transform.parent.SendMessage("WalkSound");
    }
}
