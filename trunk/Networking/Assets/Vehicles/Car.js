#pragma strict

//maximal corner and braking acceleration capabilities
var maxCornerAccel=10.0;
var maxBrakeAccel=10.0;

//center of gravity height - effects tilting in corners
var cogY = 0.0;

//engine powerband
var minRPM = 700;
var maxRPM = 6000;

//maximum Engine Torque
var maxTorque = 400;

//automatic transmission shift points
var shiftDownRPM = 2500;
var shiftUpRPM = 5500;

//gear ratios
var gearRatios = [-2.66, 2.66, 1.78, 1.30, 1.00];
var finalDriveRatio = 3.4;

//a basic handling modifier:
//1.0 understeer
//0.0 oversteer
var handlingTendency = 0.7;

//graphical wheel objects
var wheelFR : Transform;
var wheelFL : Transform;
var wheelBR : Transform;
var wheelBL : Transform;

//suspension setup
var suspensionDistance = 0.3;
var springs = 1000;
var dampers = 200;
var wheelRadius = 0.45;

//particle effect for ground dust
var groundDustEffect : Transform;

private var queryUserInput = true;
private var engineRPM : float;
private var steerVelo = 0.0;
private var brake = 0.0;
private var handbrake = 0.0;
private var steer = 0.0;
private var motor = 0.0;
private var skidTime = 0.0;
private var onGround = false;
private var cornerSlip = 0.0;
private var driveSlip = 0.0;
private var wheelRPM : float;
private var gear = 1;
private var skidmarks : Skidmarks;
private var wheels : WheelData[];
private var wheelY = 0.0;
private var rev = 0.0;

//Functions to be used by external scripts 
//controlling the car if required
//===================================================================

//return a status string for the vehicle
function GetStatus(gui : GUIText) {
	gui.text="v="+(rigidbody.velocity.magnitude * 3.6).ToString("f1") + " km/h\ngear= "+gear+"\nrpm= "+engineRPM.ToString("f0");
}

//return an information string for the vehicle
function GetControlString(gui : GUIText) {
	gui.text="Use arrow keys to control the jeep,\nspace for handbrake.";
}

//Enable or disable user controls
function SetEnableUserInput(enableInput)
{
	queryUserInput=enableInput;
}

//Car physics
//===================================================================

//some whee calculation data
class WheelData{
	var rotation = 0.0;
	var coll : WheelCollider;
	var graphic : Transform;
	var maxSteerAngle = 0.0;
	var lastSkidMark = -1;
	var powered = false;
	var handbraked = false;
	var originalRotation : Quaternion;
};

function Start () {
	//setup wheels
	wheels=new WheelData[4];
	for(i=0;i<4;i++)
		wheels[i] = new WheelData();
		
	wheels[0].graphic = wheelFL;
	wheels[1].graphic = wheelFR;
	wheels[2].graphic = wheelBL;
	wheels[3].graphic = wheelBR;

	wheels[0].maxSteerAngle=30.0;
	wheels[1].maxSteerAngle=30.0;
	wheels[2].powered=true;
	wheels[3].powered=true;
	wheels[2].handbraked=true;
	wheels[3].handbraked=true;

	for(w in wheels)
	{
		if(w.graphic==null)
			Debug.Log("You need to assign all four wheels for the car script!");
		if(!w.graphic.transform.IsChildOf(transform))	
			Debug.Log("Wheels need to be children of the Object with the car script");
			
		w.originalRotation = w.graphic.localRotation;

		//create collider
		colliderObject = new GameObject("WheelCollider");
		colliderObject.transform.parent = transform;
		colliderObject.transform.position = w.graphic.position;
		w.coll = colliderObject.AddComponent(WheelCollider);
		w.coll.suspensionDistance = suspensionDistance;
		w.coll.suspensionSpring.spring = springs;
		w.coll.suspensionSpring.damper = dampers;
		//no grip, as we simulate handling ourselves
		w.coll.forwardFriction.stiffness = 0;
		w.coll.sidewaysFriction.stiffness = 0;
		w.coll.radius = wheelRadius;
	}	

	//get wheel height (height forces are applied on)
	wheelY=wheels[0].graphic.localPosition.y;
	
	//setup center of gravity
	rigidbody.centerOfMass.y = cogY;
	
	//find skidmark object
	skidmarks = FindObjectOfType(typeof(Skidmarks));
	
	//shift to first
	gear=1;
}

