using UnityEngine;
using System.Collections;
public class NetworkInterpolatedTransform : MonoBehaviour
{
    public double interpolationBackTime = 0.1;
    internal struct State
    {
        internal double timestamp;
        internal Vector3 pos;
        internal Quaternion rot;
    }
    State[] m_BufferedState = new State[20];
    int m_TimestampCount;
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            Vector3 pos = transform.localPosition;
            Quaternion rot = transform.localRotation;
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
        }
        else
        {
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
            for (int i = m_BufferedState.Length - 1; i >= 1; i--)
            {
                m_BufferedState[i] = m_BufferedState[i - 1];
            }
            State state;
            state.timestamp = info.timestamp;
            state.pos = pos;
            state.rot = rot;
            m_BufferedState[0] = state;
            m_TimestampCount = Mathf.Min(m_TimestampCount + 1, m_BufferedState.Length);
            for (int i = 0; i < m_TimestampCount - 1; i++)
            {
                if (m_BufferedState[i].timestamp < m_BufferedState[i + 1].timestamp)
                    Debug.Log("State inconsistent");
            }
        }
    }
    void Update()
    {
        double currentTime = Network.time;
        double interpolationTime = currentTime - interpolationBackTime;
        if (m_BufferedState[0].timestamp > interpolationTime)
        {
            for (int i = 0; i < m_TimestampCount; i++)
            {
                if (m_BufferedState[i].timestamp <= interpolationTime || i == m_TimestampCount - 1)
                {
                    State rhs = m_BufferedState[Mathf.Max(i - 1, 0)];
                    State lhs = m_BufferedState[i];
                    double length = rhs.timestamp - lhs.timestamp;
                    float t = 0.0F;
                    if (length > 0.0001)
                        t = (float)((interpolationTime - lhs.timestamp) / length);
                    transform.localPosition = Vector3.Lerp(lhs.pos, rhs.pos, t);
                    transform.localRotation = Quaternion.Slerp(lhs.rot, rhs.rot, t);
                    return;
                }
            }
        }
        else
        {
            State latest = m_BufferedState[0];
            transform.localPosition = latest.pos;
            transform.localRotation = latest.rot;
        }
    }
}
