//script by igor levochkin
using UnityEditor;
using System;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using GUI = UnityEngine.GUILayout;
using System.IO;

[ExecuteInEditMode]
public class InspectorSearch : EditorWindow
{
    string search = "";
    void OnGUI()
    {
        DrawObjects();
        DrawSearch();
    }
    private void DrawSearch()
    {
        search = EditorGUILayout.TextField(search);
        EditorGUIUtility.LookLikeInspector();
        if (search.Length > 1)
        {
            if (Selection.activeGameObject != null)
                foreach (var m in Selection.activeGameObject.GetComponents<Component>())
                {
                    SerializedObject so = new SerializedObject(m);
                    SerializedProperty pr = so.GetIterator();
                    pr.NextVisible(true);
                    do
                    {
                        if (pr.propertyPath.ToLower().Contains(search.ToLower()))
                            EditorGUILayout.PropertyField(pr);
                    }
                    while (pr.NextVisible(true));
                    so.ApplyModifiedProperties();
                }
        }
    }
    private void DrawObjects()
    {
        if (GUI.Button("Add"))
            if (!instances.Contains(Selection.activeGameObject))
                instances.Add(Selection.activeGameObject);
        List<GameObject> toremove = new List<GameObject>();
        foreach (var inst in instances)
        {
            GUI.BeginHorizontal();
            GameObject o = inst;
            if (o != null && GUI.Button(o.name))
                Selection.activeGameObject = o;
            if (GUI.Button("X", GUI.ExpandWidth(false)))
                toremove.Add(inst);
            GUI.EndHorizontal();
        }
        foreach (var inst in toremove)
            instances.Remove(inst);

    }
    [MenuItem("RTools/InspectorSearch")]
    static void rtoolsclick()
    {
        if (_ewnd == null) _ewnd = EditorWindow.GetWindow<InspectorSearch>();
    }
    static EditorWindow _ewnd;
    static EditorWindow ewnd
    {
        get
        {
            if (_ewnd == null) _ewnd = EditorWindow.GetWindow<InspectorSearch>();
            return _ewnd;
        }
    }
    public List<GameObject> instances = new List<GameObject>();
}