using System;
using UnityEngine;


public class Cursor : bs
{
    public void Update()
    {
        if (!Screen.lockCursor) return;
        if (Player == null) return;
        var dir = (pos - Player.pos).normalized;
        if (Input.GetMouseButtonDown(0))
            Player.ropes[0].MouseDown(dir);
        
        if (Input.GetMouseButtonDown(1))
            Player.ropes[1].MouseDown(dir);

        if (Input.GetMouseButtonUp(0))
            Player.ropes[0].MouseUp();

        if (Input.GetMouseButtonUp(1))
            Player.ropes[1].MouseUp();

        Vector2 v = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) ;
        //if (v.magnitude < 20)
        cursorpos += v;
        pos = Player.pos2 + cursorpos;

    }
    Vector2 cursorpos;
}