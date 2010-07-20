//Floatable Water component -- required by boat to float on
RequireComponent(BoxCollider);

//wave parameters
var waveFreq1 = 0.0;
var waveXMotion1 = 0.08;
var waveYMotion1 = 0.015;
var waveFreq2 = 1.3;
var waveXMotion2 = 0.025;
var waveYMotion2 = 0.10;
var waveFreq3 = 0.3;
var waveXMotion3 = 0.125;
var waveYMotion3 = 0.03;

//how strong under water drag is compared to air drag
var waterDragFactor = 8;

function Start()
{
	collider.isTrigger = true;
}