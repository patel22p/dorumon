using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }
    Rect windowRect = new Rect(20, 20, 120, 50);

    void OnGUI()
    {
        windowRect = GUI.Window(0, windowRect, DoMyWindow, "My Window");
    }

    // Make the contents of the window
    void DoMyWindow(int windowID)
    {
        GUI.Button(new Rect(10, 20, 100, 20), "Can't drag me");
        // Insert a huge dragging area at the end.
        // This gets clipped to the window (like all other controls) so you can never
        //  drag the window from outside it.
        GUI.DragWindow();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
