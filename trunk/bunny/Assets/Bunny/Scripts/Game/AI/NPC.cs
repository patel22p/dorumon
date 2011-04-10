using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// IDLE, ROAM, COMBAT, ROUTINE
/// </summary>
public enum NPCSTATE { IDLE, ROAM, COMBAT, ROUTINE, FOLLOW, JUMP, DEATH }
public enum NPCANIMATIONS { IDLE, RUN, WALK, MIDAIR, DEATH }

public class NPC : Character
{
    public NPCSTATE MyState { set { myState = value; } get { return myState; } }
    private NPCSTATE myState = NPCSTATE.IDLE;
    private NPCANIMATIONS myAnim = NPCANIMATIONS.IDLE;
    
    private PathFinder myFinder;

    private bool initialized = false;

    public WaypointNodeParent WorldWaypoints;

    public GameObject FriendlyTarget { set { friendlyTarget = value; } get { return friendlyTarget;} }
    private GameObject friendlyTarget;
    public GameObject AggroTarget { set { aggroTarget = value; } get { return aggroTarget; } }
    private GameObject aggroTarget;
    private Vector3 gotoTarget = Vector3.zero;

    // General attributes of NPC
    public float NPCRUNSPEED;
    public float NPCWALKSPEED;
    public float NPCTURNSPEED;
    public float NPCACCELERATIONSPEED = 30.0f;
    public float NPCFOLLOWWALKDIST = 4.0f;
    public float NPCSTARTFOLLOWDIST = 4.0f;
    public float NPCJUMPFORCE = 10.0f;
    public float NPCHEIGHTOFFSET = 0.5f;

    // Combat and chaes stuff
    public float NPCFORGETDISTANCE = 4.0f;
    private Vector3 chaseStartPosition = Vector3.zero;
    private NPCSTATE oldState;

    public float NPCAGGRODISTANCE = 3.0f;

    // Death stuff
    public float NPCDEATHDOWNSPEED = 1.0f;
    public float NPCTIMEBEFOREDOWNED = 2.0f;

    // Private movement attributes
    private bool grounded = true;
    private float speedMagnitude = 0.0f;            // Used if animating by speed. Default in movement.

    private float attackCooldown = 0.0f;

    // Animations
    public float AnimationCrossFade = 0.2f;
    private float animationTimer = 0.0f;

    // PF
    private ArrayList pf_path = new ArrayList();
    private bool usingPath = false;
    private int pointInPath = 0;

    public float FollowSightUpdateInterval = 1.5f;

    private bool applyMovementForce = false;
    private Vector3 movementDirection = Vector3.zero;
    private int steeringStep = 0; // 0 fw-left, 1 fw (jump check), 2 fw-right, 3 edge check?
    private float steeringTimerCheck;
    private Vector3 steerDir = Vector3.zero;
    private bool steerLeftBlocked = false;
    private bool steerRightBlocked = false;
    private bool steerFwdBlocked = false;

    private float internalTimer = 0.0f;
    private LayerMask ignoreSightLayer;
    private bool seeTarget = false;
    private bool seeOriginTarget = false;

    /// <summary>
    /// Initialization code.
    /// </summary>
    /// <returns></returns>
    public bool Init()
    {
        if(WorldWaypoints)
            myFinder = new PathFinder(WorldWaypoints.GetWaypoints);
        // Initialization code

        animation.wrapMode = WrapMode.Loop;
        animation["death"].wrapMode = WrapMode.ClampForever;

        // Ignore layers.
        ignoreSightLayer = ~((1 << LayerMask.NameToLayer("NPC")) | (1 << LayerMask.NameToLayer("Player")));

        if (base.myBehaviour == NPCBEHAVIOUR.HOSTILE) aggroTarget = PlayerController.Instance.gameObject; // Since npc is hostile, default aggrotarget should be player.
        return true;
    }

