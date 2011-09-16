using UnityEngine;
using System.Collections;
using doru;
using System.Linq;

public class Gun : Bs {
    internal Player pl;
    public Transform CamRnd { get { return pl.CamRnd; } }
    Timer timer = new Timer();
    AnimationState[] handsShoot{get{return pl.handsShoot;}}
    public AudioClip[] shootSound;
    public ParticleEmitter Capsules;
    public ParticleEmitter Capsules2;
    public float shootBump = 1;
    public float shootCursor = 5;
    public float shootTime = .1f;
    float lastShoot;
    float cursorOffset;
	
    internal bool shooting;

    public void Update () {
        timer.Update();
        shooting = Time.time - lastShoot < shootTime * 2;
        CamRnd.localRotation = Quaternion.Slerp(CamRnd.localRotation, Quaternion.identity, Time.deltaTime * 2);
        cursorOffset = Mathf.MoveTowards(cursorOffset, 0, Time.deltaTime * 20);
        if (IsMine && Input.GetMouseButton(0) && Time.time - lastShoot > shootTime && Screen.lockCursor)
            shoot();
    }

    
    public void shoot()
    {
        lastShoot = Time.time;
        
        
        Ray ray = pl.camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        CamRnd.localRotation = Quaternion.Euler(CamRnd.localRotation.eulerAngles + (Random.insideUnitSphere + Vector3.left) * shootBump);
        ray.direction +=
            (Random.insideUnitSphere * cursorOffset * 0.005f) +
            (Random.insideUnitSphere * 0.01f * pl.controller.velocity.magnitude) +
            (Time.time - pl.HitTime < 1f ? Random.insideUnitSphere * .01f : Vector3.zero);
        
        CallRPC(RPCShoot, RPCMode.All, ray.origin, ray.direction);
    }
    [RPC]
    private void RPCShoot(Vector3 rayOrg , Vector3 rayDir)
    {
        if (!pl.observing)
        {
            pl.MuzzleFlash2.GetComponentInChildren<Animation>().Play();
            Capsules2.Emit();
        }
        else
        {
            pl.MuzzleFlash.renderer.material = pl.MuzzleFlashMaterials.Random();
            pl.MuzzleFlash.animation.Play();
            Capsules.Emit();
        }
        Ray ray = new Ray(rayOrg, rayDir);
        pl.audio.PlayOneShot(shootSound.Random(), pl.observing ? .2f : 1);
        cursorOffset = Mathf.Min(cursorOffset + shootCursor, 15);

        foreach (var h in Physics.RaycastAll(ray, 1000, 1 << LayerMask.NameToLayer("Level") | 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("IgnoreColl")).Reverse()) //note use for
        {
            Player enemy = h.collider.transform.root.GetComponent<Player>();
            if (enemy != null)
            {                
                if (enemy == pl) continue;
                if (enemy.team != pl.team)
                {
                    CreateBlood(h);
                    ray = new Ray(h.point, ray.direction + Vector3.down + Random.insideUnitSphere * .4f);
                    RaycastHit h2;
                    if (Physics.Raycast(ray, out h2, 100, 1 << LayerMask.NameToLayer("Level")))
                    {
                        GameObject g = (GameObject)Instantiate(pl.Plane, h2.point + h2.normal * .04f, Quaternion.LookRotation(h2.normal));
                        g.transform.localScale = Vector3.one * 18;
                        g.renderer.material = pl.BloodDecals.Random();
                        g.transform.parent = _Game.Fx;
                    }
                    if (enemy.IsMine || Offline && !enemy.dead)
                    {

                        var angle = Mathf.DeltaAngle(Quaternion.LookRotation(pl.pos - enemy.pos).eulerAngles.y, enemy.rot.eulerAngles.y);
                        Debug.Log(angle);
                        if (angle > 45) _Hud.SetPainLeft(1);
                        if (angle < -45) _Hud.SetPainRight(1);

                        if (h.collider.name == "Bip01 Head")
                        {
                            enemy.CallRPC(enemy.SetLife, RPCMode.All, 0, pl.id);
                            enemy.audio.PlayOneShot(pl.headShootSound.Random(), 6);
                        }
                        else
                            enemy.CallRPC(enemy.SetLife, RPCMode.All, enemy.Life - 25, pl.id);
                    }
                }                
            }
            else if (h.rigidbody != null)
            {
                CreateBlood(h);
                //timer.AddMethod(delegate { });
                h.rigidbody.AddForceAtPosition(ray.direction * 1000, h.point); 
            }
            else
            {
                ((GameObject)Instantiate(pl.sparks, h.point, Quaternion.LookRotation(h.normal))).transform.parent = _Game.Fx;
                var g = (GameObject)Instantiate(pl.Plane, h.point + h.normal * .04f, Quaternion.LookRotation(h.normal));
                g.transform.localScale = Vector3.one * 1f;
                g.transform.parent = _Game.Fx;
                g.renderer.material = pl.BulletHoleMaterials.Random();
            }
            break;
        }
        var a = handsShoot.Random();
        a.time = 0;
        pl.handsAn.Play(a.name, PlayMode.StopSameLayer);
    }

    private void CreateBlood(RaycastHit h)
    {
        ((GameObject)Instantiate(pl.BloodPrefab, h.point, Quaternion.LookRotation(h.normal))).transform.parent = _Game.Fx;        
    }
    public void OnRenderObject()
    {
        if (IsMine)
        {
            LineMaterial.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.LINES);
            GL.Color(Color.green);
            foreach (var a in list)
            {
                var v = new Vector3(Screen.width, Screen.height) / 2 + new Vector3(a.x, a.y);
                v += a.normalized * (1 + cursorOffset);
                v.x /= Screen.width;
                v.y /= Screen.height;
                GL.Vertex(v);
            }
            GL.End();
        }
    }

    #region props

    static Material lineMaterial;

    static Material LineMaterial
    {
        get
        {
            if (!lineMaterial)
            {
                lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
                    "SubShader { Pass { " +
                    "    Blend SrcAlpha OneMinusSrcAlpha " +
                    "    ZWrite Off Cull Off Fog { Mode Off } " +
                    "    BindChannels {" +
                    "      Bind \"vertex\", vertex Bind \"color\", color }" +
                    "} } }") {hideFlags = HideFlags.HideAndDontSave, shader = {hideFlags = HideFlags.HideAndDontSave}};
            }
            return lineMaterial;
        }
    }
    const float d = 3, len = 5;
    Vector3[] list = new[]{ 
            Vector3.left *d, Vector3.left*(d+len),
            Vector3.up *d, Vector3.up*(d+len),
            Vector3.right *d, Vector3.right*(d+len),
            Vector3.down *d, Vector3.down*(d+len)};

    #endregion
}
