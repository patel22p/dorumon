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


public class InspectorSearch : EditorWindow
{
    string search = "";
    float autosavetm = 0;    
    Type[] types = new Type[] { typeof(GameObject), typeof(Material) };
    protected virtual void OnGUI()
    {
        if (GUI.Button("Init"))
            foreach (var go in Selection.gameObjects)
                foreach (var scr in go.GetComponents<Base>())
                    scr.Init();
        foreach (var a in GameObject.FindGameObjectsWithTag("EditorGUI").Where(a => a != Selection.activeGameObject))
            a.GetComponent<Base>().OnEditorGui();

        if (Selection.activeGameObject != null)
        {
            var bs2 = Selection.activeGameObject.GetComponent<Base>();
            if (bs2 != null)
                bs2.OnEditorGui();

        }
        GUI.BeginHorizontal();
        Base.debug = GUI.Toggle(Base.debug, "debug", GUI.ExpandWidth(false));
        GUI.EndHorizontal();
        DrawSearch();
    }
    void OnSelectionChange()
    {        
        search = "";
        this.Repaint();
    }

    protected virtual void Update()
    {
        autosavetm += 0.01f;
        if (autosavetm > 60 * 2)
        {
            autosavetm = 0;
            Backup();
        }
    }

    [MenuItem("File/Backup")]
    private static void Backup()
    {
        if (!isPlaying && EditorApplication.currentScene.Contains(".unity"))
        {            
            EditorApplication.SaveAssets();
            EditorApplication.SaveScene(EditorApplication.currentScene);
        }
    }

    [MenuItem("Window/Rtools", false, 0)]
    static void rtoolsclick()
    {
        EditorWindow.GetWindow<InspectorSearch>();
    }
    [MenuItem("Assets/TakeScreenshot")]
    static void TakeScreenshot()
    {
        var g = UnityEditor.Selection.activeObject;
        var Texture = UnityEditor.EditorUtility.GetAssetPreview(g);
        File.WriteAllBytes("Assets/" + g.name + ".png", Texture.EncodeToPNG());
    }
    [MenuItem("Edit/Play % ")]
    private static void Play()
    {
        if (EditorApplication.isPlaying)
            EditorApplication.isPaused = !EditorApplication.isPaused;
        else
            if (!EditorApplication.isPlaying) EditorApplication.isPlaying = true;
    }
    [MenuItem("Assets/Create/Prefab", priority = 0)]
    static void CreatePrefabs()
    {
        Undo.RegisterSceneUndo("rtools");
        foreach (GameObject g in Selection.gameObjects)
        {
            if (!AssetDatabase.IsMainAsset(g))
            {
                var p = EditorUtility.CreateEmptyPrefab("Assets/" + g.name + ".prefab");
                EditorUtility.ReplacePrefab(g, p, ReplacePrefabOptions.ConnectToPrefab);
                EditorUtility.SetDirty(g);
            }
            AssetDatabase.Refresh();
        }
    }
    private void DrawSearch()
    {

        search = EditorGUILayout.TextField("search", search);
        EditorGUIUtility.LookLikeInspector();
        var ago = Selection.activeGameObject;
        var ao = Selection.activeObject;
        if (search.Length > 0 && types.Contains(ao.GetType()) && ao != null && !(ago != null && ago.camera != null))
        {
            IEnumerable<Object> array = new Object[] { ao };
            if (ago != null)
            {
                array = array.Union(ago.GetComponents<Component>());
                if (ago.renderer != null)
                    array = array.Union(new[] { ago.renderer.sharedMaterial });
            }
            foreach (var m in array)
            {

                SerializedObject so = new SerializedObject(m);
                SerializedProperty pr = so.GetIterator();

                pr.NextVisible(true);
                do
                {
                    if (pr.propertyPath.ToLower().Contains(search.ToLower())
                        || (pr.propertyType == SerializedPropertyType.String && pr.stringValue.ToLower().Contains(search.ToLower()))
                        || (pr.propertyType == SerializedPropertyType.Enum && pr.enumNames.Length >= 0 && pr.enumNames[pr.enumValueIndex].ToLower().Contains(search.ToLower()))
                        && pr.editable)
                        EditorGUILayout.PropertyField(pr);

                    if (so.ApplyModifiedProperties())
                    {
                        //Debug.Log(pr.name);
                        SetMultiSelect(m, pr);
                    }
                }
                while (pr.NextVisible(true));
            }
        }
    }
    private void SetMultiSelect(Object m, SerializedProperty pr)
    {
        switch (pr.propertyType)
        {
            case SerializedPropertyType.Float:
                MySetValue(m, pr.floatValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.Boolean:
                MySetValue(m, pr.boolValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.Integer:
                MySetValue(m, pr.intValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.String:
                MySetValue(m, pr.stringValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.Color:
                MySetValue(m, pr.colorValue, pr.propertyPath, pr.propertyType);
                break;
            case SerializedPropertyType.Enum:
                MySetValue(m, pr.enumValueIndex, pr.propertyPath, pr.propertyType);
                break;
        }
    }
    void MySetValue(Object c, object value, string prName, SerializedPropertyType type)
    {
        var array = Selection.gameObjects.Select(a => a.GetComponent(c.GetType())).Cast<Object>().Union(Selection.objects.Where(a => !(a is GameObject)));
        if (c is Material)
        {
            var d = Selection.gameObjects.Select(a => a.renderer).SelectMany(a => a.sharedMaterials).Distinct();
            array = array.Union(d.Cast<Object>());
        }

        foreach (var nc in array) //êîìïîíåíòû gameobjectîâ è âûáðàíûå Objectû
        {
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
                    case SerializedPropertyType.Color:
                        pr.colorValue = (Color)value;
                        break;
                    case SerializedPropertyType.Enum:
                        pr.enumValueIndex = (int)value;
                        break;
                }

                so.ApplyModifiedProperties();
            }
        }
    }
    public static bool isPlaying { get { return EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode; } }

}