using UnityEngine;
using System.Collections;

public enum CameraMode { free, thirdperson, firstperson }

public class MrCamera : MonoBehaviour {

	private Transform CameraTarget; 				// The main character which the camera will and should follow at all times.
	public float heightOffset;							// Height offset from the characters pivot point.
	public float maxDistance, minDistance; 		// Max distance from target character, if in 3rd person mode.
	public float MouseSensitivity; 					// Public variable for desired mouse sensitivity, applied for character turning and rotating the camera on freemode and looking up and down on 3rd person,
	public CameraMode camMode; 					// The camera mode
    public float CameraSmooth = 5.0f;           // Smoothness of the camera.

    // Get Set
    public float CurentDistance { get { return currentDistance; } set { currentDistance = value; } }
    public Vector2 RotateAxis { get { return rotateAxis; } set { rotateAxis = value; } }
	
	//private variables
	private float tempDistance;						// If raycast hit a object behind the camera
	private Vector3 targetPos;						// The player position.
	private float currentDistance; 					// Cuirrent distance from target
	private float roamSpeed = 10.0f; 				// Hardcoded freecamera speed
	private Vector2 rotateAxis = Vector2.zero;	    // The desired rotation from mouse axis
    private Vector2 oldRotateAxis = Vector2.one;
	
	private Quaternion camRotation;				// The rotation of camera target point.
	
	private LayerMask playerLayer;					// The player layer
    
    // Player script uses this for desired direction.
    public Quaternion GetRotation { get { return camRotation; } }
	
	// Called once, when script is opened.
	void Awake()
	{
		
	}
	
	// Use this for initialization
	void Start ()
	{
        CameraTarget = (Transform)GameObject.FindWithTag("Player").transform;
        if (!CameraTarget&&Application.loadedLevel != 0)
        {
            Debug.LogError("Could not find player object! Make sure player is tagged as 'Player'");
            Application.Quit();
        }
		if(Screen.showCursor)Screen.showCursor=false; 	// Hide cursor in freeroam.
		if(!Screen.lockCursor)Screen.lockCursor=true; 	// Lock cursor in freeroam.
		camRotation=CameraTarget.rotation; 					// Default starting rotation
		currentDistance=(minDistance+maxDistance)/2.0f;
		playerLayer = ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("NPC"))); // Bit shifting the layer to only ignore player layer
	}
	
	// Update once a frame.
	#region Update
	void Update()
	{
		switch(camMode)
		{
			case CameraMode.free:
				FreeCamera();
				break;
			case CameraMode.thirdperson:
				ThirdPerson();
				break;
			default:
				Debug.Log("IMPOSSIBLE!!");
			break;
		}
		
		// Toggle cameramode
		if(Input.GetKeyDown(KeyCode.F6))
		{
			if((int)camMode!=2)
				camMode++;
			else
				camMode=CameraMode.free;
		}
	}
	#endregion Update
	
	// Free roaming camera mode
	#region FreeCamera
	void FreeCamera()
	{
		rotateAxis.x += Input.GetAxis("Mouse X") * MouseSensitivity;
		rotateAxis.y -= Input.GetAxis("Mouse Y") * MouseSensitivity;
		rotateAxis.x=Clamp360(rotateAxis.x);
		rotateAxis.y=Mathf.Clamp(rotateAxis.y,-80.0f,80.0f);

        if (rotateAxis != oldRotateAxis)
        {
            oldRotateAxis = rotateAxis;
            transform.rotation = Quaternion.Euler(oldRotateAxis.y, oldRotateAxis.x, transform.eulerAngles.z);
        }

        Vector3 desDirection = Vector3.zero; // Desired direction. Reset always at beginning.
        if (Input.GetKey(KeyCode.W)) desDirection += transform.TransformDirection(Vector3.forward); // move forward
        if (Input.GetKey(KeyCode.A)) desDirection += transform.TransformDirection(Vector3.left); // move left
        if (Input.GetKey(KeyCode.D)) desDirection += transform.TransformDirection(Vector3.right); // move right
        if (Input.GetKey(KeyCode.S)) desDirection += transform.TransformDirection(Vector3.back); // move back

		transform.position+=(desDirection.normalized * roamSpeed) * Time.deltaTime; // Normalize vector // Apply to camera transform position
	}
	
	#endregion

	// Thirdperson cameramode, raycast check for behind walls
	#region ThirdPerson cameramode
    void ThirdPerson()
    {
        // Rotation.
        rotateAxis.x += Input.GetAxisRaw("Mouse X") * MouseSensitivity;
        rotateAxis.y -= Input.GetAxisRaw("Mouse Y") * MouseSensitivity;
        rotateAxis.x = Clamp360(rotateAxis.x);
        rotateAxis.y = Mathf.Clamp(rotateAxis.y, -60.0f, 60.0f);

        targetPos = CameraTarget.position + (Vector3.up * heightOffset);

        RaycastHit theRay;
        if (Physics.Raycast(targetPos, camRotation * (-Vector3.forward), out theRay, currentDistance + 0.3f, playerLayer))
        {
            tempDistance = theRay.distance - 0.3f;
        }
        else tempDistance = currentDistance;

        // Calculate the camera rotation.
        if (rotateAxis != oldRotateAxis)
        {
            oldRotateAxis = rotateAxis;
            camRotation = Quaternion.Euler(rotateAxis.y, rotateAxis.x, camRotation.eulerAngles.z);
        }

        currentDistance -= Input.GetAxis("Mouse ScrollWheel");
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        transform.rotation = Quaternion.Slerp(transform.rotation, camRotation, Time.deltaTime * CameraSmooth);
        transform.position = Vector3.Slerp(transform.position, (targetPos + ((camRotation * Vector3.back) * tempDistance)), Time.deltaTime * CameraSmooth);
        
    }
	#endregion
	
	// For clamping rotation from Y axis (turning the character around or camera.)
	float Clamp360(float val)
	{
		if(val>360.0f)
			return val-360.0f;
		else if(val<0.0f)
			return val+360.0f;
		else
			return val;
	}
}
