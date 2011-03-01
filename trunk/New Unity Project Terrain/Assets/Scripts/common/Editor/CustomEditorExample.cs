using UnityEditor;
using System;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using GUI = UnityEngine.GUILayout;
using gui = UnityEditor.EditorGUILayout;
using Object = UnityEngine.Object;
using System.IO;
using doru;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections;


[CustomEditor(typeof(Attach))]
public class CustomEditorExample : Editor
{

    public override void OnInspectorGUI()
    {

        var t = (Attach)target;
        t.OnInspectorGUI();
        base.OnInspectorGUI();        
    }
}