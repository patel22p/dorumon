using UnityEngine;
using UnityEditor;
using System.Collections;

public class PaintableWindow : EditorWindow
{
    // Are we painting?
    public static bool isPaintOn
    {
        get { return paintOn; }
        set { paintOn = value; }
    }
    private static bool paintOn = false;

    public static PaintVars PaintVariables;

    // Dummy object
    GameObject paintDummy;

    [MenuItem("Object Paint/Paint Menu")]
    static void Init()
    {
        // Initialization.
        PaintableWindow window = (PaintableWindow)EditorWindow.GetWindow(typeof(PaintableWindow));
        PaintVariables.Init(); // Set the variables okAY
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset settings"))
        {
            PaintVariables.Init();
        }
        GUILayout.Label("Selected object:");
        PaintVariables.SelectedObj = (GameObject)EditorGUILayout.ObjectField(PaintVariables.SelectedObj, typeof(GameObject));
        GUILayout.EndHorizontal();

        PaintVariables.objectScale = EditorGUILayout.FloatField("Object scale: ", PaintVariables.objectScale);

        // Use normals as rotation modifier
        GUILayout.BeginHorizontal();
        PaintVariables.useNormals = GUILayout.Toggle(PaintVariables.useNormals, " Use normals as orientation ");
        GUILayout.EndHorizontal();

        // Ignore painted objects
        GUILayout.BeginHorizontal();
        PaintVariables.ignorePaintLayer = GUILayout.Toggle(PaintVariables.ignorePaintLayer, " Ignore painted objects (" + LayerMask.NameToLayer("ObjPaint") + ")");
        GUILayout.EndHorizontal();


        // Size yitter
        GUILayout.BeginHorizontal();
        PaintVariables.scaleYitter = GUILayout.Toggle(PaintVariables.scaleYitter, " Use specific scale yitter");
        GUILayout.EndHorizontal();

        // More scaleYitter
        if (PaintVariables.scaleYitter)
        {
            GUILayout.BeginHorizontal();
            PaintVariables.minYitter = EditorGUILayout.Vector3Field("Minimum Axis yitter XYZ", PaintVariables.minYitter);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            PaintVariables.maxYitter = EditorGUILayout.Vector3Field("Maximum yitter XYZ", PaintVariables.maxYitter);
            GUILayout.EndHorizontal();
        }

        // Uniform scaleyitter
        GUILayout.BeginHorizontal();
        PaintVariables.uniformscaleYitter = GUILayout.Toggle(PaintVariables.uniformscaleYitter, " Use uniform scale yitter");
        GUILayout.EndHorizontal();

        if (PaintVariables.uniformscaleYitter)
        {
            GUILayout.BeginHorizontal();
            PaintVariables.uniYitter = EditorGUILayout.Vector2Field("Uniform scale yitter ( min / max) ", PaintVariables.uniYitter);
            GUILayout.EndHorizontal();
        }

        // Axis Yitter
        GUILayout.BeginHorizontal();
        PaintVariables.axisYitter = GUILayout.Toggle(PaintVariables.axisYitter, " Use UP Axis yitter");
        GUILayout.EndHorizontal();

        if (PaintVariables.axisYitter)
        {
            GUILayout.BeginHorizontal();
            PaintVariables.maxAxisYitterAngle = EditorGUILayout.FloatField("Maximum Angle yitter:", PaintVariables.maxAxisYitterAngle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            PaintVariables.minAxisYitterAngle = EditorGUILayout.FloatField("Minimum Angle yitter:", PaintVariables.minAxisYitterAngle);
            GUILayout.EndHorizontal();
        }

        // Radius
        GUILayout.BeginHorizontal();
        GUILayout.Label("Brush size: " + PaintVariables.brushRadius);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        PaintVariables.brushRadius = GUILayout.HorizontalSlider(PaintVariables.brushRadius, 1.0f, 10.0f);
        GUILayout.EndHorizontal();

        // Intensity
        GUILayout.BeginHorizontal();
        GUILayout.Label("Brush paint step: " + (int)PaintVariables.brushIntensity);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        PaintVariables.brushIntensity = (int)GUILayout.HorizontalSlider(PaintVariables.brushIntensity, 300, 5);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Paint current object.");
        if (GUILayout.Button("GO!"))
        {
            paintOn = !paintOn;
        }
        GUILayout.EndHorizontal();

        if (paintOn)
        {
            GUILayout.Box("ON ON ON ON ON");
        }
        else
        {
            GUILayout.Box("OFF OFF OFF OFF OFF");
        }
    }

    void Update()
    {
        if (PaintVariables.myObj != PaintVariables.SelectedObj) PaintVariables.myObj = PaintVariables.SelectedObj;

        if (paintOn)
        {
            if (!paintDummy)
            {
                paintDummy = new GameObject("Paint dummy - Just Ignore me.");
                paintDummy.AddComponent<PaintDummy>();
                Selection.activeGameObject = paintDummy;
            }
        }
        else if (!paintOn)
        {
            if (paintDummy) DestroyImmediate(paintDummy);
        }

        if (paintDummy != Selection.activeGameObject)
        {
            DestroyImmediate(paintDummy);
            paintOn = false;
            PaintVariables.SelectedObj = Selection.activeGameObject;
            Repaint();
        }
    }

    void OnDestroy()
    {
        DestroyImmediate(paintDummy);
    }
}

public struct PaintVars
{
    // Tool variables
    public GameObject myObj;           // The current selected gameobject
    public GameObject SelectedObj;

    // Rotation axle
    public float maxAxisYitterAngle;
    public float minAxisYitterAngle;
    public bool axisYitter;

    // Paint variables
    public bool paintOn;       // Is the painting tool enabled
    public bool painting;      // Are we currently trying to paint in the scene
    public LayerMask myLayer;

    // Randomnes variables
    public bool useNormals;
    public bool scaleYitter;
    public bool uniformscaleYitter;
    public bool ignorePaintLayer;
    public Vector3 minYitter;
    public Vector3 maxYitter;
    public Vector3 uniYitter;

    // Object variables
    public float objectScale;

    // Editable variables
    public float brushRadius;
    public int brushIntensity;

    public void Init()
    {
        SelectedObj = Selection.activeGameObject;           // The current selected gameobject

        // Rotation axle
        maxAxisYitterAngle = 10.0f;
        minAxisYitterAngle = 0.0f;
        axisYitter = false;

        // Paint variables
        paintOn = false;       // Is the painting tool enabled
        painting = false;      // Are we currently trying to paint in the scene
        myLayer = ~(1 << LayerMask.NameToLayer("ObjPaint"));

        // Randomnes variables
        useNormals = true;
        scaleYitter = false;
        uniformscaleYitter = false;
        ignorePaintLayer = true;
        minYitter = new Vector3(-0.25f, -0.25f, -0.25f);
        maxYitter = new Vector3(-0.25f, -0.25f, -0.25f);
        uniYitter = new Vector2(-0.2f, 0.2f);

        // Object variables
        objectScale = 1.0f;

        // Editable variables
        brushRadius = 5.0f;
        brushIntensity = 100;
    }
}