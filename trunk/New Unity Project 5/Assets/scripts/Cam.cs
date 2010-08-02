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

    protected override void OnLoaded()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    protected override void OnLateUpdate()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) Screen.lockCursor = !Screen.lockCursor;
        if (localplayer == null || localplayer.isdead) return;                
        if (Screen.lockCursor)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }
        y = ClampAngle(y, yMinLimit, yMaxLimit);

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        //RaycastHit hit;
        Vector3 pos = localplayer.transform.position;        
        Vector3 pos2 = rotation * new Vector3(0.0f, 0.0f, -maxdistance) + pos;
        pos2.y -= yoffset;        
        //if (Physics.Raycast(pos, pos2, out hit, maxdistance, LayerMask.NameToLayer("Untagged")))
        //    transform.position = pos;
        //else
            transform.position = pos2;        
        transform.rotation = rotation;        


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