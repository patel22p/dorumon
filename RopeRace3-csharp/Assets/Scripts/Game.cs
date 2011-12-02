using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : Bs {
    public override void Awake()
    {
        _Game = this;
        _Camera = cam.GetComponentInChildren<Camera>();
        base.Awake();
    }
	void Start () {
        //CurNode = Path.nodes.First();
        foreach (Rigidbody a in GameObject.FindObjectsOfType(typeof(Rigidbody)))
        {
            if (a.gameObject.GetComponent<Player>() == null)
                obsToRotate.Add(a.transform);
        }
    }
    public Path Path;
    public Transform level;
    public Player pl;
    public Transform cam;
    
    public Transform Death;
    
    public Node CurNode;
    public List<Transform> obsToRotate;
    void FixedUpdate()
    {

        var lr = pl.rigidbody.velocity.x > 0;
        CurNode = CurNode.Nodes.FirstOrDefault(a => Vector3.Distance(a.position, pl.pos) < 2f) ?? CurNode;
        var nextbone = CurNode.Nodes.Add(CurNode).FirstOrDefault(a =>
            (lr && pl.posx < a.position.x || !lr && pl.posx >= a.position.x)
            && Vector3.Distance(a.position, pl.pos) > 2f);

        if (nextbone != null)
        {
            Debug.DrawLine(pl.pos, nextbone.position, Color.red);
            var angle = lr ? clamp(Quaternion.LookRotation(pl.pos - nextbone.position).eulerAngles.y + 90) :
                clamp(Quaternion.LookRotation(nextbone.position - pl.pos).eulerAngles.y + 90);
            //print(angle);
            angle = angle * Time.deltaTime * camrotspeed;
            foreach (var a in obsToRotate)
                a.RotateAround(pl.pos, -Vector3.up, angle);

            cam.position = Vector3.Lerp(cam.position, pl.pos, Time.deltaTime * camspeed);           
        }
        if (pl.position.y < Death.position.y)
        {
            pl.position = CurNode.PrevNode.pos + Vector3.up;
            pl.rigidbody.velocity = Vector3.zero;
        }
    }

    public float camspeed=1;
    public float camrotspeed = 1;
}
