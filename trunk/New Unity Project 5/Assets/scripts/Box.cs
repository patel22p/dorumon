using UnityEngine;
public class Box : Base
{
    protected override void OnCollisionEnter(Collision collisionInfo)
    {
        //Base b = collisionInfo.gameObject.GetComponent<Base>();
        //if (b.OwnerID == null)
        //{
        //    foreach (NetworkView otherView in this.GetComponents<NetworkView>())
        //        otherView.observed = null;
        //    b.myNetworkView.observed = b.rigidbody;
        //}
    }
}