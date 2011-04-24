using System;
using UnityEngine;
using System.Collections;

public class Menu : bs
{
    public GameObject Box;

    public TextMesh Press;
    public GameObject Text;
    public GameObject cursor;
    public GameObject Plane;

    
    public IEnumerator Start()
    {
        Application.ExternalEval("FB.Canvas.setSize();");

        if (!Application.isEditor)
            _Music.Play("ropector");

        var cs = new GameObject("cubes");

        if (Text.gameObject.active)
            for (int y = 0; y < 10; y++)
                for (int x = -15; x < 15; x++)
                {
                    Vector3 pos = new Vector3(x, y, 0);
                    var cb = (GameObject)Instantiate(Box, pos, Quaternion.identity);
                    cb.transform.parent = cs.transform;
                    yield return null;
                }

        yield return new WaitForSeconds(1);
        Text.rigidbody.isKinematic = false;
        Text.rigidbody.WakeUp();  
    }
    
    void Update()
    {
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, 1 << LayerMask.NameToLayer("Ignore Raycast")))
            cursor.transform.position = hit.point;

    }

}