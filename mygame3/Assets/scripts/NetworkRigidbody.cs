using UnityEngine;
using System.Collections;
[AddComponentMenu("_NetworkRigidbody")]
public class NetworkRigidbody : MonoBehaviour
{
    
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 velocity;
    public Vector3 angularVelocity;
    public bool _enbld;
    public static bool _lock;    
    public NetworkPlayer? selected;
    public int id = -3;
    public int sender;
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {        
        if (selected.HasValue)
            id = selected.GetHashCode();
        else
            id = -2;

        _enbld = (selected == Network.player);
        
        if (selected == Network.player || stream.isReading || (Network.isServer && info.networkView.owner == selected ) )
        {
            
            lock ("ser")
            {
                if (stream.isWriting)
                {
                    sender = info.sender.GetHashCode(); 
                    pos = rigidbody.position;
                    rot = rigidbody.rotation;
                    velocity = rigidbody.velocity;
                    angularVelocity = rigidbody.angularVelocity;
                }                
                stream.Serialize(ref pos);
                stream.Serialize(ref rot);
                stream.Serialize(ref velocity);
                stream.Serialize(ref angularVelocity);

                if (stream.isReading && pos!= default(Vector3))
                {
                    rigidbody.position = pos;
                    rigidbody.velocity = velocity;
                    rigidbody.rotation = rot;
                    rigidbody.angularVelocity = angularVelocity;
                }
            }
        }
    }
}
