using UnityEngine;
using System.Collections;
using doru;

public class Gun : Bs {
    internal Player pl;
    public Cam Cam { get { return pl.Cam; } }
    public Transform CamRnd { get { return pl.CamRnd; } }
    Timer timer = new Timer();
    AnimationState[] handsShoot{get{return pl.handsShoot;}}
    public AudioClip[] shootSound;

    public float shootBump = 1;
    public float shootCursor = 5;
    public float shootTime = .1f;
    float lastShoot;
    float cursorOffset;
	
    internal bool shooting;

    void Update () {
        timer.Update();
        shooting = Time.time - lastShoot < shootTime * 2;
        CamRnd.localRotation = Quaternion.Slerp(CamRnd.localRotation, Quaternion.identity, Time.deltaTime * 2);
        cursorOffset = Mathf.MoveTowards(cursorOffset, 0, Time.deltaTime * 20);
        if (IsMine)
        {
            if (Input.GetMouseButton(0) && Time.time - lastShoot > shootTime)
                CallRPC(RpcShoot, RPCMode.All, Cam.rotx, pl.roty);
        }
	}

    [RPC]
    private void RpcShoot(float rotx, float roty)
    {
        pl.audio.PlayOneShot(shootSound.Random(), .5f);
        Cam.rotx = rotx;
        pl.roty = roty;
        lastShoot = Time.time;
        var a = handsShoot.Random();
        pl.MuzzleFlash.renderer.material = pl.MuzzleFlashMaterials.Random();
        pl.MuzzleFlash.animation.Play();
        pl.MuzzleFlash2.GetComponentInChildren<Animation>().Play();
        Ray ray = Cam.cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        CamRnd.localRotation = Quaternion.Euler(CamRnd.localRotation.eulerAngles + (Random.insideUnitSphere + Vector3.left) * shootBump);
        ray.direction += Random.insideUnitSphere * cursorOffset * 0.005f + Random.insideUnitSphere * tempp * pl.controller.velocity.magnitude;

        cursorOffset = Mathf.Min(cursorOffset + shootCursor, 15);
        RaycastHit h;
        if (Physics.Raycast(ray, out h, 1000, 1 << LayerMask.NameToLayer("Level") | 1 << LayerMask.NameToLayer("Enemy") | 1 << LayerMask.NameToLayer("Dead")))
        {
            Player enemy = h.collider.transform.root.GetComponent<Player>();
            
            if (enemy != null)
            {
                CreateBlood(h,ray);
                ray = new Ray(h.point, ray.direction + Vector3.down + Random.insideUnitSphere * .4f);
                if (Physics.Raycast(ray, out h, 100, 1 << LayerMask.NameToLayer("Level")))
                {
                    GameObject g = (GameObject)Instantiate(pl.Plane, h.point + h.normal * .04f, Quaternion.LookRotation(h.normal));
                    g.transform.localScale = Vector3.one * 18;
                    g.renderer.material = pl.BloodDecals.Random();
                    g.transform.parent = _Game.Fx;
                }

                if (h.collider.name == "Bip01 Head")
                {
                    enemy.SetLife(0);
                    enemy.audio.PlayOneShot(pl.headShootSound.Random(), 6);
                }
                else
                    enemy.SetLife(enemy.Life - 25);                
            }
            else if (h.rigidbody != null)
            {
                CreateBlood(h, ray);
                timer.AddMethod(delegate { h.rigidbody.AddForceAtPosition(ray.direction * 1000, h.point); });
            }
            else
            {
                ((GameObject)Instantiate(pl.sparks, h.point, Quaternion.LookRotation(h.normal))).transform.parent = _Game.Fx;
                var g = (GameObject)Instantiate(pl.Plane, h.point + h.normal * .04f, Quaternion.LookRotation(h.normal));
                g.transform.localScale = Vector3.one * 1f;
                g.transform.parent = _Game.Fx;
                g.renderer.material = pl.BulletHoleMaterials.Random();
            }
        }
        a.time = 0;
        pl.handsAn.Play(a.name, PlayMode.StopSameLayer);
    }

    private void CreateBlood(RaycastHit h,Ray ray)
    {
        ((GameObject)Instantiate(pl.BloodPrefab, h.point, Quaternion.LookRotation(h.normal))).transform.parent = _Game.Fx;        
    }
    void OnRenderObject()
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
                    "} } }");
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
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
