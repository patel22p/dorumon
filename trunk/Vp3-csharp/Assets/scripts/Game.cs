using System.Linq;
using System.Collections.Generic;
//using UnityEditor;
using UnityEngine;
using doru;

public class Game : bs
{

    public override void Awake()
    {
        _Game = this;
        if (!isEditor)
            blur.enabled = true;
        base.Awake();
    }

    public bool FollowCamera;
    public Transform Level;
    public float Jump = 1;
    public BlurEffect blur;
    public GUIText tmText;
    public GameObject Dead;
    public bool Inverse;
    public Transform DiePos;
    //public Transform Win;
    internal List<Player> players = new List<Player>();
    internal List<Transform> buttons = new List<Transform>();
    internal List<Transform> doors = new List<Transform>();
    internal List<Transform> scores = new List<Transform>();
    internal List<Laser> LaserGuns = new List<Laser>();
    public ParticleEmitter sparks;
    private Timer timer = new Timer();
    private Vector3 oldMouse;
    public Vector2 MouseSensivity = Vector2.one;
    private Vector2 MouseOffset;
    public float scroll = 1;
    public Transform Cam;
    public Transform Win;
    //public Vector3 eu;
    public bool Android;
    public Transform Spawn;
    public float LevelSpeed = 1;
    public  void Start()
    {
        Spawn.position = _Player.transform.position;
        buttons = GameObject.FindGameObjectsWithTag("Button").Select(a => a.transform).ToList();
        doors = GameObject.FindGameObjectsWithTag("Door").Select(a => a.transform).ToList();
        scores = GameObject.FindGameObjectsWithTag("Score").Select(a => a.transform).ToList();
        LaserGuns = GameObject.FindGameObjectsWithTag("LaserGun").Select(a => a.GetComponent<Laser>()).ToList();
        players = GameObject.FindGameObjectsWithTag("Player").Select(a => a.GetComponent<Player>()).ToList();

        foreach (var sc in scores)
            sc.GetComponentInChildren<Animation>().animation["score"].time = Random.value;

        foreach (var a in this.transform.GetComponentsInChildren<Animation>().Where(a => a.clip != null))
            a["Take 001"].speed = LevelSpeed;
    }

    public  void Respawn()
    {
        _Player.transform.position = Spawn.position;
        _Player.rigidbody.velocity = Vector3.zero;
        _Player.rigidbody.angularVelocity = Vector3.zero;
    }
    public  void Die()
    {
        var a = (GameObject)Instantiate(Dead, _Player.pos, Quaternion.identity);
        foreach (Transform b in a.transform)
        {
            b.renderer.material = _Player.renderer.material;
            b.rigidbody.velocity = _Player.rigidbody.velocity;
            b.rigidbody.mass = .01f;
        }
        Destroy(a, 5);
        Respawn();
    }

    public float grav = 30;
    public float clamp = 30;
    private bool winGame;
    public void Update()
    {

        UpdateWinGame();
        if (winGame) return;
        UpdateOther();
        UpdateButtons();
        UpdateScores();
        UpdateLaserGuns();
        UpdateInput();
        timer.Update();
    }

    private void UpdateOther()
    {
        if (scores.Count == 0 && (Win.position - new Vector3(players.Average(a => a.posx), players.Average(a => a.posy), players.Average(a => a.posz))).magnitude < 1)
            winGame = true;
        foreach (var p in players)
            p.rigidbody.WakeUp();
        if (blur.enabled)
        {
            if (blur.iterations == 0) blur.enabled = false;
            if (timer.TimeElapsed(30))
                blur.iterations--;
        }
        if (_Player.posy < DiePos.position.y)
            Respawn();
    }

    private void UpdateWinGame()
    {
        if (winGame)
        {            
            blur.enabled = true;
            if (timer.TimeElapsed(30))
                blur.iterations++;
            var a = Application.loadedLevel + 1;
            if (a >= Application.levelCount) a = 0;
            if (blur.iterations > 30) Application.LoadLevel(a);
            timer.Update();

        }
    }