//update wheel status
function UpdateWheels()
{
	//calculate handbrake slip for traction gfx
 	handbrakeSlip=handbrake*rigidbody.velocity.magnitude*0.1;
	if(handbrakeSlip>1)
		handbrakeSlip=1;
		
	totalSlip=0.0;
	onGround=false;
	for(w in wheels)
	{		
		//rotate wheel
		w.rotation += wheelRPM / 60.0 * -rev * 360.0 * Time.fixedDeltaTime;
		w.rotation = Mathf.Repeat(w.rotation, 360.0);		
		w.graphic.localRotation= Quaternion.Euler( w.rotation, w.maxSteerAngle*steer, 0.0 ) * w.originalRotation;

		//check if wheel is on ground
		if(w.coll.isGrounded)
			onGround=true;
			
		slip = cornerSlip+(w.powered?driveSlip:0.0)+(w.handbraked?handbrakeSlip:0.0);
		totalSlip += slip;
		
		var hit : WheelHit;
		var c : WheelCollider;
		c = w.coll;
		if(c.GetGroundHit(hit))
		{
			//if the wheel touches the ground, adjust graphical wheel position to reflect springs
			w.graphic.localPosition.y-=Vector3.Dot(w.graphic.position-hit.point,transform.up)-w.coll.radius;
			
			//create dust on ground if appropiate
			if(slip>0.5 && hit.collider.tag=="Dusty")
			{
				groundDustEffect.position=hit.point;
				groundDustEffect.particleEmitter.worldVelocity=rigidbody.velocity*0.5;
				groundDustEffect.particleEmitter.minEmission=(slip-0.5)*3;
				groundDustEffect.particleEmitter.maxEmission=(slip-0.5)*3;
				groundDustEffect.particleEmitter.Emit();								
			}
			
			//and skid marks				
			if(slip>0.75 && skidmarks != null)
				w.lastSkidMark=skidmarks.AddSkidMark(hit.point,hit.normal,(slip-0.75)*2,w.lastSkidMark);
			else
				w.lastSkidMark=-1;
		}
		else w.lastSkidMark=-1;
	}
	totalSlip/=wheels.length;
}

//Automatically shift gears
function AutomaticTransmission()
{
	if(gear>0)
	{
		if(engineRPM>shiftUpRPM&&gear<gearRatios.length-1)
			gear++;
		if(engineRPM<shiftDownRPM&&gear>1)
			gear--;
	}
}

//Calculate engine acceleration force for current RPM and trottle
function CalcEngine() : float
{
	//no engine when braking
	if(brake+handbrake>0.1)
		motor=0.0;
	
	//if car is airborne, just rev engine
	if(!onGround)
	{
		engineRPM += (motor-0.3)*25000.0*Time.deltaTime;
		engineRPM = Mathf.Clamp(engineRPM,minRPM,maxRPM);
		return 0.0;
	}
	else
	{
		AutomaticTransmission();
		engineRPM=wheelRPM*gearRatios[gear]*finalDriveRatio;
		if(engineRPM<minRPM)
			engineRPM=minRPM;
		if(engineRPM<maxRPM)
		{
			//fake a basic torque curve
			x = (2*(engineRPM/maxRPM)-1);
			torqueCurve = 0.5*(-x*x+2);
			torqueToForceRatio = gearRatios[gear]*finalDriveRatio/wheelRadius;
			return motor*maxTorque*torqueCurve*torqueToForceRatio;
		}
		else
			//rpm delimiter
			return 0.0;
	}
}

