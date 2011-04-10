using UnityEngine;
using System.Collections;

public enum MovementType  { FORCE, RAW }
public enum PlayerState { running, idle, falling }

public class PlayerController : Character
{
    public static PlayerController Instance
    {
        get
        {
            if (_instance)
            {
                return _instance;
            }
            else
            {
                return null;
            }
        }

        set
        {
            if (!_instance)
            {
                _instance = value;
            }
        }
    }
    private static PlayerController _instance;

	// PUBLIC VARIABLES
	public float maxSpeed;				            // Movement max speed
	public float turnSpeed;				            // The speed, the mesh rotates towards the desired movement direction
	public float jumpForce;				            // Jump force
	public float accelerationSpeed; 	            // Movement accelerationspeed.

    // Damage related variables
    public ForceMode KnockbackForceMode = ForceMode.Impulse;
    public int PlayerDamage = 1;
    public float PlayerKnockbackForce = 3.0f;
    private int playerDamageBuff = 0;
    private float playerKnockbackForceBuff = 0.0f;
    public float damageAddCooldown = 0.0f; // Half of the animation length.
	
	// Set values
	public PowerupType SetPowerup { set { curPower=value; } }
	public PowerupType GetPowerup { get { return curPower; } }
	
	// PRIVATE VARIABLES

    // Player hp
    public int GetHP()
    {
        return base.CharacterHealth;
    }
    public void SetHP(int hp)
    {
        base.CharacterHealth = hp;
    }

    // Player score
    public int GetScore()
    {
        return playerScore;
    }
    public void SetScore(int score)
    {
        playerScore = score;
    }
    public void AddScore(int score)
    {
        playerScore += score;
    }
    private int playerScore;

	// Animation variables
	private float animationTimer = 0.0f;				// Timer for once animations, till prepearing for crossfade for loops.. check the status of those after this.,
	private PlayerState myState=PlayerState.idle;	    // Starting state for player
	private string playAnim = "run";					// Current animation
	
	// Movement variables
    private MrCamera myCamera;
	public MovementType mType = MovementType.RAW;
	private bool moveDenied = false;					// If unbale to move towards the desired direction
	private Quaternion camRot;							// desired rotation for the mesh
	private Quaternion charRot;							// Rotation of the character.
	private Vector3 desDirection = Vector3.zero;
	private Vector3 xzVel = Vector3.zero;			    // The current velocity towards desired direction divided by maxspeed.
	
	// Groundcheck variables
	private bool grounded;								// Is the character grounded, controls jump conditions.	
	private float rayInterval;							// Interval when checking grounded, like 50ms should suffice.
	private float myTime;								// My own time
	
	// Physics variables
	private LayerMask playerLayer;						// Player layer, inverted to ignore itself and work on other layers.
	
	// Powerup variables
	private float powerDuration = 0.0f;					// The duration powrup lasts..
	private PowerupType curPower = PowerupType.NONE;	// Current Powerup type
	private float jumpForceBonus = 0.0f;				// Bonus jumpforce.
	private bool doubleJumpBonus = false;				// Double jump bonus.
	private bool doubleJumpBonusUsed = false;			// if it is used
	
	// Debugging variables
	public string GetLastPowerup { get { return lastPowerup; } }
	private string lastPowerup = "None.";
	
	// Use this for initialization
    void Awake ()
    {
        // Game started, we should tell the runtime im in the scene...
        PlayerController.Instance = this;

        if (GameData.Instance.CurrentProfile == null)
        {
            Debug.Log("Editor, I guess.");
        }
        else if (GameData.Instance.CurrentProfile.FirstTime)
        {
            // PRofile is first time being loaded. SWEET!!
            GameData.Instance.CurrentProfile.FirstTime = false;
        }
        else
        {
            GameData.Instance.LoadWorld(this);
        }
    }

	public override void Start ()
	{
        myCamera = (MrCamera)GameObject.FindObjectOfType(typeof(MrCamera)) as MrCamera;

		// Animation configuration wrapmodes
		animation["idle"].wrapMode 				= WrapMode.Loop;
		animation["run"].wrapMode 				= WrapMode.Loop;
		animation["midair"].wrapMode 			= WrapMode.Loop;
		animation["jump"].wrapMode 				= WrapMode.Once;
		animation["landing"].wrapMode 			= WrapMode.Once;
		animation["pawhit1"].wrapMode 			= WrapMode.Once;
		animation["aerialattack1"].wrapMode 	= WrapMode.Once;
		
		playerLayer = ~(1 << LayerMask.NameToLayer("Player"));
		rayInterval = 0.020f;

        base.Start();
	}
	
