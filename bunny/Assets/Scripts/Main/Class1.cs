using System;
using UnityEngine;

[Serializable]
public class RigidBodyMotorMovement
{
    // The maximum horizontal speed when moving
    public float MaxForwardSpeed = 10.0f;
    public float MaxSidewaysSpeed = 10.0f;
    public float MaxBackwardsSpeed = 10.0f;

    // How fast does the character change speeds?  Higher is faster.
    public float MaxGroundAcceleration = 2.0f;
    public float MaxAirAcceleration = 2.0f;

    // The gravity for the character
    public float Gravity = 10.0f;
    public float SlideRate = 30.0f;
    public float MaxFallSpeed = 50.0f;
}

[Serializable]
public class RigidBodyMotorJumping
{
    // Can the character jump?
    public bool Enabled = true;
    // How high do we jump when pressing jump and letting go immediately
    public float BaseHeight = 0.5f;
    // We add extraHeight units (meters) on top when holding the button down longer while jumping
    public float ExtraHeight = 2.0f;
    public float WallPush = 3.0f;

    [NonSerialized]
    public bool Jumping;
    [NonSerialized]
    public bool HoldingJumpButton;

    [NonSerialized]
    public float LastStartTime;
    [NonSerialized]
    public float LastButtonDownTime = -100f;
    [NonSerialized]
    public Vector3 JumpDir = Vector3.up;
}

[Serializable]
public class RigidBodyMotorMovingPlatform
{
    public bool Enabled = true;

    // TODO: Add
    //public MovementTransferOnJump MovementTransfer = MovementTransferOnJump.PermaTransferDecay;

    [NonSerialized]
    public Transform HitPlatform;
    [NonSerialized]
    public Transform ActivePlatform;
    [NonSerialized]
    public Vector3 ActiveLocalPoint;
    [NonSerialized]
    public Vector3 ActiveGlobalPoint;
    [NonSerialized]
    public Quaternion ActiveLocalRotation;
    [NonSerialized]
    public Quaternion ActiveGlobalRotation;
}

