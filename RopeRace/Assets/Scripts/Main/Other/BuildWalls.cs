using System.Linq;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using doru;

public class BuildWalls : bs
{
    public GameObject Box;

    public TextMesh Press;
    public GameObject Text;
    public GameObject cursor;
    public GameObject Plane;

    public IEnumerator Start()
    {
        InitLoader();
        return BuildWall();
    }

    void Update()
    {
        UpdateOther();
    }
    private void UpdateOther()
    {        
        Press.gameObject.active = !_MenuGui.enabled;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, 1 << LayerMask.NameToLayer("Ignore Raycast")))
            cursor.transform.position = hit.point;
    }

    public IEnumerator BuildWall()
    {
        
        //if (!Application.isEditor)            

        var cs = new GameObject("cubes");

        if (Text.gameObject.active)
            for (int y = 0; y < 10; y++)
                for (int x = -15; x < 15; x++)
                {
                    Vector3 pos = new Vector3(x, y, 0);
                    var cb = (GameObject)Instantiate(Box, pos, Quaternion.identity);
                    cb.transform.parent = cs.transform;
                    var drg = (Dragable)cb.AddComponent(typeof(Dragable));
                    drg.Plane = Plane;
                    drg.cursor = cursor;
                    yield return null;
                }

        yield return new WaitForSeconds(1);
        Text.rigidbody.isKinematic = false;
        Text.rigidbody.WakeUp();
    }

}