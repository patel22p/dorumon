using System;
using UnityEngine;

public class Patron:bs
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
                Debug.Log("HIt" + h.collider.name);
                Debug.Log(h.rigidbody == null);

                var z = h.collider.gameObject.GetComponent<Zombie>();
                if (z != null)
                {
                    z.Damage(100);
                    Debug.Log("hitz");
                }
                GameObject.Destroy(this.gameObject);
            }

        }
        oldpos = pos;
    }
}