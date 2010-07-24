using UnityEngine;
using System.Collections;
public class Box : Base
{
    Vector3 spawn;
    Quaternion rotation;
    protected override void Start()
    {
        spawn = this.transform.position;
        rotation = this.transform.rotation;
        rigidbody.angularDrag = 10;
        if (collider is MeshCollider)
            ((MeshCollider)collider).convex = true;
        FixedJoint fx = gameObject.GetComponent<FixedJoint>();
        if (null != fx)
            fx.breakForce = 20;
    }
    public Bounds bounds { get { return GameObject.Find("Cube").collider.bounds; } }
    protected override void Update()
    {
        if (!bounds.Contains(this.transform.position))
            Reset();
    }
    public void Reset()
    {
        this.transform.rigidbody.velocity = Vector3.zero;
        this.transform.rigidbody.angularVelocity = Vector3.zero;
        //Trace.Log(bounds.min + "<<<<<<<<>>>>>>>>>>>" + bounds.max);
        this.transform.position = Player.SpawnPoint();
        //    Random.Range(bounds.min.x, bounds.max.x), bounds.max.y,
        //    Random.Range(bounds.min.z, bounds.max.z));
        //this.transform.rotation = rotation;
    }
    [RPC]
    void SetOwner(NetworkPlayer owner, NetworkMessageInfo a)
    {
        this.CallRPC(owner, a);
        //if (b.isMine)
        //        b.RPC("SetOwner", RPCMode.All, Network.player);
        foreach (NetworkView b in this.GetComponents<NetworkView>())
            b.observed = null;
        a.networkView.observed = this.rigidbody;
        OwnerID = owner;
    }
}