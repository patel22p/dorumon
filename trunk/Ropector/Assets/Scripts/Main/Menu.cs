using System;
using UnityEngine;
using System.Collections;

public class Menu : bs
{
    public GameObject Box;
    public GameObject Text;
    [FindTransform(scene=true)]
    public GameObject cursor;
    [FindTransform(scene = true)]
    public GameObject Plane;
    public IEnumerator Start() //creates cubes in menu
    {
        Application.ExternalEval("FB.Canvas.setSize();");

        if (Music != null && !Application.isEditor)
            Music.Play("ropector");
        _MenuWindow.Show(_Loader);
        var cs = new GameObject("cubes");

        if (Text.gameObject.active)
            for (int y = 0; y < 10; y++)
                for (int x = -15; x < 15; x++)
                {
                    Vector3 pos = new Vector3(x, y, 0);
                    var cb = (GameObject)Instantiate(Box, pos, Quaternion.identity);
                    var drg = (Dragable)cb.AddComponent(typeof(Dragable));
                    drg.Plane = Plane;
                    drg.cursor = cursor;
                    cb.transform.parent = cs.transform;
                    yield return null;
                }

        yield return new WaitForSeconds(1);
        Text.rigidbody.isKinematic = false;
        Text.rigidbody.WakeUp();
    }
    
    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.P)) Debug.Break();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, 1 << LayerMask.NameToLayer("Ignore Raycast")))
            cursor.transform.position = hit.point;        

    }

}