var speed = 0.00;
var explosion : GameObject;

function Update ()
{
	transform.Translate(Vector3.forward * speed * Time.deltaTime);
	var hit : RaycastHit;
	if(Physics.Raycast(transform.position, transform.forward, hit, speed * Time.deltaTime))
	{
		if(explosion) Instantiate(explosion, hit.point, Quaternion.LookRotation(hit.normal) );
		DetachEmitters();
		Destroy(gameObject);	
	}
}

function DetachEmitters ()
{
	i = 0;
	while(i < transform.childCount)
	{
		child = transform.GetChild(i);
		if(child.particleEmitter)
		{
			child.parent = null;
			child.particleEmitter.emit = false;
			child.GetComponent(ParticleAnimator).autodestruct = true;
		}
		i++;	
	}	
}