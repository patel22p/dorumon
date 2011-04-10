using UnityEngine;
using System.Collections;

public class DebugUI : MonoBehaviour {

	public float updateInterval; 			// How often we like to update the statistics
	private float timer;						// The timer till the timer resets, wat?
	private int FPS;							// Current fps counted.
	private int showFPS;					// The shown fps in label
	private float currentSpeed;				// Players current movement speed.
    private int playerScore;
    private int playerHP;
	private PlayerController myPlayer;		// Player script
    private MrCamera myCamera;                // Camera script
	// Use this for initialization
	void Start ()
	{
		myPlayer = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        myCamera = Camera.mainCamera.GetComponent<MrCamera>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(timer>updateInterval)
		{
			showFPS = (int)((float)FPS/updateInterval);
			currentSpeed = new Vector2(myPlayer.rigidbody.velocity.x, myPlayer.rigidbody.velocity.z).magnitude;
            playerScore = myPlayer.GetScore();
            playerHP = myPlayer.GetHP();
			FPS =0;
			timer=0.0f;
		}
		else
			timer+=Time.deltaTime;
		FPS++;

        if(Input.GetKey(KeyCode.K))
        {
            if(myCamera.CameraSmooth > 0.5f)
            {
                myCamera.CameraSmooth -= 0.1f;
            }
        }

        if (Input.GetKey(KeyCode.L))
        {
            if (myCamera.CameraSmooth < 500f)
            {
                myCamera.CameraSmooth += 0.1f;
            }
        }
	}
	
	void OnGUI()
	{
		GUI.Label(new Rect(5,5,100,30),"FPS: "+showFPS);
		GUI.Label(new Rect(5,20,200,30),"Powerup: "+myPlayer.GetPowerup);
		GUI.Label(new Rect(5,35,200,30),"Speed: "+currentSpeed);
        GUI.Label(new Rect(5, 50, 200, 30), "Score: " + playerScore);
        GUI.Label(new Rect(5, 65, 200, 30), "HP: " + playerHP);
        GUI.Label(new Rect(5, 80, 200, 30), "CameraSmooth: " + myCamera.CameraSmooth + "(-K +L)");
	}
}
