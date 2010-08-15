using UnityEngine;
using System.Collections;
public abstract class Car : IPlayer
{
    public float maxCornerAccel = 10.0f;
    public float maxBrakeAccel = 10.0f;
    public float cogY = 0.0f;
    public Skidmarks skidmarks;
    public int minRPM = 700;
    public int maxRPM = 6000;
    public int maxTorque = 400;
    public int shiftDownRPM = 2500;
    public int shiftUpRPM = 5500;
    public float[] gearRatios = { -2.66f, 2.66f, 1.78f, 1.30f, 1.00f };
    public float finalDriveRatio = 3.4f;
    public float handlingTendency = 0.7f;
    public Transform wheelFR;
    public Transform wheelFL;
    public Transform wheelBR;
    public Transform wheelBL;
    public float suspensionDistance = 0.3f;
    public int springs = 1000;
    public int dampers = 200;
    public float wheelRadius = 0.45f;
    public Transform groundDustEffect;
    private float engineRPM;
    private float steerVelo = 0.0f;
    protected float brake = 0.0f;
    protected float handbrake = 0.0f;
    protected float steer = 0.0f;
    protected float motor = 0.0f;
    private bool onGround = false;
    private float cornerSlip = 0.0f;
    private float driveSlip = 0.0f;
    private float wheelRPM;
    private int gear = 1;
    private WheelData[] wheels;
    private float wheelY = 0.0f;
    private float rev = 0.0f;
    void GetStatus(GUIText gui)
    {
        gui.text = "v=" + (rigidbody.velocity.magnitude * 3.6).ToString("f1") + " km/h\ngear= " + gear + "\nrpm= " + engineRPM.ToString("f0");
    }
    void GetControlString(GUIText gui)
    {
        gui.text = "Use arrow keys to control the jeep,\nspace for handbrake.";
    }
    class WheelData
    {
        public float rotation = 0.0f;
        public WheelCollider coll;
        public Transform graphic;
        public float maxSteerAngle = 0.0f;
        public int lastSkidMark = -1;
        public bool powered = false;
        public bool handbraked = false;
        public Quaternion originalRotation;
    };
    
