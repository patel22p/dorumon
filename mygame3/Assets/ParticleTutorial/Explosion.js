
function Start ()
{
	OneShotEmitters();
	Explosion();
}

function OneShotEmitters ()
{
	yield; // make sure the particle emitters have actually started
	i = 0;
	while(i < transform.childCount)
	{
		child = transform.GetChild(i);
		if(child.particleEmitter)
		{
			child.particleEmitter.emit = false;
			child.GetComponent(ParticleAnimator).autodestruct = true;
		}
		i++;	
	}	
}

function Explosion ()
{
	// do explosion damage stuff here	
}