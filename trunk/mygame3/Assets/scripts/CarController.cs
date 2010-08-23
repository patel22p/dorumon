using UnityEngine;
using System.Collections;
using doru;


public class CarController : Car
{
    public Transform exp;
    public Transform nitropref;

    protected override void Start()
    {
        base.Start();
        rigidbody.angularDrag = 5;
        StartCar();
        Life = 200;
        Reset();

    }

    double timedown1;
    protected override void Update()
    {
        audio.enabled = OwnerID != null;
        base.Update();
        if (_LocalPlayer == null) return;
        rigidbody.drag = OwnerID == null ? 1 : 0;
        if (isOwner)
        {
            if (Mathf.Abs(clamp(transform.rotation.eulerAngles.z)) > 70 || Mathf.Abs(clamp(transform.rotation.eulerAngles.x)) > 70)
                timedown1 += _TimerA._SecodsElapsed;
            else
                timedown1 = 0;
            if (timedown1 > 3)
            {
                transform.rotation = Quaternion.identity;
            }


            brake = Mathf.Clamp01(-Input.GetAxis("Vertical"));
            handbrake = Input.GetButton("Jump") ? 1f : 0.0f;
            steer = Input.GetAxis("Horizontal");
            motor = Mathf.Clamp01(Input.GetAxis("Vertical"));

            if (Input.GetKeyDown(KeyCode.LeftShift)) RPCNitro(true);
            if (Input.GetKeyUp(KeyCode.LeftShift)) RPCNitro(false);
            if (Input.GetKeyDown(KeyCode.F))
            {
                _LocalPlayer.RPCShow(true);
                _LocalPlayer.RCPSelectGun(1);
                _LocalPlayer.transform.position = transform.position + new Vector3(0, 1.5f, 0); ;
                _localiplayer = _LocalPlayer;
                RPCCarOut(Network.player);
                RPCResetOwner();
            }
            //if (Input.GetButton("Jump")) rigidbody.velocity *= .99f;
        }
        else
        {
            if (transform.Find("door").collider.bounds.Contains(_LocalPlayer.transform.position)
                && _LocalPlayer.enabled && Input.GetKeyDown(KeyCode.F) && OwnerID == null)
            {
                _LocalPlayer.RPCShow(false);
                RPCSetOwner();
                _localiplayer = this;
                RPCCarIn(Network.player);
            }
        }
    }
    [RPC]
    private void RPCNitro(bool p)
    {
        CallRPC(true, p);
        nitroenabled = p;
    }
    [RPC]
    private void RPCCarIn(NetworkPlayer np)
    {
        CallRPC(true, np);
        _Spawn.players[np].car = this;
        transform.Find("door").GetComponent<AudioSource>().Play();
    }
    bool nitroenabled;
    protected virtual void FixedUpdate()
    {

        if (nitroenabled && nitro > 0)
        {
            foreach (Renderer r in nitropref.GetComponentsInChildren<Renderer>())
                r.enabled = true;
            this.rigidbody.AddForce(this.transform.rotation * new Vector3(0, 0, 50000));
            nitro -= .2f;
        }
        else
            foreach (Renderer r in nitropref.GetComponentsInChildren<Renderer>())
                r.enabled = false;
                
        if (OwnerID != null)
            FixedUpdateCar();
    }
    [RPC]
    private void RPCCarOut(NetworkPlayer np)
    {
        CallRPC(true, np);
        rigidbody.velocity = Vector3.zero;
        _Spawn.players[np].car = null;
    }
    public override void RPCDie()
    {

        Destroy(Instantiate(exp, transform.position, Quaternion.identity), 10);
        if (isOwnerOrServer)
            _TimerA.AddMethod(10000, RPCSpawn);
        if (isOwner)
        {
            RPCCarOut(Network.player);
            _localiplayer = _LocalPlayer;
            RPCResetOwner();
            _LocalPlayer.killedyby = killedyby;
            _LocalPlayer.RPCSetLife(-2);
            Screen.lockCursor = false;
        }

        Destroy(Instantiate(brokencar, transform.position, transform.rotation), 20);
        Show(false);

    }
    public Transform brokencar;
    public Transform effects;
    [RPC]
    public void RPCSpawn()
    {
        CallRPC(true);
        Show(true);
        foreach (Transform item in Base.getChild(effects))
            Destroy(item);
        transform.position = spawnpos;
        Life = life;
        Reset();
    }

    private void Reset()
    {
        foreach (GunBase gunBase in guns)
            gunBase.Reset();
    }
    const int life = 300;

}
