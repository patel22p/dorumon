using UnityEngine;
using System.Collections;
public class Cam : Base
{
    public Player localplayer { get { return Find<Player>("LocalPlayer"); } }

    public float maxdistance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -90f;
    public float yMaxLimit = 90f;

    float x = 0.0f;
    float y = 0.0f;
    public float yoffset = -3;

    protected override void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    protected override void LateUpdate()
    {
        if (localplayer == null || localplayer.isdead) return;

        Vector3 pos = localplayer.transform.position;
        pos.y -= yoffset;
        float distance = maxdistance;
        if (Screen.lockCursor)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }
        y = ClampAngle(y, yMinLimit, yMaxLimit);

        Quaternion rotation = Quaternion.Euler(y, x, 0);

        RaycastHit hit;
        if (Physics.Raycast(pos, transform.position - pos, out hit, maxdistance, LayerMask.NameToLayer("Default")))
        {
            distance = hit.distance;
        }
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + pos;

        transform.rotation = rotation;
        transform.position = position;


    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }


}