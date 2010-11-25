using UnityEngine;
using System.Collections;
public class Door : Base
{
    public int score;
    void OnMouseEnter()
    {
        _GameWindow.CenterText.text = "Press F to open door, you must have " + score + " score";
        lookat = true;
    }
    bool lookat;
    //bool bought { get { return animation[0].time != 0; } }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && lookat)
            animation.Play();
    }
    void OnMouseExit()
    {
        lookat = false;
        _GameWindow.CenterText.text = "";
    }
}
