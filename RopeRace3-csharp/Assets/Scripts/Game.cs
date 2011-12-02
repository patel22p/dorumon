using System.Linq;
using UnityEngine;
using System.Collections;

public class Game : Bs {

	void Start () {

    }
    public Transform[] bones;
    public Transform level;
    public Player pl;
    public Transform cam;
    public Transform Death;
    public Transform lastbone;
    void Update()
    {

        var bons = bones.Where(a => a != null).SelectMany(a => a.GetTransforms());
        var lr = pl.rigidbody.velocity.x > 0 ;
        var nextbone = bons.OrderBy(a => Vector3.Distance(a.position, pl.pos)).FirstOrDefault(a =>
            (lr && pl.posx < a.position.x || !lr && pl.posx > a.position.x)
            && Vector3.Distance(a.position, pl.pos) > 2f && Vector3.Distance(a.position, pl.pos) < 8f);

        if (nextbone != null)
        {
            lastbone = nextbone;
            Debug.DrawLine(pl.pos, nextbone.position, Color.red);
            var angle = lr ? clamp(Quaternion.LookRotation(pl.pos - nextbone.position).eulerAngles.y + 90) :
                clamp(Quaternion.LookRotation(nextbone.position - pl.pos).eulerAngles.y + 90);
            //print(angle);
            angle = angle * Time.deltaTime * camrotspeed;
            print(nextbone.forward);
            level.RotateAround(pl.pos, -Vector3.up, angle);
        
        }
        else
            print("CannotFindBone");

        cam.position = Vector3.Lerp(cam.position, pl.pos, Time.deltaTime * camspeed);
        if (pl.position.y < Death.position.y)
        {
            pl.position = lastbone.position+ Vector3.up;
            pl.rigidbody.velocity = Vector3.zero;
        }
    }
    
    public float camspeed=1;
    public float camrotspeed = 1;
}
