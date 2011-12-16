using UnityEngine;
using System.Collections;

public class Game : bs
{
    public void Start()
    {
    }

    public Transform Player;
    public Transform Spawn;
    public Transform Death;
    public Transform Shadow;
    public Transform Cam;
    public float maxMove = 2;
    public float Smooth = 20;
    private Vector3 mouseOffset;
    private Vector3 nwmouseOffset;
    private Vector3 oldMouse;
    public float mouseSmooth = 5;
    public Vector2 MouseSensivity = Vector2.one;
    public float Clamp = 40;
    
    //public float ClampSpeed = 1;
    public void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            var v = (oldMouse - Input.mousePosition);
            v.x *= MouseSensivity.x;
            v.y *= MouseSensivity.y;
            //v = Vector3.ClampMagnitude(v, ClampSpeed);
            mouseOffset += v;
            mouseOffset = Vector3.ClampMagnitude(mouseOffset, Clamp);
            transform.RotateAround(Player.position, Vector3.left, v.y);
            transform.RotateAround(Player.position, Vector3.forward, v.x);
            transform.Rotate(Vector3.up, -transform.rotation.eulerAngles.y);
            
        }
        oldMouse = Input.mousePosition;
        Quaternion q = Quaternion.Euler(270, 270, 0);
        if (Input.acceleration.magnitude > 0)
        {
            var e = Quaternion.LookRotation(-Input.acceleration).eulerAngles;
            q = Quaternion.Euler(e.x, -e.y, e.z);
        }
        else
        {
            nwmouseOffset = Vector3.Slerp(nwmouseOffset, mouseOffset, Time.deltaTime * mouseSmooth);
            q = Quaternion.Euler(270, 270, 0) * Quaternion.Euler(nwmouseOffset.x, -nwmouseOffset.y, 0);
        }
        
        //rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.Lerp(transform.rotation, q, Time.deltaTime * Smooth), maxMove));
        if (Player.position.y < Death.position.y)
        {
            Player.transform.position = Spawn.position;
            Player.rigidbody.velocity = Vector3.zero;
            Player.rigidbody.angularVelocity = Vector3.zero;
        }
        Shadow.transform.position = Player.transform.position;
        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();
        //print(Input.acceleration);
    }
}
