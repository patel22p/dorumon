using UnityEngine;
using System.Collections;

public class Score : bs
{
    private Vector3 vel;

    private Vector3 oldpos;
    public void Update()
    {
        if (oldpos == Vector3.zero) oldpos = pos;
        oldpos = pos;
        if ((_Game.Player.position - this.transform.position).magnitude < 1)
        {
            Destroy(this.gameObject);
        }
        if ((_Game.Player.position - this.transform.position).magnitude < 3)
        {
            vel +=  -ZeroYNorm(_Game.Player.position - this.transform.position) * .2f;
        }
        transform.localPosition += vel * Time.deltaTime;
        vel *= .86f;
        Ray r= new Ray(oldpos,(pos-oldpos).normalized);
        RaycastHit h;
        if (Physics.Raycast(r, out h, .4f, 1 << LayerMask.NameToLayer("Level")))
        {
            print("hit");
            pos = oldpos;
        }
   
    }
}
