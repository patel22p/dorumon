using UnityEngine;
using System.Collections;

public class TestMonster : MonoBehaviour {

    private PathFinder myFinder;
    private Vector3 DesiredPosition;

    public Transform DesiredTarget;
    public WaypointNodeParent Path;

    // Timers
    public float UpdateInterval = 0.5f; // 500ms

    // How long did it take to find a path...
    private float timeForFinding;

    private bool showNodes = false;

    private bool findPath = false;
    private bool startMoving = false;
    private int pointInPath = 0;
    private bool instant = false;

    public int MonsterID;

    ArrayList myPath = new ArrayList();

	// Use this for initialization
	void Start ()
    {
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            instant = !instant;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0)&&MonsterID==1)
        {
            if (Path != null)
                myFinder = new PathFinder(Path.GetWaypoints);

            Ray myRay = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit myHit;
            if (Physics.Raycast(myRay, out myHit, Mathf.Infinity))
            {
                if (myHit.collider.tag == "pf_obj")
                {
                    if (myFinder != null)
                    {
                        DesiredTarget.transform.position = myHit.point;
                        myFinder.Start(transform.position, DesiredTarget.position);
                        findPath = true;
                        startMoving = false;
                        myPath.Clear();
                    }
                    else
                        Debug.Log("Casting error..");
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse1) && MonsterID == 2)
        {
            Ray myRay = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit myHit;
            if (Physics.Raycast(myRay, out myHit, Mathf.Infinity))
            {
                if (myHit.collider.tag == "pf_obj")
                {
                    DesiredTarget.transform.position = myHit.point;
                    myFinder.Start(transform.position, DesiredTarget.position);
                    findPath = true;
                    startMoving = false;
                    myPath.Clear();
                }
            }
        }
    }

	// Update is called once per frame
	void FixedUpdate ()
    {
        if (Input.GetKeyDown(KeyCode.F6) && Application.isEditor)
        {
            showNodes = !showNodes;
        }

        if (findPath)
        {
            if (instant)
            {
                while (myFinder.Update())
                {
                }

            }
            else
            {
                myFinder.Update();
            }

            if (!myFinder.Update())
            {
                myPath = myFinder.GetPath();
                findPath = false;
                startMoving = true;
                pointInPath = myPath.Count - 1;
            }
        }
        if (startMoving)
        {
            ListedNode point = null;
            if (pointInPath >= 0)
                point = (ListedNode)myPath[pointInPath];

            if (point != null)
            {
                transform.Translate(((point.Position - transform.position).normalized * Time.deltaTime * 15.0f));
                // transform.LookAt(point.Position);
                if (Vector3.Distance(transform.position, point.Position) < 0.2f)
                    pointInPath--;
            }
        }
	}

    void OnGUI()
    {
        if(instant)
            GUI.Label(new Rect(15, 15, 200, 50), "Instant method. (F5 to toggle)");
        else
            GUI.Label(new Rect(15, 15, 200, 50), "Slow method. (F5 to toggle)");
    }

    void OnDrawGizmos()
    {
        if (myFinder != null)
        {
            if (Application.isPlaying) myFinder.DebugMe();
            if (showNodes)
            {
                myFinder.ShowNodes();
            }
        }
    }
}
