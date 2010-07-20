var runSpeedScale = 1.0;
var walkSpeedScale = 1.0;
var torso : Transform;

function Awake ()
{
	// By default loop all animations
	animation.wrapMode = WrapMode.Loop;

	// We are in full control here - don't let any other animations play when we start
	animation.Stop();
	animation.Play("idle");
}

function Update ()
{
	var marioController : ThirdPersonController = GetComponent(ThirdPersonController);
	var currentSpeed = marioController.GetSpeed();

	// Fade in run
	if (currentSpeed > marioController.walkSpeed)
	{
		animation.CrossFade("run");
		// We fade out jumpland quick otherwise we get sliding feet
		animation.Blend("jumpland", 0);
		SendMessage("SyncAnimation", "run");
	}
	// Fade in walk
	else if (currentSpeed > 0.1)
	{
		animation.CrossFade("walk");
		// We fade out jumpland realy quick otherwise we get sliding feet
		animation.Blend("jumpland", 0);
		SendMessage("SyncAnimation", "walk");
	}
	// Fade out walk and run
	else
	{
		animation.CrossFade("idle");
		SendMessage("SyncAnimation", "idle");
	}
	
	animation["run"].normalizedSpeed = runSpeedScale;
	animation["walk"].normalizedSpeed = walkSpeedScale;
	
	if (marioController.IsJumping ())
	{
		if (marioController.IsCapeFlying())
		{
			animation.CrossFade ("jetpackjump", 0.2);
			SendMessage("SyncAnimation", "jetpackjump");
		}
		else if (marioController.HasJumpReachedApex ())
		{
			animation.CrossFade ("jumpfall", 0.2);
			SendMessage("SyncAnimation", "jumpfall");
		}
		else
		{
			animation.CrossFade ("jump", 0.2);
			SendMessage("SyncAnimation", "jump");
		}
	}
	// We fell down somewhere
	else if (!marioController.IsGroundedWithTimeout ())
	{
		animation.CrossFade ("ledgefall", 0.2);
		SendMessage("SyncAnimation", "ledgefall");
	}
	// We are not falling down anymore
	else
	{
		animation.Blend ("ledgefall", 0.0, 0.2);
	}
}

function DidLand () {
	animation.Play("jumpland");
	SendMessage("SyncAnimation", "jumpland");
}

function DidPunch () {
	animation.CrossFadeQueued("punch", 0.3, QueueMode.PlayNow);
}

function DidButtStomp () {
	animation.CrossFade("buttstomp", 0.1);
	SendMessage("SyncAnimation", "buttstomp");
	animation.CrossFadeQueued("jumpland", 0.2);
}

function ApplyDamage () {
	animation.CrossFade("gothit", 0.1);
	SendMessage("SyncAnimation", "gothit");
}


function DidWallJump ()
{
	// Wall jump animation is played without fade.
	// We are turning the character controller 180 degrees around when doing a wall jump so the animation accounts for that.
	// But we really have to make sure that the animation is in full control so 
	// that we don't do weird blends between 180 degree apart rotations
	animation.Play ("walljump");
	SendMessage("SyncAnimation", "walljump");
}

@script AddComponentMenu ("Third Person Player/Third Person Player Animation")