	// Update is called once per frame
	public override void Update ()
	{
        if(Input.GetKeyDown(KeyCode.F5))
        {
            GameData.Instance.UpdateCurrentProfile();
        }

		 // Grounded check
		if(myTime>rayInterval)
		{
			// Do ray casts
			GroundCheck();
		}
		else myTime+=Time.deltaTime;

        // PLayer movement.
        desDirection = Vector3.zero; // Desired direction. Reset always at beginning.
        if (Input.GetKey(KeyCode.W)) desDirection += Vector3.forward; // move forward
        if (Input.GetKey(KeyCode.A)) desDirection += Vector3.left; // move left
        if (Input.GetKey(KeyCode.D)) desDirection += Vector3.right; // move right
        if (Input.GetKey(KeyCode.S)) desDirection += Vector3.back; // move back

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();				// Jump logic
        }

        // Attack
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Attack();			// Attack logic
        }


        // Player managment functions
        PlayAnimation();				// Control the animations
        // Debug.Log(jumpForceBonus);
        if (powerDuration < Time.time && curPower != PowerupType.NONE)
        {
            ResetPowerup();
        }
		
        base.Update();
	}

    void LateUpdate()
    {
        
    }
	
	public override void FixedUpdate()
	{
        if (myCamera) Move();

        base.FixedUpdate();
	}

	// Ground check here and other collision related actions.
	#region collisions
	void OnCollisionEnter(Collision collision)
	{
		float velY = collision.relativeVelocity.y;
		GroundCheck();
 		if(grounded&&velY>4.0f)
			playAnim="landing";
	}
	#endregion
	
	// Debugging information ingame overlay.
	#region DEBUG
	public override void OnDrawGizmos()
	{
		Gizmos.DrawRay(transform.position, desDirection * 2.0f);
        base.OnDrawGizmos();
	}
	#endregion

    #region Player Movement
    void Move()
    {
        // Move towards
        if (desDirection != Vector3.zero)
        {

            // Multiply cameras relative rotation with the desired movement direction and you get the correct direction for movement
            desDirection = myCamera.GetRotation * desDirection;

            // Take away the y factor. So we wont move up or down
            desDirection = new Vector3(desDirection.x, 0.0f, desDirection.z).normalized;

            // Set the character rotation on the right axis
            charRot = Quaternion.LookRotation(desDirection, Vector3.up);

            // Rotate towards the desired direction relative to the camera
            transform.rotation = Quaternion.RotateTowards(transform.rotation, charRot, turnSpeed * Time.deltaTime);

            // Get the velocity for a stored vector3
            xzVel = new Vector3(rigidbody.velocity.x, 0.0f, rigidbody.velocity.z);
            // Get the magnitude of the velocity aka speed
            float magn = xzVel.magnitude;
            // Divide velocity with maxspeed, to get value between 0.0...1.0f
            xzVel /= maxSpeed;

            if (!moveDenied)
            {
                switch (mType)
                {
                    case MovementType.FORCE:
                        #region Physics based movement. ( Buggy as hell. )
                        Vector3 rigVel = xzVel.normalized;

                        Vector3 directionCorrectionForce = Vector3.Exclude(rigVel, desDirection).normalized;
                        directionCorrectionForce = new Vector3(directionCorrectionForce.x, 0.0f, directionCorrectionForce.z);

                        if (magn < maxSpeed)
                        {
                            if (grounded)
                                rigidbody.AddForce(desDirection * (accelerationSpeed * Time.deltaTime));
                            else
                                rigidbody.AddForce(desDirection * (accelerationSpeed * Time.deltaTime));
                        }
                        else if (Vector3.Angle(rigVel, desDirection) > 15.0f)
                        {
                            rigidbody.AddForce(directionCorrectionForce * (accelerationSpeed * Time.deltaTime));
                        }
                        #endregion
                        break;
                    case MovementType.RAW:
                        #region Raw based movement
                        // Raw movement

                        rigidbody.velocity = new Vector3(desDirection.x * maxSpeed, rigidbody.velocity.y, desDirection.z * maxSpeed);
                        // rigidbody.velocity = transform.TransformDirection(desDirection * maxSpeed);
                        #endregion
                        break;
                }
                if (grounded)
                {
                    animation["run"].speed = magn / maxSpeed;
                    myState = PlayerState.running;
                }
            }
            else if (grounded) myState = PlayerState.idle;
        }
        else if (grounded) myState = PlayerState.idle;
    }
    #endregion

    // My custom functions
	#region CustomFucntions
	// Animation state managment
	void PlayAnimation()
	{
		switch(animation[playAnim].wrapMode)
		{
			case WrapMode.Loop:
				if(!animation.IsPlaying(playAnim))
				{
//					Debug.Log(playAnim+" loop");
					animation.CrossFade(playAnim,0.2f);
				}
				switch(myState)
				{
					case PlayerState.idle:
						playAnim="idle";
						break;
					case PlayerState.running:
						playAnim="run";
						break;
					case PlayerState.falling:
						playAnim="midair";
						break;
				}
				break;
			case WrapMode.Once:
				if(!animation.IsPlaying(playAnim))
				{
					animationTimer=0.0f;
//					Debug.Log(playAnim+ " once");
					animation.CrossFade(playAnim,0.2f);
				}
				else animationTimer+=Time.deltaTime;
				if(animationTimer>(animation[playAnim].length-0.2f))
				{
					switch(myState)
					{
						case PlayerState.idle:
							playAnim="idle";
							break;
						case PlayerState.running:
							playAnim="run";
							break;
						case PlayerState.falling:
							playAnim="midair";
							break;
					}
				}
				break;
		}
	}
	
	// Checking if we are falling or grounded. OnCollisionEnter and OnCollisionStay
	void GroundCheck()
	{
		myTime = 0.000f;
		RaycastHit myRay;
		if(Physics.Raycast(transform.position, Vector3.down, out myRay, 0.25f, playerLayer))
		{
			grounded=true;
			doubleJumpBonusUsed = false;
		}
		else
		{
			myState=PlayerState.falling;
			grounded=false;
		}
		
		if(Physics.Raycast(transform.position + (Vector3.up * 0.2f), desDirection, out myRay, 0.25f, playerLayer)||Physics.Raycast(transform.position + (Vector3.up * 0.6f), desDirection, out myRay, 0.25f, playerLayer))
		{
			// Something is front of me
			moveDenied=true;
		}
		else
		{
			moveDenied=false;
		}
	}
	
	// 
	public void SetPowerUp(PowerupType pwr)
	{
		//  NONE, DOUBLE_JUMP, SUPER_JUMP, ONE_HIT_WONDER, INVINCIBILITY, INVISIBILITY, SUPER_BUNNY
		ResetPowerup();
		curPower=pwr;
		switch(curPower)
		{
			case PowerupType.NONE:
			break;
			case PowerupType.DOUBLE_JUMP:
				powerDuration = Time.time + 15.0f; // Seconds.
				doubleJumpBonus = true;
				doubleJumpBonusUsed = false;
			break;
			case PowerupType.SUPER_JUMP:
				powerDuration = Time.time + 15.0f;
				jumpForceBonus = jumpForce / 2.0f;
			break;
			case PowerupType.ONE_HIT_WONDER:
			break;
			case PowerupType.INVINCIBILITY:
			break;
			case PowerupType.INVISIBILITY:
			break;
			case PowerupType.SUPER_BUNNY:
			break;
		}
	}
	
	void ResetPowerup()
	{
		curPower=PowerupType.NONE;
		doubleJumpBonus = false;
		jumpForceBonus = 0.0f;
	}
	
	void Jump()
	{
        GroundCheck();
		if(grounded) // Normal jump when grounded
		{
			switch(mType)
			{
				case MovementType.FORCE:
					rigidbody.AddForce(Vector3.up * (jumpForce + jumpForceBonus), ForceMode.Impulse);
				break;
				case MovementType.RAW:
					rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpForce + jumpForceBonus, rigidbody.velocity.z);
				break;
			}
			playAnim = "jump";
			grounded=false;
			myTime=0.000f;
		}
		else if(doubleJumpBonus&&!doubleJumpBonusUsed)
		{
			doubleJumpBonusUsed=true;
			switch(mType)
			{
				case MovementType.FORCE:
					rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
				break;
				case MovementType.RAW:
					rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpForce + jumpForceBonus, rigidbody.velocity.z);
				break;
			}
			playAnim = "jump";
			grounded=false;
			myTime=0.000f;
		}
	}
	
	void Attack()
	{
		// Normal attack
		if(grounded)
		{
			playAnim = "pawhit1";
            if (animation["pawhit1"].normalizedTime > 0.5f)
            {

                if (Time.time > damageAddCooldown)
                {
                    GameObject target = MeleeDamage(this.gameObject, PlayerDamage + playerDamageBuff, PlayerKnockbackForce + playerKnockbackForceBuff, KnockbackForceMode);
                    damageAddCooldown = Time.time + animation["pawhit1"].length;
                }
            }
		}
		else // Possibly midair?
		{
			playAnim = "aerialattack1";
            if (animation["aerialattack1"].normalizedTime > 0.5f)
            {

                if (Time.time > damageAddCooldown)
                {
                    GameObject target = MeleeDamage(this.gameObject, PlayerDamage + playerDamageBuff, PlayerKnockbackForce + playerKnockbackForceBuff, KnockbackForceMode);
                    damageAddCooldown = Time.time + animation["aerialattack1"].length;
                }
            }
		}
	}
	#endregion

    public override void DamageTaken(GameObject source)
    {
        
    }

    public override void Death()
    {
        GameData.Instance.GameState = GAMESTATE.GAMEOVER;
    }
}