[Serializable]
[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class RigidBodyWalker : MonoBehaviour
{
    public bool CanControl = true;

    public RigidBodyMotorMovement Movement = new RigidBodyMotorMovement();
    public RigidBodyMotorJumping Jumping = new RigidBodyMotorJumping();
    public RigidBodyMotorMovingPlatform MovingPlatform = new RigidBodyMotorMovingPlatform();

    // The current global direction we want the character to move in.
    [NonSerialized]
    public Vector3 InputMoveDirection = Vector3.zero;
    [NonSerialized]
    public bool InputJump;

    [NonSerialized]
    private bool _contact;
    [NonSerialized]
    private float _lastContact;
    [NonSerialized]
    private bool _grounded;

    [NonSerialized]
    private Vector3 _contactNormal = Vector3.up;

    private Transform _transform;

    void Awake()
    {
        _transform = transform;

        rigidbody.freezeRotation = true;
        rigidbody.useGravity = false;

        Jumping.LastButtonDownTime = Time.time;
    }
    void FixedUpdate()
    {
        // Project specific as part of freeze/pause control.
        if (rigidbody.isKinematic)
            return;

        UpdateFunction();
    }
    void UpdateFunction()
    {
        // Update velocity based on input
        var velocity = ApplyInputVelocityChange();
        // Add gravity 
        velocity += ApplyGravity();
        // Apply jumping force
        ApplyJumping();

        rigidbody.AddForce(velocity, ForceMode.VelocityChange);

        if (MovingPlatform.Enabled && MovingPlatform.ActivePlatform != MovingPlatform.HitPlatform)
        {
            if (MovingPlatform.HitPlatform != null)
            {
                MovingPlatform.ActivePlatform = MovingPlatform.HitPlatform;
            }
        }

        _grounded = _contact;

        if (MovingPlatform.Enabled && _contact && MovingPlatform.HitPlatform != null)
        {
            // Use the center of the lower half sphere of the capsule as reference point.
            // This works best when the character is standing on moving tilting platforms. 
            MovingPlatform.ActiveGlobalPoint = _transform.position;
            MovingPlatform.ActiveLocalPoint = MovingPlatform.ActivePlatform.InverseTransformPoint(MovingPlatform.ActiveGlobalPoint);

            // Support moving platform rotation as well:
            MovingPlatform.ActiveGlobalRotation = _transform.rotation;
            MovingPlatform.ActiveLocalRotation = Quaternion.Inverse(MovingPlatform.ActivePlatform.rotation) * MovingPlatform.ActiveGlobalRotation;
        }
    }
    private Vector3 ApplyInputVelocityChange()
    {
        var velocity = rigidbody.velocity;
        velocity.y = 0.0f;

        if (!CanControl)
            InputMoveDirection = Vector3.zero;

        // Find desired velocity
        Vector3 desiredVelocity = GetDesiredHorizontalVelocity(InputMoveDirection);

        // Find the required velocity change to reach the desired velocity
        var velocityChange = (desiredVelocity - velocity);

        // Moving platform support
        if (MovingPlatform.Enabled && _grounded && _contactNormal.y >= 0.5f && MovingPlatform.ActivePlatform != null)
        {
            var newGlobalPoint = MovingPlatform.ActivePlatform.TransformPoint(MovingPlatform.ActiveLocalPoint);
            var moveDistance = (newGlobalPoint - MovingPlatform.ActiveGlobalPoint) / Time.deltaTime;

            velocityChange += moveDistance;

            // Support moving platform rotation as well:
            var newGlobalRotation = MovingPlatform.ActivePlatform.rotation * MovingPlatform.ActiveLocalRotation;
            var rotationDiff = newGlobalRotation * Quaternion.Inverse(MovingPlatform.ActiveGlobalRotation);

            var yRotation = rotationDiff.eulerAngles.y;
            if (yRotation != 0)
            {
                // Prevent rotation of the local up vector
                _transform.Rotate(0, yRotation, 0);
            }
        }

        var maxVelocityChange = GetMaxAcceleration() * Time.deltaTime;

        if (InputMoveDirection == Vector3.zero)
        {
            if (_contact)
                maxVelocityChange /= 2.0f;
            else
                maxVelocityChange /= 10.0f;
        }

        // Limit max rate of change
        if (velocityChange.magnitude > maxVelocityChange)
        {
            velocityChange = velocityChange.normalized * maxVelocityChange;
        }

        // If we are touching a wall that isn't vertical
        // Vertical check is there to prevent getting stuck in corners
        if (_contact && _contactNormal.y > 0.01)
            velocityChange = AdjustGroundVelocityToNormal(velocityChange, _contactNormal);

        return velocityChange;
    }
    private Vector3 ApplyGravity()
    {
        var velocity = rigidbody.velocity;
        velocity.x = 0;
        velocity.z = 0;

        var desiredVelocity = GetDesiredVerticalVelocity();

        var velocityChange = (desiredVelocity - velocity);

        var maxVelocityChange = (!_contact || _contactNormal.y <= 0.01f ? Movement.Gravity : Movement.SlideRate) * Time.deltaTime;

        // Limit max rate of change
        if (velocityChange.magnitude > maxVelocityChange)
        {
            velocityChange = velocityChange.normalized * maxVelocityChange;
        }

        if (_contact)
            velocityChange = AdjustGravityToNormal(velocityChange.y, _contactNormal);

        return velocityChange;
    }
    private void ApplyJumping()
    {
        var velocity = rigidbody.velocity;

        if (!InputJump || !CanControl)
        {
            Jumping.HoldingJumpButton = false;
            Jumping.LastButtonDownTime = -100;
        }

        if (InputJump && Jumping.LastButtonDownTime < 0 && CanControl)
            Jumping.LastButtonDownTime = Time.time;

        if (!_contact)
        {
            if (Jumping.Jumping && Jumping.HoldingJumpButton)
            {
                // Calculate the duration that the extra jump force should have effect.
                // If we're still less than that duration after the jumping time, apply the force.
                if (Time.time < Jumping.LastStartTime + Jumping.ExtraHeight / CalculateJumpVerticalSpeed())
                {
                    // Negate the gravity we just applied, except we push in jumpDir rather than jump upwards.
                    velocity += Jumping.JumpDir * Movement.Gravity * Time.deltaTime;
                }
            }
        }

        if (_contact || !_contact && (Time.time - _lastContact < 0.2f))
        {
            if (!Jumping.Jumping && Jumping.Enabled && CanControl && (Time.time - Jumping.LastButtonDownTime < 0.2f))
            {
                Jumping.Jumping = true;
                Jumping.LastStartTime = Time.time;
                Jumping.LastButtonDownTime = -100;
                Jumping.HoldingJumpButton = true;

                _lastContact = -100f;

                Jumping.JumpDir = (_contactNormal.y >= -0.01) ? Vector3.up : Vector3.down;

                // Apply the jumping force to the velocity. Cancel any vertical velocity first.
                velocity.y = 0;
                velocity += Jumping.JumpDir * CalculateJumpVerticalSpeed();

                var wallDir = new Vector3(_contactNormal.x, 0, _contactNormal.z);

                velocity += wallDir * Jumping.WallPush;

                _contact = false;
            }
        }

        rigidbody.velocity = velocity;
    }

    private float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * Jumping.BaseHeight * Movement.Gravity);
    }
    private Vector3 GetDesiredHorizontalVelocity(Vector3 direction)
    {
        // Find desired velocity
        var desiredLocalDirection = _transform.InverseTransformDirection(direction);
        var maxSpeed = MaxSpeedInDirection(desiredLocalDirection);

        return _transform.TransformDirection(desiredLocalDirection * maxSpeed);
    }
    private Vector3 GetDesiredVerticalVelocity()
    {
        return new Vector3(0, -Movement.MaxFallSpeed, 0);
    }
    private static Vector3 AdjustGroundVelocityToNormal(Vector3 hVelocity, Vector3 groundNormal)
    {
        var sideways = Vector3.Cross(Vector3.up, hVelocity);
        return Vector3.Cross(sideways, groundNormal).normalized * hVelocity.magnitude;
    }
    private static Vector3 AdjustGravityToNormal(float velocity, Vector3 groundNormal)
    {
        // Stop floating point errors
        groundNormal = Vector3.ClampMagnitude(groundNormal, 1);

        if (groundNormal.y < 0)
            return new Vector3(0, velocity, 0);

        // Angle of ground
        var angle = Mathf.Acos(groundNormal.y);

        // Magnitude down the slope
        var magnitude = velocity * Mathf.Sin(angle);

        // Magnitudes of componenets
        var vMagnitude = magnitude * Mathf.Sin(angle);
        var hMagnitude = -magnitude * Mathf.Cos(angle);

        var horizontal = new Vector2(groundNormal.x, groundNormal.z).normalized * hMagnitude;

        return new Vector3(horizontal.x, vMagnitude, horizontal.y);
    }
    private float MaxSpeedInDirection(Vector3 desiredMovementDirection)
    {
        if (desiredMovementDirection == Vector3.zero)
            return 0;

        var zAxisEllipseMultiplier = (desiredMovementDirection.z > 0 ? Movement.MaxForwardSpeed : Movement.MaxBackwardsSpeed) / Movement.MaxSidewaysSpeed;
        var temp = new Vector3(desiredMovementDirection.x, 0, desiredMovementDirection.z / zAxisEllipseMultiplier).normalized;
        var length = new Vector3(temp.x, 0, temp.z * zAxisEllipseMultiplier).magnitude * Movement.MaxSidewaysSpeed;

        return length;
    }
    private float GetMaxAcceleration()
    {
        // Maximum acceleration on ground and in air
        return _contact ? Movement.MaxGroundAcceleration : Movement.MaxAirAcceleration;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Jumping.Jumping = false;
        _lastContact = -100;
        Jumping.HoldingJumpButton = false;
    }
    private void OnCollisionExit(Collision collision)
    {
        _contact = false;
        _lastContact = Time.time;

        if (collision.contacts.Length > 0)
            MovingPlatform.HitPlatform = collision.contacts[0].otherCollider.transform;
    }
    private void OnCollisionStay(Collision collision)
    {
        _contact = true;

        var contacts = collision.contacts;

        Array.Sort(contacts, (c1, c2) => c1.normal.y.CompareTo(c2.normal.y));
        Array.Reverse(contacts);

        _contactNormal = contacts[0].normal;

        MovingPlatform.HitPlatform = contacts[0].otherCollider.transform;
    }
}
