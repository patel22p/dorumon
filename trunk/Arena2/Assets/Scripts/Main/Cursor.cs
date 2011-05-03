using UnityEngine;
using System.Collections;

public class Cursor : bs
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            Screen.lockCursor = !Screen.lockCursor;
    }
    
}
