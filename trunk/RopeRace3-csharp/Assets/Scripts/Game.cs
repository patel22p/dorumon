using System.Linq;
using UnityEngine;
using System.Collections;

public class Game : Bs {

	void Start () {
        //CurNode = Path.nodes.First();
    }
    public Path Path;
    public Transform level;
    public Player pl;
    public Transform cam;
    public Transform Death;
    public Node CurNode;

    void FixedUpdate()
    {
        var tmp = CurNode.Nodes.FirstOrDefault(a => Vector3.Distance(pl.pos, a.pos) <1);
        if (tmp != null) CurNode = tmp;


        if (Input.GetKey(KeyCode.D) && CurNode.NextNode != null)
            pl.rigidbody.AddForce((-pl.pos + CurNode.NextNode.pos).normalized * pl.torq);
        if (Input.GetKey(KeyCode.A) && CurNode.PrevNode != null)
            pl.rigidbody.AddForce((-pl.pos + CurNode.PrevNode.pos).normalized * pl.torq);
        Node nearest = CurNode.Nodes.OrderBy(a => Vector3.Distance(pl.pos, a.pos)).FirstOrDefault();
        Debug.DrawLine(pl.pos, nearest.pos);
        cam.position = Vector3.Lerp(cam.position, pl.pos, Time.deltaTime * camspeed);
        if (pl.position.y < Death.position.y)
        {
            pl.position = CurNode.position + Vector3.up;
            pl.rigidbody.velocity = Vector3.zero;
        }
        pl.rigidbody.angularVelocity = Vector3.Project(pl.rigidbody.angularVelocity, CurNode.transform.right);
        Debug.DrawLine(pl.pos, pl.pos + pl.rigidbody.angularVelocity, Color.red);
        pl.rigidbody.velocity = ZeroY(Vector3.Project(pl.rigidbody.velocity, pl.pos - nearest.pos)) + Vector3.up * pl.rigidbody.velocity.y;
        
    }
    public float camspeed=1;
    public float camrotspeed = 1;
}