    public void StartCar()
    {
        
        wheels = new WheelData[4];
        for (int i = 0; i < 4; i++)
            wheels[i] = new WheelData();
        wheels[0].graphic = wheelFL;
        wheels[1].graphic = wheelFR;
        wheels[2].graphic = wheelBL;
        wheels[3].graphic = wheelBR;
        wheels[0].maxSteerAngle = 30.0f;
        wheels[1].maxSteerAngle = 30.0f;
        wheels[2].powered = true;
        wheels[3].powered = true;
        wheels[2].handbraked = true;
        wheels[3].handbraked = true;
        foreach (WheelData w in wheels)
        {
            if (w.graphic == null)
                Debug.Log("You need to assign all four wheels for the car script!");
            if (!w.graphic.transform.IsChildOf(transform))
                Debug.Log("Wheels need to be children of the Object with the car script");
            w.originalRotation = w.graphic.localRotation;
            GameObject colliderObject = new GameObject("WheelCollider");
            colliderObject.transform.parent = transform;
            colliderObject.transform.position = w.graphic.position;
            w.coll = (WheelCollider)colliderObject.AddComponent(typeof(WheelCollider));
            w.coll.suspensionDistance = suspensionDistance;
            JointSpring t = new JointSpring();
            t.spring = springs;
            t.damper = dampers;
            w.coll.suspensionSpring = t;
            WheelFrictionCurve wf = new WheelFrictionCurve();
            wf.stiffness = wf.extremumValue = wf.extremumSlip = wf.asymptoteSlip = wf.asymptoteValue = 6;
            w.coll.forwardFriction = wf;
            w.coll.sidewaysFriction = wf;
            w.coll.radius = wheelRadius;
        }
        wheelY = wheels[0].graphic.localPosition.y;
        Skidmarks skidmarks = (Skidmarks)FindObjectOfType(typeof(Skidmarks));
        gear = 1;
    }
    void UpdateWheels()
    {
        float handbrakeSlip = handbrake * rigidbody.velocity.magnitude * 0.1f;
        if (handbrakeSlip > 1)
            handbrakeSlip = 1;
        float totalSlip = 0.0f;
        onGround = false;

        foreach (WheelData w in wheels)
        {
            w.rotation += wheelRPM / 60.0f * -rev * 360.0f * Time.fixedDeltaTime;
            w.rotation = Mathf.Repeat(w.rotation, 360.0f);
            w.graphic.localRotation = Quaternion.Euler(w.rotation, w.maxSteerAngle * steer, 0.0f) * w.originalRotation;
            if (w.coll.isGrounded)
                onGround = true;
            float slip = cornerSlip + (w.powered ? driveSlip : 0.0f) + (w.handbraked ? handbrakeSlip : 0.0f);
            totalSlip += slip;
            WheelHit hit;
            WheelCollider c;
            c = w.coll;
            if (c.GetGroundHit(out hit))
            {
                Vector3 t = w.graphic.localPosition; t.y -= Vector3.Dot(w.graphic.position - hit.point, transform.up) - w.coll.radius; w.graphic.localPosition = t;
                if (slip > 0.5 && hit.collider.tag == "Dusty")
                {
                    groundDustEffect.position = hit.point;
                    groundDustEffect.particleEmitter.worldVelocity = rigidbody.velocity * 0.5f;
                    groundDustEffect.particleEmitter.minEmission = (slip - 0.5f) * 3f;
                    groundDustEffect.particleEmitter.maxEmission = (slip - 0.5f) * 3f;
                    groundDustEffect.particleEmitter.Emit();
                }
                if (slip > 0.75 && skidmarks != null)
                    w.lastSkidMark = skidmarks.AddSkidMark(hit.point, hit.normal, (slip - 0.75f) * 2f, w.lastSkidMark);
                else
                    w.lastSkidMark = -1;
            }
            else w.lastSkidMark = -1;
        }
        totalSlip /= wheels.Length;
    }
    void AutomaticTransmission()
    {
        if (gear > 0)
        {
            if (engineRPM > shiftUpRPM && gear < gearRatios.Length - 1)
                gear++;
            if (engineRPM < shiftDownRPM && gear > 1)
                gear--;
        }
    }
    float CalcEngine()
    {
        if (brake + handbrake > 0.1)
            motor = 0.0f;
        if (!onGround)
        {
            engineRPM += (motor - 0.3f) * 25000.0f * Time.deltaTime;
            engineRPM = Mathf.Clamp(engineRPM, minRPM, maxRPM);
            return 0.0f;
        }
        else
        {
            AutomaticTransmission();
            engineRPM = wheelRPM * gearRatios[gear] * finalDriveRatio;
            if (engineRPM < minRPM)
                engineRPM = minRPM;
            if (engineRPM < maxRPM)
            {
                float x = (2 * (engineRPM / maxRPM) - 1);
                float torqueCurve = 0.5f * (-x * x + 2f);
                float torqueToForceRatio = gearRatios[gear] * finalDriveRatio / wheelRadius;
                return motor * maxTorque * torqueCurve * torqueToForceRatio;
            }
            else
                return 0.0f;
        }
    }
    void HandlePhysics()
    {
        Vector3 velo = rigidbody.velocity;
        wheelRPM = velo.magnitude * 60.0f * 0.5f;
        rigidbody.angularVelocity = new Vector3(rigidbody.angularVelocity.x, 0.0f, rigidbody.angularVelocity.z);
        Vector3 dir = transform.TransformDirection(Vector3.forward);
        Vector3 flatDir = Vector3.Normalize(new Vector3(dir.x, 0, dir.z));
        Vector3 flatVelo = new Vector3(velo.x, 0, velo.z);
        rev = Mathf.Sign(Vector3.Dot(flatVelo, flatDir));
        if ((rev < 0 || flatVelo.sqrMagnitude < 0.5) && brake > 0.1)
            gear = 0;
        if (gear == 0)
        {
            float tmp = brake;
            brake = motor;
            motor = tmp;
            if ((rev > 0 || flatVelo.sqrMagnitude < 0.5) && brake > 0.1)
                gear = 1;
        }
        Vector3 engineForce = flatDir * CalcEngine();
        float totalbrake = brake + handbrake * 0.5f;
        if (totalbrake > 1.0f) totalbrake = 1.0f;
        Vector3 brakeForce = -flatVelo.normalized * totalbrake * rigidbody.mass * maxBrakeAccel;
        flatDir *= flatVelo.magnitude;
        flatDir = Quaternion.AngleAxis(steer * 30.0f, Vector3.up) * flatDir;
        flatDir *= rev;
        float diff = (flatVelo - flatDir).magnitude;
        float cornerAccel = maxCornerAccel;
        if (cornerAccel > diff) cornerAccel = diff;
        Vector3 cornerForce = -(flatVelo - flatDir).normalized * cornerAccel * rigidbody.mass;
        cornerSlip = Mathf.Pow(cornerAccel / maxCornerAccel, 3);
        rigidbody.AddForceAtPosition(brakeForce + engineForce + cornerForce, transform.position + transform.up * wheelY);
        float handbrakeFactor = 1 + handbrake * 4;
        if (rev < 0)
            handbrakeFactor = 1;
        float veloSteer = ((15 / (2 * velo.magnitude + 1)) + 1) * handbrakeFactor;
        float steerGrip = (1 - handlingTendency * cornerSlip);
        if (rev * steer * steerVelo < 0)
            steerGrip = 1;
        float maxRotSteer = 2 * Time.fixedDeltaTime * handbrakeFactor * steerGrip;
        float fVelo = velo.magnitude;
        float veloFactor = fVelo < 1.0 ? fVelo : Mathf.Pow(velo.magnitude, 0.3f);
        float steerVeloInput = rev * steer * veloFactor * 0.5f * Time.fixedDeltaTime * handbrakeFactor;
        if (velo.magnitude < 0.1)
            steerVeloInput = 0f;
        if (steerVeloInput > steerVelo)
        {
            steerVelo += 0.02f * Time.fixedDeltaTime * veloSteer;
            if (steerVeloInput < steerVelo)
                steerVelo = steerVeloInput;
        }
        else
        {
            steerVelo -= 0.02f * Time.fixedDeltaTime * veloSteer;
            if (steerVeloInput > steerVelo)
                steerVelo = steerVeloInput;
        }

        steerVelo = Mathf.Clamp(steerVelo, -maxRotSteer, maxRotSteer);
        transform.Rotate(Vector3.up * steerVelo * 57.295788f);
    }
    protected void FixedUpdateCar()
    {

        if (onGround)
            HandlePhysics();
        else
            CalcEngine();
        UpdateWheels();
        audio.pitch = 0.5f + 0.2f * motor + 0.8f * engineRPM / maxRPM;
        audio.volume = 0.5f + 0.8f * motor + 0.2f * engineRPM / maxRPM;
    }
    void Detonate()
    {
        foreach (WheelData w in wheels)
            w.coll.gameObject.active = false;
        enabled = false;
    }

}
