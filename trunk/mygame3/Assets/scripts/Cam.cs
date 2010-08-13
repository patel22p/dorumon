using UnityEngine;
using System.Collections;
public class Cam : Base
{
    public IPlayer localplayer;

    public float maxdistance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -90f;
    public float yMaxLimit = 90f;

    float x = 0.0f;
    float y = 0.0f;
    public float yoffset = -3;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }
    public bool spectator;
    void LateUpdate()
    {
        transform.Find("pointer").rotation = Quaternion.LookRotation(this.transform.position- Find<Tower>().transform.position);

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) Screen.lockCursor = !Screen.lockCursor;

        if (Screen.lockCursor)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }

        y = ClampAngle(y, yMinLimit, yMaxLimit, 45);
        if (spectator || localplayer == null || localplayer.isdead)
        {
            Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection.Normalize();
            transform.rotation = Quaternion.Euler(y, x, 0);
            transform.position += moveDirection / 3;
        }
        else
        {
            if (localplayer is CarController)
                x = ClampAngle(x, yMinLimit, yMaxLimit, 30);
            bool car = localplayer is CarController;
            Quaternion rotation = Quaternion.Euler(y, x + (car ? localplayer.transform.rotation.eulerAngles.y : 0), 0);
            Vector3 pos = localplayer.transform.position;
            Vector3 pos2 = rotation * new Vector3(0.0f, 0.0f, -maxdistance) + pos;
            pos2.y -= yoffset;
            transform.position = pos2;
            transform.rotation = rotation;
        }
    }

    public static float ClampAngle(float angle, float min, float max, float clamp)
    {
        if (angle < -clamp)
            angle = -clamp;
        if (angle > clamp)
            angle = clamp;
        return Mathf.Clamp(angle, min, max);
    }


}