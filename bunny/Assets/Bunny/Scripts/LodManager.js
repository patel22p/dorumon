/*

Automatically set LOD Levels of models
(C)2010 by Tom Vogt
Use and modify as you wish, but retain author credits (aka BSD license)


== Usage ==

    * Create an empty gameobject
    * Drag all LOD levels into it, so they become children of the LOD object
    * Attach this script to the empty gameobject
    * Set the number of meshes in the script, and drag them into the slots, from highest (closest) to lowest (far away)
    * Set the number of distances to '''one less''' than the number of meshes (beyond the highest distance will use the most far away mesh automatically)
    * Set the distances at which the script shall switch to the next lowest LOD. So the first number means "at this distance or less, use the highest LOD".
    * Press Play and drag the camera around to check if your distances are fine.



    */



public var Distances : float[];
public var Models : GameObject[];

private var current = -2;


function Start() {
    for (var i=0; i<Models.Length; i++) {
        Models[i].SetActiveRecursively(false);
    }
}

function Update() {
    var d = Vector3.Distance(camera.main.transform.position, transform.position);
    var stage = -1;
    for (var i=0; i<Distances.Length; i++) {
        if (d<Distances[i]) {
            stage=i;
            i=Distances.Length;
        }
    }
    if (stage==-1) stage=Distances.Length;
    if (current!=stage) {
        SetLOD(stage);
    }
}


function SetLOD(stage) {
    Models[stage].SetActiveRecursively(true);
    if (current>=0) Models[current].SetActiveRecursively(false);
    current = stage;
}