    private void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (Android)
        {
            var e = Quaternion.LookRotation(Input.acceleration).eulerAngles;
            print(e);
            var q = Quaternion.Euler(180, 0, 0) * Quaternion.Euler(e.y, 0, e.x);
            Cam.transform.rotation = Quaternion.Lerp(Cam.transform.rotation, q, Time.deltaTime * 10);
            //rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.Lerp(transform.rotation, q, Time.deltaTime * Smooth), maxMove));
        }
        else
        {
            if (Input.GetMouseButton(0) && oldMouse != Vector3.zero)
            {
                Vector2 v = (Inverse ? -1 : 1) * (oldMouse - Input.mousePosition);
                if (v.magnitude < 50)
                {
                    v.x *= MouseSensivity.x;
                    v.y *= MouseSensivity.y;
                    MouseOffset += v;
                    MouseOffset = Vector3.ClampMagnitude(MouseOffset, clamp);
                }
            }
            if(!Input.GetMouseButton(0))
            {
                MouseOffset *= .88f;
            }
            Cam.transform.rotation = Quaternion.Euler(MouseOffset.y, 0, -MouseOffset.x);
        }
        if (FollowCamera)
            Cam.transform.position = _Player.pos;
        Physics.gravity = Camera.main.transform.forward * grav;
        oldMouse = Input.mousePosition;
        
        scroll += Input.GetAxis("Mouse ScrollWheel") / 10;
        Cam.transform.localScale = Vector3.one * scroll;
        
    }

    private void UpdateScores()
    {

        for (int i = scores.Count - 1; i >= 0; i--)
        {
            var sc = scores[i];
            if (sc.rigidbody != null)
                sc.rigidbody.WakeUp();
            var dist = Vector3.Distance(_Player.transform.position, sc.position);
            var d = 3;
            var v = _Player.pos - sc.position;
            if (dist < d && !Physics.Raycast(new Ray(sc.position, v), v.magnitude, 1 << LayerMask.NameToLayer("Default")))
            {
                var norm = (_Player.transform.position - sc.position).normalized;
                sc.position += norm * (d - dist) * Time.deltaTime * 10;
            }

            if (dist < .5f)
            {
                scores.Remove(sc);
                Destroy(sc.gameObject);
            }
        }
    }

    private void UpdateButtons()
    {
        for (int i = buttons.Count - 1; i >= 0; i--)
        {
            var b = buttons[i];
            if ((_Player.pos - b.position).magnitude < 2)
            {
                b.renderer.material.SetColor("_Emission", Color.black);
                buttons.Remove(b);
                if (buttons.Count == 0)
                    foreach (var d in doors)
                        d.animation.Play();
            }
        }
    }

    private void UpdateLaserGuns()
    {
        foreach (var l in LaserGuns)
        {
            l.transform.Rotate(Vector3.up, Time.deltaTime * 10, Space.World);
            l.LineRenderer.SetPosition(0, l.position);
            RaycastHit h;
            Vector3 p;
            if (Physics.Raycast(l.pos, l.Target.position - l.pos, out h, 1000, 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Default")))
            {
                p = h.point;
                if (h.transform.GetComponent<Player>() != null)
                    Die();
            }
            else
                p = Quaternion.LookRotation(l.pos - l.Target.position) * Vector3.forward * 1000;

            l.LineRenderer.SetPosition(1, p);
            sparks.transform.position = p;
            sparks.transform.rotation = Quaternion.LookRotation(l.pos - l.Target.position);
            sparks.Emit();


        }
    }

    //public bool DrawGizmos = true;
    //public void OnDrawGizmos()
    //{
    //    if (Selection.activeGameObject != null && DrawGizmos)
    //        Gizmos.DrawWireSphere(Selection.activeGameObject.transform.position, Selection.activeTransform.localScale.y);
    //}
}
