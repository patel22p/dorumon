using System.Linq;
using UnityEngine;
using System.Collections;

public class Game : bs
{
    public override  void Awake()
    {
        _Game = this;
    }

    public void Start()
    {
    }

    public Transform Player;
    public Transform Spawn;
    public Transform Death;
    public Transform Shadow;
    public Transform Cam;
    private Vector3 mouseOffset;
    private Vector3 oldMouse;
    public Vector2 MouseSensivity = Vector2.one;
    public float Clamp = 40;
    public float CameraZoom = 1;
    public BloomAndLensFlares bmf;
    public GlowEffect glow;
    public void FixedUpdate()
    {
        
        if (Input.GetMouseButton(0))
        {
            var v = (oldMouse - Input.mousePosition);
            v.x *= MouseSensivity.x;
            v.y *= MouseSensivity.y;
            mouseOffset += v;
            mouseOffset = Vector3.ClampMagnitude(mouseOffset, Clamp);
            transform.RotateAround(Player.position, Vector3.left, v.y);
            transform.RotateAround(Player.position, Vector3.forward, v.x);
            transform.Rotate(Vector3.up, -transform.rotation.eulerAngles.y);
        }

        CameraZoom += Input.GetAxis("Mouse ScrollWheel");
        //Cam.transform.position = Vector3.Lerp(Cam.transform.position, Player.position, Time.deltaTime * CamSmoth);
        Cam.transform.position = Player.position;
        Cam.transform.localScale = Vector3.one * CameraZoom;
        oldMouse = Input.mousePosition;

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

    public float Power = 1;
    public float SoundSmooth = 5;
    public void Update()
    {
        float[] numArray = new float[256];
        AudioListener.GetSpectrumData(numArray, 0, FFTWindow.BlackmanHarris);
        print(numArray.Length);
        glow.glowIntensity += numArray[0] * Power;
        //glow.glowIntensity = bmf.bloomIntensity;
        glow.glowIntensity = Mathf.Lerp(glow.glowIntensity, 0, Time.deltaTime * SoundSmooth); 
        //print(numArray[freq]);
    }




}
