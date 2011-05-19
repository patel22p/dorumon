//using UnityEngine;
//using System.Collections;

//public class Jumper : bs {

//    // Use this for initialization
//    void Start () {
//        animation.wrapMode = WrapMode.Clamp;
//    }
//    //void OnTriggerEnter(Collider coll)
//    //{
//    //    Debug.Log("hit");
//    //    var pl = other.gameObject.GetComponent<Player>();
//    //    if (pl != null)
//    //    {
//    //        Debug.Log("pl");
//    //        pl.rigidbody.AddForce(Vector3.up * 1000);
//    //        animation.Play();
//    //    }
//    //}
//    public float force = 1;
//    void OnCollisionEnter(Collision coll)
//    {

//        var pl = coll.gameObject.GetComponent<Player>();
//        if (pl != null)
//        {
//            Debug.Log("UP");
//            pl.pos += Vector3.up * 1f;
//            pl.rigidbody.AddForce(Vector3.up * 1000*force);
//            animation.Play();
//        }
//    }
//    // Update is called once per frame
//    void Update () {
	
//    }
//}
