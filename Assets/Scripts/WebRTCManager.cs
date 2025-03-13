using UnityEngine;
using Unity.WebRTC;  // Import WebRTC package

public class WebRTCManager: MonoBehaviour
{
    private RTCPeerConnection peerConnection;
    private RTCDataChannel dataChannel;

    void Start()
    {
        //WebRTC.Initialize();

        CreateConnection();

    }

    void CreateConnection()
    {
        //RTCConfiguration config = new RTCConfiguration
        //{
        //    iceServers = new RTCIceServer[]
        //    {
        //        new RTCIceServer { urls = new string[] { "stun:stun.l.google.com:19302" } }
        //    }
        //};

        //peerConnection = new RTCPeerConnection(config);
        //peerConnection.OnIceCandidate = candidate => Debug.Log("ICE Candidate: " + candidate.Candidate);

        //dataChannel = peerConnection.CreateDataChannel("audioChannel");
        //dataChannel.OnOpen = () => Debug.Log("Data channel opened!");
    }
}
