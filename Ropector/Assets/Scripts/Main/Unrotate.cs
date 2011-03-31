using UnityEngine;
using System.Collections;
[AddComponentMenu("Game/Unrotate")]
public class Unrotate : bs
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.identity;
    }
}
