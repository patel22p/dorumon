using System;
using UnityEngine;

public class Bullet:bs
{
    void Start()
    {
        
    }
    Vector3 oldpos;
    float tm;
    
    public void Update()
    {
        tm += Time.deltaTime;
        pos += rot * Vector3.forward * 100 * Time.deltaTime;

        if (tm > 3)            
            GameObject.Destroy(this.gameObject);
        if (oldpos != Vector3.zero)
        {
            var v = oldpos - pos;

            RaycastHit h;
            //Debug.Log(v.magnitude);
            
            if (Physics.Raycast(new Ray(pos, v), out h, v.magnitude + 1,~(1 << LayerMask.NameToLayer("Player"))))
            {
                var z = h.collider.gameObject.GetComponent<Zombie>();
                if (z != null)
                {
                    //if (networkView.isMine)
                    z.networkView.RPC("Damage", RPCMode.All, 30, .3f, id);
                    Debug.Log("hitz");
                }
                GameObject.Destroy(this.gameObject);
            }

        }
        oldpos = pos;
    }
}