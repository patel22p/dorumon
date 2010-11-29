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
    public List<string> instances = new List<string>();
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
        if (search.Length > 0)
        {
            if (Selection.activeGameObject != null)
                foreach (var m in Selection.activeGameObject.GetComponents<Component>())
                {
                    SerializedObject so = new SerializedObject(m);
                    SerializedProperty pr = so.GetIterator();
                    pr.NextVisible(true);
                    do
                    {
                        if (pr.propertyPath.ToLower().Contains(search.ToLower()) && pr.editable)
                            EditorGUILayout.PropertyField(pr);
                        if (so.ApplyModifiedProperties() && Selection.gameObjects.Length > 1)
                            SetMultiSelect(m, pr);
                    }
                    while (pr.NextVisible(true));
                }
        }
    }

    private void SetMultiSelect(Component m, SerializedProperty pr)
    {
        switch (pr.propertyType)
        {
            case SerializedPropertyType.Float:
                SetValue(m, pr.floatValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.Boolean:
                SetValue(m, pr.boolValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.Integer:
                SetValue(m, pr.intValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.String:
                SetValue(m, pr.stringValue, pr.propertyPath, pr.propertyType);
                break;
        }
    }
    void SetValue(Component c, object value, string prName, SerializedPropertyType type)
    {
        foreach (var o in Selection.gameObjects)
        {
            Component nc = o.GetComponent(c.GetType());
            if (nc != null && nc != c)
            {
                SerializedObject so = new SerializedObject(nc);
                var pr = so.FindProperty(prName);
                switch (type)
                {
                    case SerializedPropertyType.Float:
                        pr.floatValue = (float)value;
                        break;
                    case SerializedPropertyType.Boolean:
                        pr.boolValue = (bool)value;
                        break;
                    case SerializedPropertyType.String:
                        pr.stringValue = (string)value;
                        break;
                    case SerializedPropertyType.Integer:
                        pr.intValue = (int)value;
                        break;

                }

                so.ApplyModifiedProperties();
            }
        }
    }
    private void DrawObjects()
    {
        if (GUI.Button("Add"))
            if (!instances.Contains(Selection.activeGameObject.name))
                instances.Add(Selection.activeGameObject.name);
        List<string> toremove = new List<string>();
        foreach (var inst in instances)
        {
            GUI.BeginHorizontal();
            GameObject o = (GameObject)GameObject.FindObjectsOfTypeIncludingAssets(typeof(GameObject)).FirstOrDefault(a => a.name == inst);
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

}