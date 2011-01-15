using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using doru;
using System.Text.RegularExpressions;
using System.Linq;

public class Shared : Base
{
    public bool isController { get { return selected == Network.player.GetHashCode(); } }
    public Vector3 syncPos;
    public Quaternion syncRot;
    public Vector3 syncVelocity;
    public Vector3 syncAngularVelocity;
    public Vector3 spawnpos;
    public Quaternion spawnrot;
    public bool velSync = true, posSync = true, rotSync = true, angSync = true, Sync = true;
    public int selected = -1;
    public float tsendpackets;
    public bool shared = true;
    public Renderer[] renderers;
    [FindAsset("collision1")]
    public AudioClip soundcollision;
    protected override void Awake()
    {
        renderers = this.GetComponentsInChildren<Renderer>().Distinct().ToArray();
        base.Awake();
    }
    public override void Init()
    {
        foreach (Transform t in transform.GetTransforms())
        {
            t.gameObject.isStatic = false;
            t.gameObject.layer = LayerMask.NameToLayer("Default");
        }
        gameObject.AddOrGet<NetworkView>().observed = this;
        gameObject.AddOrGet<Rigidbody>();
        gameObject.AddOrGet<AudioSource>();
        
        if (collider is MeshCollider)
        {
            ((MeshCollider)collider).convex = true;
            rigidbody.centerOfMass = transform.worldToLocalMatrix.MultiplyPoint(collider.bounds.center);
        }
        rigidbody.drag = .2f;
        rigidbody.angularDrag = .5f;
        base.Init();
    }
    protected override void Start()
    {        
        spawnpos = transform.position;
        spawnrot = transform.rotation;
        if (shared)
            if (!Network.isServer)
                networkView.RPC("AddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());
        base.Start();
    }
    public int updateLightmapInterval = 100;
    protected virtual void Update()
    {
        tsendpackets -= Time.deltaTime;

        if (!_Game.bounds.collider.bounds.Contains(this.transform.position))
            _TimerA.AddMethod(2000, delegate { ResetSpawn(); });

        if (_TimerA.TimeElapsed(updateLightmapInterval))
            UpdateLightmap();
        if (_TimerA.TimeElapsed(100))
        {
            if (shared && Network.isServer)
                ControllerUpdate();
        }
    }
    Dictionary<Material, Color> defcolors = new Dictionary<Material, Color>();
    public void UpdateLightmap()
    {
        var materials = renderers.SelectMany(a => a.materials);

        var r = new Ray(pos + Vector3.up, Vector3.down);
        RaycastHit h;
        if (Physics.Raycast(r, out h, 10, 1 << LayerMask.NameToLayer("Level"))) 
        {
            var i = h.collider.gameObject.renderer.lightmapIndex;
            if (i != -1)
            {
                var t = LightmapSettings.lightmaps[i].lightmapFar;
                if (t != null)
                {
                    float a = t.GetPixelBilinear(h.lightmapCoord.x, h.lightmapCoord.y).a * 10 + .1f;
                    foreach (var m in materials)
                        if (m != null && !m.shader.name.ToLower().Contains("illu") && m.HasProperty("_Color"))
                        {
                            Color c;
                            if (!defcolors.TryGetValue(m, out c))
                            {
                                if (!m.name.Contains("DefColors"))
                                    m.name = "DefColors" + "-" + m.color.r + "-" + m.color.b + "-" + m.color.g + "-" + m.color.a + "-";
                                var cs = m.name.ToString().Split('-');
                                c = new Color(float.Parse(cs[1]), float.Parse(cs[2]), float.Parse(cs[3]), float.Parse(cs[4]));
                                defcolors.Add(m, c);
                            }                            
                            m.color = c * a;
                        }

                }
            }
        }
    }

    void ControllerUpdate()
    {
        
        float min = float.MaxValue;
        Destroible nearp = null;
        foreach (Player p in _Game.players)
            if (p != null)
            {
                if (p.Alive && p.OwnerID != -1)
                {
                    float dist = Vector3.Distance(p.transform.position, this.transform.position);
                    if (min > dist)
                        nearp = p;
                    min = Math.Min(dist, min);
                }
            }

        if (nearp != null && nearp.OwnerID != -1 && selected != nearp.OwnerID)
            RPCSetController(nearp.OwnerID);

    }
    public override void OnPlayerConnected1(NetworkPlayer np)
    {
        if (OwnerID != -1) RPCSetOwner(OwnerID);
        if (selected != -1) RPCSetController(selected);
        base.OnPlayerConnected1(np);
    }

    public void RPCSetOwner(int owner) { CallRPC("SetOwner", owner); }
    [RPC]
    public void SetOwner(int owner)
    {
        SetController(owner);
        foreach (Base bas in GetComponentsInChildren(typeof(Base)))
        {
            bas.OwnerID = owner;
            bas.OnSetOwner();
        }
    }

    public void RPCSetController(int owner) { CallRPC("SetController", owner); }
    [RPC]
    public void SetController(int owner)
    {
        this.selected = owner;
    }
    public void RPCResetOwner() { CallRPC("ResetOwner"); }
    [RPC]
    public void ResetOwner()
    {
        Debug.Log("_ResetOwner");
        this.selected = -1;
        foreach (Base bas in GetComponentsInChildren(typeof(Base)))
            bas.OwnerID = -1;

    }
    [RPC]
    public void AddNetworkView(NetworkViewID id)
    {
        var ss = networkView.stateSynchronization;
        NetworkView nw = this.gameObject.AddComponent<NetworkView>();
        nw.group = (int)GroupNetwork.Shared;
        nw.observed = this;
        nw.stateSynchronization = ss;
        nw.viewID = id;
        name += "+" + Regex.Match(nw.viewID.ToString(), @"\d+").Value;
    }

    public void RPCSetOwner()
    {
        RPCSetOwner(Network.player.GetHashCode());
    }
    public virtual void ResetSpawn()
    {
        transform.position = spawnpos;
        transform.rotation = spawnrot;
        rigidbody.angularVelocity = rigidbody.velocity = Vector3.zero;
    }
    
    protected virtual void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {        
        if (!enabled || !Sync) return;        
        if ((selected == -1 && Network.isServer) || selected == Network.player.GetHashCode() || stream.isReading || (Network.isServer && info.networkView.owner.GetHashCode() == selected))
        {
            if (stream.isReading || this.GetType() != typeof(Zombie) || tsendpackets < 0)
                lock ("ser")
                {
                    tsendpackets = .3f;
                    if (stream.isWriting)
                    {
                        syncPos = pos;
                        syncRot = rot;
                        syncVelocity = rigidbody.velocity;
                        syncAngularVelocity = rigidbody.angularVelocity;
                    }
                    if (posSync) stream.Serialize(ref syncPos);
                    if (velSync) stream.Serialize(ref syncVelocity);
                    if (rotSync) stream.Serialize(ref syncRot);
                    if (angSync) stream.Serialize(ref syncAngularVelocity);
                    if (stream.isReading)//&& syncPos != default(Vector3)
                    {
                        if (posSync) pos = syncPos;
                        if (velSync) rigidbody.velocity = syncVelocity;
                        if (rotSync) rot = syncRot;
                        if (angSync) rigidbody.angularVelocity = syncAngularVelocity;
                    }
                }
        }
    }
}
