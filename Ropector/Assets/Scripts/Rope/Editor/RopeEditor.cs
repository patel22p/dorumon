using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ClothRope))]
public class RopeEditor : Editor
{
    ClothRope cRope;

    public override void OnInspectorGUI()
    {
        cRope = (ClothRope)target;

        cRope.ropeEnd = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Rope End","This is the end of the rope (Point B). This needs to be assigned a gameObject before the rope can be built."),cRope.ropeEnd, typeof(GameObject));
        cRope.ropeSegments = (int)Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Segments", "This controls how many segments are inserted through the length of your rope. The higher the value, the smoother the rope looks when it bends."), cRope.ropeSegments), 0, Mathf.Infinity);
        cRope.ropeRadialDetail = EditorGUILayout.IntSlider(new GUIContent("Radial Detail", "This controls how much detail your rope has around its core. You cannot go below 3 and the higher you go the greater the performance hit."), cRope.ropeRadialDetail, 3, 50);
        cRope.ropeWidth = (float)Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Rope Width", "This sets the radius of your rope"), cRope.ropeWidth), 0, Mathf.Infinity);
        cRope.ropeCanTearFromEnds = EditorGUILayout.Toggle(new GUIContent("Tearable Ends", "When enabled the game objects that are attached to the rope are able to be torn away."), cRope.ropeCanTearFromEnds);
        
        SceneView.RepaintAll();
    }
}
