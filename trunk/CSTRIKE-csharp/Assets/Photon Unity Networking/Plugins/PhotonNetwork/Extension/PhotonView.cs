// ----------------------------------------------------------------------------
// <copyright file="PhotonView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------
using UnityEngine;
using System.Collections;

public enum ViewSynchronization { Off, ReliableDeltaCompressed, Unreliable }
public enum OnSerializeTransform { OnlyPosition, OnlyRotation, OnlyScale, PositionAndRotation, All }
public enum OnSerializeRigidBody { OnlyVelocity, OnlyAngularVelocity, All }


[AddComponentMenu("Miscellaneous/Photon View")]
public class PhotonView : Photon.MonoBehaviour
{
    //Save scene ID in serializable INT (only changable via Editor)
    [SerializeField]
    private int sceneViewID = 0;

    [SerializeField]
    private PhotonViewID ID = new PhotonViewID(0, null);
    
    public Component observed;
    public ViewSynchronization synchronization;
    public int group = 0;
    public int prefix = -1;
    public object[] instantiationData = null;
    public Hashtable lastOnSerializeDataSent = null;
    public OnSerializeTransform onSerializeTransformOption = OnSerializeTransform.PositionAndRotation;
    public OnSerializeRigidBody onSerializeRigidBodyOption = OnSerializeRigidBody.All;
    private bool registeredPhotonView = false;

    public PhotonViewID viewID
    {
        get
        {
            if (ID.ID < 1 && sceneViewID > 0)
            {   //Load the correct scene ID
                ID = new PhotonViewID(sceneViewID, null);
            }
            return ID;
        }
        set
        {
            if (registeredPhotonView && PhotonNetwork.networkingPeer != null) PhotonNetwork.networkingPeer.RemovePhotonView(this);
            ID = value;
            if (PhotonNetwork.networkingPeer != null)
            {
                PhotonNetwork.networkingPeer.RegisterPhotonView(this);
                registeredPhotonView = true;
            }
        }
    }

    public override string ToString()
    {
        return string.Format("View {0} on {1} {2}", this.ID.ID, this.gameObject.name, (this.isSceneView)? "(scene)" : "");
    }

    public bool isSceneView
    {
        get
        {
            return sceneViewID > 0;
        }
    }

    public PhotonPlayer owner
    {
        get
        {
            return viewID.owner;
        }
    }

    public bool isMine
    {
        get
        {
            return (owner == PhotonNetwork.player) || (isSceneView && PhotonNetwork.isMasterClient);
        }
    }

#if UNITY_EDITOR

    public void SetSceneID(int newID)
    {
        sceneViewID = newID;
    }

    public int GetSceneID()
    {
        return sceneViewID;
    }

#endif

    //Start, since in Awake PhotonNetwork isn't set up properly.
    void Awake()
    {
        if (isSceneView)  
        {
            if (sceneViewID < 1) Debug.LogError("SceneView " + sceneViewID);
            ID = new PhotonViewID(sceneViewID, null);
            registeredPhotonView = true;
            PhotonNetwork.networkingPeer.RegisterPhotonView(this);
        }
        else
        {
            ID = new PhotonViewID(0, null);
        }
    }

    void OnDestroy()
    {
        PhotonNetwork.networkingPeer.RemovePhotonView(this);
    }

    public void RPC(string methodName, PhotonTargets target, params object[] parameters)
    {
        PhotonNetwork.RPC(this, methodName, target, parameters);
    }

    public void RPC(string methodName, PhotonPlayer targetPlayer, params object[] parameters)
    {
        PhotonNetwork.RPC(this, methodName, targetPlayer, parameters);
    }

    public static PhotonView Get(Component component)
    {
        return component.GetComponent<PhotonView>();
    }

    public static PhotonView Get(GameObject gameObj)
    {
        return gameObj.GetComponent<PhotonView>();
    }

    public static PhotonView Find(int viewID)
    {
        return PhotonNetwork.networkingPeer.GetPhotonView(viewID);
    }
}