//Car physics
//The physics of this car are really a trial-and-error based extension of 
//basic "Asteriods" physics -- so you will get a pretty arcade-like feel.
//This may or may not be what you want, for a more physical approach research
//the wheel colliders
function HandlePhysics () {
	var velo=rigidbody.velocity;
	wheelRPM=velo.magnitude*60.0*0.5;

	rigidbody.angularVelocity=new Vector3(rigidbody.angularVelocity.x,0.0,rigidbody.angularVelocity.z);
	dir=transform.TransformDirection(Vector3.forward);
	flatDir=Vector3.Normalize(new Vector3(dir.x,0,dir.z));
	flatVelo=new Vector3(velo.x,0,velo.z);
	rev=Mathf.Sign(Vector3.Dot(flatVelo,flatDir));
	//when moving backwards or standing and brake is pressed, switch to reverse
	if((rev<0||flatVelo.sqrMagnitude<0.5)&&brake>0.1)
		gear=0;
	if(gear==0)
	{	
		//when in reverse, flip brake and gas
		tmp=brake;
		brake=motor;
		motor=tmp;
		
		//when moving forward or standing and gas is pressed, switch to drive
		if((rev>0||flatVelo.sqrMagnitude<0.5)&&brake>0.1)
			gear=1;
	}
	engineForce=flatDir*CalcEngine();
	totalbrake=brake+handbrake*0.5;
	if(totalbrake>1.0)totalbrake=1.0;
	brakeForce=-flatVelo.normalized*totalbrake*rigidbody.mass*maxBrakeAccel;

	flatDir*=flatVelo.magnitude;
	flatDir=Quaternion.AngleAxis(steer*30.0,Vector3.up)*flatDir;
	flatDir*=rev;
	diff=(flatVelo-flatDir).magnitude;
	cornerAccel=maxCornerAccel;
	if(cornerAccel>diff)cornerAccel=diff;
	cornerForce=-(flatVelo-flatDir).normalized*cornerAccel*rigidbody.mass;
	cornerSlip=Mathf.Pow(cornerAccel/maxCornerAccel,3);
	
	rigidbody.AddForceAtPosition(brakeForce+engineForce+cornerForce,transform.position+transform.up*wheelY);
	
	handbrakeFactor=1+handbrake*4;
	if(rev<0)
		handbrakeFactor=1;
	veloSteer=((15/(2*velo.magnitude+1))+1)*handbrakeFactor;
	steerGrip=(1-handlingTendency*cornerSlip);
	if(rev*steer*steerVelo<0)
		steerGrip=1;
	maxRotSteer=2*Time.fixedDeltaTime*handbrakeFactor*steerGrip;
	fVelo=velo.magnitude;
	veloFactor=fVelo<1.0?fVelo:Mathf.Pow(velo.magnitude,0.3);
	steerVeloInput=rev*steer*veloFactor*0.5*Time.fixedDeltaTime*handbrakeFactor;
	if(velo.magnitude<0.1)
		steerVeloInput=0;
	if(steerVeloInput>steerVelo)
	{
		steerVelo+=0.02*Time.fixedDeltaTime*veloSteer;
		if(steerVeloInput<steerVelo)
			steerVelo=steerVeloInput;
	}
	else
	{
		steerVelo-=0.02*Time.fixedDeltaTime*veloSteer;
		if(steerVeloInput>steerVelo)
			steerVelo=steerVeloInput;
	}
	steerVelo=Mathf.Clamp(steerVelo,-maxRotSteer,maxRotSteer);	
	transform.Rotate(Vector3.up*steerVelo*57.295788);
}

function FixedUpdate () {
	//query input axes if necessarry
	if(queryUserInput)
	{
		brake = Mathf.Clamp01(-Input.GetAxis("Vertical"));
		handbrake = Input.GetButton("Jump")?1.0:0.0;
		steer = Input.GetAxis("Horizontal");
	 	motor = Mathf.Clamp01(Input.GetAxis("Vertical"));
 	}
	else
	{
		motor = 0;
		steer = 0;
		brake = 0;
		handbrake = 0;
	}


	//if car is on ground calculate handling, otherwise just rev the engine
 	if(onGround)
		HandlePhysics();
	else
		CalcEngine();	
		
	//wheel GFX
	UpdateWheels();

	//engine sounds
	audio.pitch=0.5+0.2*motor+0.8*engineRPM/maxRPM;
	audio.volume=0.5+0.8*motor+0.2*engineRPM/maxRPM;
}

//Called by DamageReceiver if boat destroyed
function Detonate()
{
	//destroy wheels
	for( w in wheels )
		w.coll.gameObject.active=false;

	//no more car physics
	enabled=false;
}

@script RequireComponent (Rigidbody)
@script RequireComponent (AudioSource)
