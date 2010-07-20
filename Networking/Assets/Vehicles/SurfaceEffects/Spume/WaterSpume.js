//Attach this script to an object to create a foam layer on top 
//of the attached trigger collider, which automatically creates foam whenever anything
//touches the trigger.

RequireComponent(BoxCollider);
RequireComponent(MeshFilter);
RequireComponent(MeshRenderer);

//maximal number of simultaneous foam particles
var maxParticles = 1024;
//maximal seconds of life for one particle
var maxLife = 16.0;	

private var index = 0;

//structure for each particle
class FoamParticle{
	var pos : Vector3;
	var velo : Vector3;
	var ext : Vector3;
	var intensity = 0.0;
	var startIntensity =0.0;
};
var foam : FoamParticle[];	
	
// Use this for initialization
function Start () {
	collider.isTrigger = true;

	//create particle array
	foam=new FoamParticle[maxParticles];
	for(i=0;i<maxParticles;i++)
		foam[i]=new FoamParticle();
		
	//create particle mesh
	if(GetComponent(MeshFilter).mesh==null)
		GetComponent(MeshFilter).mesh=new Mesh();
}
	
//add new foam particles in the extents ext around pos, with the maximal life of intensity
function AddFoamParticles(pos : Vector3,ext: Vector3,intensity : float)
{
	//flatten coordinates to all be on the water surface
	pos-=transform.position;
	pos.y=0;
	ext.y=0;
	
	intensity=Mathf.Clamp01(intensity);

	//determine number of particles based on ext and intensity
	sizeFactor=Mathf.Sqrt(ext.magnitude);
	if(sizeFactor<1)
		intensity*=sizeFactor;
	fnum=intensity*sizeFactor*Time.deltaTime*50;
	var num:int;
	num=Mathf.Floor(fnum);
	num+=(fnum-num)>Random.value?1:0;
		
	//create particles
	while(num>0)
	{
		rnd=Random.insideUnitSphere;
		rnd.y=0;
		foam[index].pos=pos+Vector3.Scale(rnd,ext);
		foam[index].velo=rnd*intensity;
		foam[index].intensity=Random.value*intensity;
		foam[index].startIntensity=foam[index].intensity;
		foam[index].ext=Random.onUnitSphere*(1.0+1.5*Random.value)*sizeFactor;
		foam[index].ext.y=0;
		index=(index+1)%maxParticles;
		num--;
	}
}

	
function Update () {
	//count and update active particles
	segmentCount=0;
	for(i=0;i<maxParticles;i++)
	{
		//decrease life
		foam[i].intensity-=Time.deltaTime/maxLife;
		if(foam[i].intensity>0)
		{
			//move particles
			foam[i].pos+=foam[i].velo*Time.deltaTime;
			//grow particles
			size=foam[i].ext.magnitude;
			foam[i].ext*=1+(Time.deltaTime*0.5/size);
			//increase count of active particles
			segmentCount++;
		}
	}	
	//calculate mesh data for particles
	vertices=new Vector3[segmentCount*4];
	colors=new Color[segmentCount*4];
	uvs = new Vector2[segmentCount*4];
	triangles=new int[segmentCount*6];
	segmentCount=0;
	for(i=0;i<maxParticles;i++)
		if(foam[i].intensity>0)
		{
			ext1=foam[i].ext;
			ext2=Vector3(ext1.z,0,-ext1.x);
			vertices[segmentCount*4+0]=foam[i].pos+ext1;
			vertices[segmentCount*4+1]=foam[i].pos+ext2;
			vertices[segmentCount*4+2]=foam[i].pos-ext1;
			vertices[segmentCount*4+3]=foam[i].pos-ext2;
			
			intensity=foam[i].intensity;
			if(foam[i].startIntensity-intensity<0.03)
				intensity*=((foam[i].startIntensity-intensity)/0.03);
			colors[segmentCount*4+0]=0.5*Color(intensity,intensity,intensity,1);
			colors[segmentCount*4+1]=0.5*Color(intensity,intensity,intensity,1);
			colors[segmentCount*4+2]=0.5*Color(intensity,intensity,intensity,1);
			colors[segmentCount*4+3]=0.5*Color(intensity,intensity,intensity,1);

			uvs[segmentCount*4+0]=Vector2(0,0);
			uvs[segmentCount*4+1]=Vector2(1,0);
			uvs[segmentCount*4+2]=Vector2(1,1);
			uvs[segmentCount*4+3]=Vector2(0,1);
			
			triangles[segmentCount*6+0]=segmentCount*4+0;
			triangles[segmentCount*6+1]=segmentCount*4+1;
			triangles[segmentCount*6+2]=segmentCount*4+2;
			
			triangles[segmentCount*6+3]=segmentCount*4+2;
			triangles[segmentCount*6+4]=segmentCount*4+0;
			triangles[segmentCount*6+5]=segmentCount*4+3;
			segmentCount++;			
		}
		
	//update mesh
	mesh = GetComponent(MeshFilter).mesh;
	mesh.Clear();
	mesh.vertices=vertices;
	mesh.triangles=triangles;
	mesh.colors=colors;
	mesh.uv=uvs;
}

//get the rigidbody attached to a component's gameobject or it's anchestors
function GetComponentRigidBody(cmp) : Rigidbody
{
	if(cmp.rigidbody!=null)
		return cmp.rigidbody;
	else if(cmp.transform.parent!=null)
		return GetComponentRigidBody(cmp.transform.parent);
	else
		return null;
}

//if something fell into the water...
function OnTriggerStay(other) 
{
	rb=GetComponentRigidBody(other);
	//...and it has a rigidbody...
	if(rb!=null)
	{
		//...create foam particles around the object, based on the velocity of the rigidbody
		pos=other.transform.position;
		AddFoamParticles(pos,other.bounds.extents,rb.velocity.magnitude*0.05);
	}
}