    /// <summary>
    ///  Update every frame
    /// </summary>
    public override void Update()
    {
        if (!initialized) initialized = Init();

        if (Input.GetKeyDown(KeyCode.H))
        {
            AnimateOnce("gethitA");
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            friendlyTarget = GameObject.FindWithTag("Player");
            myState = NPCSTATE.FOLLOW;
            myBehaviour = NPCBEHAVIOUR.FRIENDLY;
        }

        if (Input.GetKeyDown(KeyCode.N))
            Jump();

        if (myState != NPCSTATE.DEATH)
        {
            grounded = GroundCheck();

            switch (myState)
            {
                case NPCSTATE.IDLE:
                    AggroCheck();
                    if (base.myBehaviour == NPCBEHAVIOUR.FRIENDLY && friendlyTarget != null)
                    {
                        if (Vector3.Distance(transform.position, friendlyTarget.transform.position) > NPCSTARTFOLLOWDIST)
                        {
                            myState = NPCSTATE.FOLLOW;
                        }
                    }

                    if (usingPath)
                        ResetPath();
                    break;
                case NPCSTATE.ROAM:
                    AggroCheck();
                    break;
                case NPCSTATE.COMBAT:
                    if (base.myBehaviour == NPCBEHAVIOUR.FRIENDLY)
                    {
                        // Help player in combat.
                    }
                    if (base.myBehaviour == NPCBEHAVIOUR.HOSTILE)
                    {
                        // FUCK DEM UP or IT or THAT or WHAT?
                        if (aggroTarget != null)
                        {
                            if (SightCheck(aggroTarget.transform.position))
                            {
                                if (!Move(aggroTarget.transform.position, HitDistance))
                                {
                                    if(RotateTowards(aggroTarget.transform.position))
                                    {
                                        AnimateOnce("punch");
                                        if (animation["punch"].time > animation["punch"].length * 0.2f)
                                        {
                                            if (attackCooldown <= 0)
                                            {
                                                attackCooldown = animation["punch"].length * 0.8f;
                                                MeleeDamage(this.gameObject, 1, 5.0f, ForceMode.Impulse);
                                            }
                                            else
                                                attackCooldown -= Time.deltaTime;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case NPCSTATE.ROUTINE:
                    break;
                case NPCSTATE.FOLLOW:
                    AggroCheck();
                    if (gotoTarget != Vector3.zero)
                    {
                        if (SightCheck(gotoTarget)) // Do we see target
                        {
                            Debug.Log("SEE IT!");
                            if (!Move(gotoTarget, 1.0f))
                            {
                                myState = NPCSTATE.IDLE; // CLose enough.
                                gotoTarget = Vector3.zero;
                            }

                            if (usingPath)
                                ResetPath();
                        }
                        else if (FollowingPath(gotoTarget))// Get path and follow it.
                        {
                            myState = NPCSTATE.IDLE;
                        }
                    }
                    else if (base.myBehaviour == NPCBEHAVIOUR.FRIENDLY)
                    {
                        // Follow player around.
                        if (friendlyTarget != null)
                        {
                            if (SightCheck(friendlyTarget.transform.position)) // Do we see target
                            {
                                if (!Move(friendlyTarget.transform.position, 1.0f))
                                    myState = NPCSTATE.IDLE; // CLose enough.

                                if (usingPath)
                                    ResetPath();
                            }
                            else if (FollowingPath(friendlyTarget.transform.position))// Get path and follow it.
                            {
                                myState = NPCSTATE.IDLE;
                            }
                        }
                    }
                    break;
            }
        }

        UpdateAnimations(); // State dependant animation update

        base.Update();
    }

    public override void FixedUpdate()
    {
        if (applyMovementForce&&rigidbody)
        {
            ApplyMovementForce();
        }
        base.FixedUpdate();
    }

    /// <summary>
    /// Move towards a point, return true when moving and false when done.
    /// </summary>
    /// <param name="target">Desired point in world</param>
    /// <param name="Distance">Stop distance</param>
    public bool Move(Vector3 pos, float Distance)
    {
        if (Vector3.Distance(transform.position, pos) > Distance)
        {
            movementDirection = pos - transform.position;
            movementDirection.y = 0.0f;
            movementDirection = movementDirection.normalized;
            applyMovementForce = RotateTowards(pos);
            return true;
        }
        else
        {
            applyMovementForce = false;
            return false;
        }
    }

    public void GoTo(Vector3 target)
    {
        gotoTarget = target;
        myState = NPCSTATE.FOLLOW;
    }

    /// <summary>
    /// Check wether there is anything inbetween the path to the position (TARGET)
    /// </summary>
    /// <param name="_pos">The position</param>
    /// <returns></returns>
    public bool SightCheck(Vector3 _pos)
    {
        if (Physics.Linecast(transform.position + (Vector3.up * NPCHEIGHTOFFSET), _pos +(Vector3.up * NPCHEIGHTOFFSET), ignoreSightLayer))
        {
            seeTarget = false;
        }
        else
        {
            seeTarget = true;
        }
        return seeTarget;
    }

    /// <summary>
    /// You determine the points.
    /// Fe. Path end point to player point
    /// </summary>
    /// <param name="_pos">End</param>
    /// <param name="origin">Start</param>
    /// <returns></returns>
    public bool SightCheck(Vector3 _pos, Vector3 _origin)
    {
        if (Physics.Linecast(_origin + (Vector3.up * NPCHEIGHTOFFSET), _pos + (Vector3.up * NPCHEIGHTOFFSET), ignoreSightLayer))
        {
            seeOriginTarget = false;
        }
        else
        {
            seeOriginTarget = true;
        }
        return seeOriginTarget;
    }


    /// <summary>
    ///  makes the npc move.
    /// </summary>
    public void ApplyMovementForce()
    {
        float maxSpeed = 0.0f;
        float magnitude = rigidbody.velocity.magnitude;
        switch (myState)
        {
            case NPCSTATE.COMBAT:
                maxSpeed = NPCRUNSPEED;
                myAnim = NPCANIMATIONS.RUN;
                speedMagnitude = (magnitude / NPCRUNSPEED) * 0.8f;
                // Always run.
                break;
            case NPCSTATE.FOLLOW:
                if (!friendlyTarget)
                {
                    maxSpeed = NPCWALKSPEED;
                    speedMagnitude = (magnitude / NPCWALKSPEED) * 2.2f;
                    myAnim = NPCANIMATIONS.WALK;
                }
                else if (Vector3.Distance(friendlyTarget.transform.position, transform.position) < NPCFOLLOWWALKDIST)
                {
                    maxSpeed = NPCWALKSPEED;
                    speedMagnitude = (magnitude / NPCWALKSPEED) * 2.2f;
                    myAnim = NPCANIMATIONS.WALK;
                }
                else
                {
                    maxSpeed = NPCRUNSPEED;
                    myAnim = NPCANIMATIONS.RUN;
                    speedMagnitude = (magnitude / NPCRUNSPEED) * 0.8f;
                }
                break;
            case NPCSTATE.ROAM:
                maxSpeed = NPCWALKSPEED;
                speedMagnitude = (magnitude / NPCWALKSPEED) * 2.2f;
                myAnim = NPCANIMATIONS.WALK;
                break;
        }

        if (magnitude < maxSpeed)
        {
            Vector3 mDir = movementDirection;
            mDir.y = 0.0f;
            mDir = mDir.normalized;
            rigidbody.AddForce(mDir * NPCACCELERATIONSPEED);
        }
    }

    /// <summary>
    /// Rotate towards (Includes staring)
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    public bool RotateTowards(Vector3 targetPosition)
    {
        Vector3 _dir = targetPosition - transform.position;
        _dir.y = 0.0f;
        _dir = _dir.normalized;
        Quaternion desiredRotation = Quaternion.LookRotation(_dir, Vector3.up);
        float _angle = Vector3.Angle(transform.forward, _dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, NPCTURNSPEED * Time.deltaTime);
        if (_angle > 5.0f && !steerRightBlocked && !steerRightBlocked)
        {
            // Deny movement towards target.
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Simple timer
    /// </summary>
    /// <param name="interval"></param>
    /// <returns></returns>
    private bool InternalTimer(float interval)
    {
        if (interval < internalTimer)
        {
            internalTimer = 0.0f;
            return true;
        }
        else
        {
            internalTimer += Time.deltaTime;
            return false;
        }
    }

    public bool GroundCheck()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 0.2f, ignoreSightLayer))
        {
            if (!grounded) AnimateOnce("jump");
            if (!applyMovementForce) myAnim = NPCANIMATIONS.IDLE;
            return true;
        }
        else
        {
            myAnim = NPCANIMATIONS.MIDAIR;
            return false;
        }
    }

    public void Jump()
    {
        if (grounded)
        {
            AnimateOnce("jump");
            transform.rigidbody.AddForce(Vector3.up * NPCJUMPFORCE, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Timer based animation
    /// </summary>
    /// <param name="animationName"></param>
    public void AnimateOnce(string animationName)
    {
        animationTimer = animation[animationName].length;
        animation.CrossFade(animationName, AnimationCrossFade);
    }

    /// <summary>
    /// Follwing path, returns true when reached end.. should be aborted from sightcheck and RESETED TOO!
    /// </summary>
    /// <returns></returns>
    public bool FollowingPath(Vector3 pos)
    {
        pointInPath = pf_path.Count - 1;
        if ((pf_path.Count) > 0)
        {
            if (!Move(((ListedNode)pf_path[pointInPath]).Position, 1.0f))
            {
                if (pointInPath > 0)
                {
                    pf_path.RemoveAt(pointInPath);
                }
                else
                {
                    return true;
                }
            }
        }
        else
        {
            // Create path.
            if (!usingPath)
            {
                myFinder.Start(transform.position, pos);
                usingPath = true;
            }
            else if (!myFinder.Update())
            {
                // Done searching path.
                pf_path = myFinder.GetPath();
            }
        }

        // IF DONE RETURN TRUE, ELSE RETURN FALSE. IF SEE TARGEt, RETURN TRUE AND FORGET THIS SHIT.. WAT? GETPATH?? HMm.. MABY, INSANITY? YES!
        return false;
    }

    /// <summary>
    /// Reset the following variables for a new follow search.
    /// </summary>
    public void ResetPath()
    {
        usingPath = false;
        pf_path.Clear();
        pointInPath = 0;
    }

    // Debugging
    public override void OnDrawGizmos()
    {
        /*
        //Gizmos.DrawLine(transform.position + (Vector3.up * NPCHEIGHTOFFSET), transform.position + movementDirection + (Vector3.up * NPCHEIGHTOFFSET));
        if (usingPath)
        {
            foreach (ListedNode ln in pf_path)
            {
                Gizmos.DrawCube(ln.Position, Vector3.one * 0.3f);
            }
        }
         * */
        base.OnDrawGizmos();
    }

    public override void DamageTaken(GameObject source)
    {
        AnimateOnce("gethitA");
    }

    public override void Death()
    {
        myAnim = NPCANIMATIONS.DEATH;
        myState = NPCSTATE.DEATH;
        if (collider) DestroyImmediate(collider);
        if (rigidbody) DestroyImmediate(rigidbody);

        if (animation["death"].time > animation["death"].length + NPCTIMEBEFOREDOWNED)
        {
            // Start falling through ground. 2 units?
            transform.position += (Vector3.down * Time.deltaTime) * NPCDEATHDOWNSPEED;
            Destroy(gameObject, 3.0f);
        }
    }

    /// <summary>
    /// Play default animations depending on NPC state, if not playing.. choose a default one.
    /// </summary>
    public virtual void UpdateAnimations()
    {
       // STATES
        if (animationTimer < AnimationCrossFade)
        {
            switch (myAnim)
            {
                case NPCANIMATIONS.IDLE:
                    animation.CrossFade("idle", AnimationCrossFade);
                    break;
                case NPCANIMATIONS.MIDAIR:
                    animation.CrossFade("midair", AnimationCrossFade);
                    break;
                case NPCANIMATIONS.RUN:
                    animation["run"].speed = speedMagnitude;
                    animation.CrossFade("run", AnimationCrossFade);
                    break;
                case NPCANIMATIONS.WALK:
                    animation["walk"].speed = speedMagnitude;
                    animation.CrossFade("walk", AnimationCrossFade);
                    break;
                case NPCANIMATIONS.DEATH:
                    animation.CrossFade("death", AnimationCrossFade);
                    break;
            }
        }
        else
        {
            animationTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Should be used as a check. YES"!?
    /// 
    /// </summary>
    /// <returns>returns true if enemies are found.</returns>
    public void AggroCheck()
    {
        switch (base.myBehaviour)
        {
            case NPCBEHAVIOUR.FRIENDLY:
                break;
            case NPCBEHAVIOUR.HOSTILE:
                if (aggroTarget)
                {
                    // AGGROCHECK
                    if (Vector3.Distance(transform.position, aggroTarget.transform.position) < NPCAGGRODISTANCE)
                    {
                        if(InternalTimer(FollowSightUpdateInterval))
                        {
                            if (SightCheck(aggroTarget.transform.position))
                            {
                                myState = NPCSTATE.COMBAT;
                            }
                        }
                    }
                }
                break;
        }
    }
}
