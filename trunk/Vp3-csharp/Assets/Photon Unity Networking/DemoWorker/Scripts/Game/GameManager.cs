using UnityEngine;

public class GameManager : Photon.MonoBehaviour {

    public Transform playerPrefab;
    
    void Awake()
    {   
        //PhotonNetwork.logLevel = NetworkLogLevel.Full;
        if (!PhotonNetwork.connected)
        {
            //We must be connected to a photon server! Back to main menu
            Application.LoadLevel(Application.loadedLevel - 1);
            return;
        }
        PhotonNetwork.isMessageQueueRunning = true;
       
        //Spawn our local player
        PhotonNetwork.Instantiate(playerPrefab, transform.position, Quaternion.identity, 0);
    }

    void OnGUI()
    {
        if (GUILayout.Button("Leave& QUIT"))
        {
            PhotonNetwork.LeaveRoom();
            Application.LoadLevel(Application.loadedLevelName);
        }
    }


    void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerConnected: " + player);
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.LogWarning("OnPlayerDisconneced: " + player);
    }

    void OnLeftRoom()
    {
        Debug.LogWarning("OnLeftRoom (local)");
    }
    void OnReceivedRoomList()
    {
        Debug.LogWarning("OnReceivedRoomList");
    }
    void OnReceivedRoomListUpdate()
    {
        Debug.LogWarning("OnReceivedRoomListUpdate");
    }
    void OnMasterClientSwitched(PhotonPlayer player)
    {
        Debug.LogWarning("OnMasterClientSwitched: " + player);
        if (PhotonNetwork.connected)
        {
            photonView.RPC("SendChatMessage", PhotonNetwork.masterClient, "Hi master! From:" + PhotonNetwork.player);
            photonView.RPC("SendChatMessage", PhotonTargets.All, "WE GOT A NEW MASTER: " + player + "==" + PhotonNetwork.masterClient + " From:" + PhotonNetwork.player);
        }
    }

    void OnConnectedToPhoton()
    {
        Debug.LogWarning("OnConnectedToPhoton");
    }
    void OnDisconnectedFromPhoton()
    {
        Debug.LogWarning("OnDisconnectedFromPhoton");
        //Back to main menu        
        Application.LoadLevel(Application.loadedLevelName);
    }
    void OnFailedToConnectToPhoton()
    {
        Debug.LogWarning("OnFailedToConnectToPhoton");
    }
    void OnPhotonInstantiate()
    {
        Debug.LogWarning("OnPhotonInstantiate");
       
    }


}
