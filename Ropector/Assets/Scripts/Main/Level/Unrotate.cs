using UnityEngine;
using System.Collections;

public class Unrotate : bs
{
    void Start()
    {
    }
    void Update()
    {
        transform.rotation = Quaternion.identity;
    }
}
