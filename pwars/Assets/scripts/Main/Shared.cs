using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using doru;
using System.Text.RegularExpressions;
using System.Linq;

public class Shared : bs
{
    public bool isController { get { return selected == Network.player.GetHashCode(); } }
    internal Vector3 syncPos;
    internal Quaternion syncRot;
    internal Vector3 syncVelocity;
    internal Vector3 syncAngularVelocity;
    internal Vector3 spawnpos;
    internal Quaternion spawnrot;
    public bool velSync = true, posSync = true, rotSync = true, angSync = true, Sync = true;
    internal int selected = -1;
    internal float[] tmsend = new float[maxConId];
    internal Renderer[] renderers;
    public AnimationCurve SendPackets;
    public bool shared = true;
    [FindAsset("collision1")]
    public AudioClip soundcollision;
    public override void Awake()
    {
        renderers = this.GetComponentsInChildren<Renderer>().Where(a => a.GetComponent<TextMesh>() == null).Distinct().ToArray();
        base.Awake();
    }
    public override void Init()
    {
        foreach (Transform t in transform.GetTransforms())
        {
            t.gameObject.isStatic = false;
            t.gameObject.layer = LayerMask.NameToLayer("Default");
        }
                
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
    public virtual void Start()
    {
        spawnpos = transform.position;
        spawnrot = transform.rotation;
        if (shared)
            networkView.RPC("AddNetworkView", RPCMode.AllBuffered, Network.AllocateViewID());
        
    }
    public int updateLightmapInterval = 100;
    protected virtual void Update()
    {
        for (int i = 0; i < tmsend.Length; i++)
            tmsend[i] += Time.deltaTime;

        if (!_Game.bounds.collider.bounds.Contains(this.transform.position) && isController)
        {               
            ResetSpawn();
        }

        if (_TimerA.TimeElapsed(updateLightmapInterval))
            UpdateLightmap();
        if (_TimerA.TimeElapsed(200))
        {
            if (shared && Network.isServer)
                ControllerUpdate();
        }
    }
    Dictionary<Material, Color[]> defcolors = new Dictionary<Material, Color[]>();
    public static string[] supMats = new string[] { "Bumped Specular", "Specular", "Parallax Specular", "Diffuse", "Diffuse Fast" };
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
                    Color ac = t.GetPixelBilinear(h.lightmapCoord.x, h.lightmapCoord.y);
                    float a = ac.a * 10 + .1f;
                    foreach (var m in materials)
                    {
                        if (m != null && (supMats.Contains(m.shader.name)))
                        {
                            Color[] c;
                            bool spec = m.HasProperty("_SpecColor");
                            if (!defcolors.TryGetValue(m, out c))
                            {
                                c = new Color[2];
                                Color sc = new Color();
                                if(spec)    
                                    sc = m.GetColor("_SpecColor");
                                if (!m.name.Contains("DefColors"))
                                {
                                    m.name = "DefColors" + "-" + m.color.r + "-" + m.color.b + "-" + m.color.g + "-" + m.color.a + "-";
                                    m.name += sc.r + "-" + sc.b + "-" + sc.g + "-" + sc.a + "-";
                                }
                                string[] cs = m.name.ToString().Split('-');
                                c[0] = new Color(float.Parse(cs[1]), float.Parse(cs[3]), float.Parse(cs[2]), float.Parse(cs[4]));
                                if (spec)
                                    c[1] = new Color(float.Parse(cs[5]), float.Parse(cs[6]), float.Parse(cs[7]), float.Parse(cs[8]));
                                defcolors.Add(m, c);
                            }
                            m.color = c[0] * ac * a;
                            if (c[1] != default(Color))
                                if (m.HasProperty("_SpecColor"))
                                    m.SetColor("_SpecColor", c[1] * ac * a);
                        }
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
    public override void OnPlayerConnectedBase(NetworkPlayer np)
    {
        if (OwnerID != -1) RPCSetOwner(OwnerID);
        if (selected != -1) RPCSetController(selected);
        base.OnPlayerConnectedBase(np);
    }
    public void RPCSetOwner(int owner) { CallRPC("SetOwner", owner); }
    [RPC]
    public void SetOwner(int owner)
    {
        SetController(owner);
        foreach (bs bas in GetComponentsInChildren(typeof(bs)))
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
        this.selected = -1;
        foreach (bs bas in GetComponentsInChildren(typeof(bs)))
            bas.OwnerID = -1;

    }
    public bool unReliable;
    [RPC]
    public void AddNetworkView(NetworkViewID id)
    {
        NetworkView nw = this.gameObject.AddComponent<NetworkView>();
        nw.group = (int)GroupNetwork.Shared;
        nw.observed = this;
        nw.stateSynchronization = unReliable ? NetworkStateSynchronization.Unreliable : NetworkStateSynchronization.ReliableDeltaCompressed;
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
    public bool Interval(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isReading) return true;
        if (SendPackets.length == 0) return true;
        int o = info.networkView.owner.GetHashCode();
        if (Network.isServer && Network.connections.Length > 1)
            return tmsend[o] > SendPackets.Evaluate(Vector3.Distance(players[o].pos, this.pos));
        else
            return tmsend[o] > SendPackets.Evaluate(0);
    }
    protected virtual void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (!enabled || !Sync) return;        
        if ((selected == -1 && Network.isServer) || selected == Network.player.GetHashCode() || stream.isReading || (Network.isServer && info.networkView.owner.GetHashCode() == selected))
        {            
            if (Interval(stream,info))
            {
                if (stream.isWriting)
                {
                    tmsend[info.networkView.owner.GetHashCode()] = 0;
                    syncPos = pos;
                    syncRot = rot;
                    syncVelocity = rigidbody.velocity;
                    syncAngularVelocity = rigidbody.angularVelocity;
                }
                if (posSync)
                    stream.Serialize(ref syncPos);
                if (velSync)
                    stream.Serialize(ref syncVelocity);
                if (rotSync)
                    stream.Serialize(ref syncRot);
                if (angSync)
                    stream.Serialize(ref syncAngularVelocity);
                if (stream.isReading)
                {
                    if (syncPos != default(Vector3))
                    {
                        if (posSync)
                            pos = syncPos;
                        if (velSync)
                            rigidbody.velocity = syncVelocity;
                        if (rotSync)
                            rot = syncRot;
                        if (angSync)
                            rigidbody.angularVelocity = syncAngularVelocity;
                    }
                }
            }
        }
    }
}
