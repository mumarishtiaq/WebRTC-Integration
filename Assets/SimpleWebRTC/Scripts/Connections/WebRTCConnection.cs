using NativeWebSocket;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SimpleWebRTC {
    public class WebRTCConnection : MonoBehaviour {

        private const string webSocketTestMessage = "TEST!WEBSOCKET!TEST";
        private const string dataChannelTestMessage = "TEST!CHANNEL!TEST";

        public bool IsWebSocketConnected => webRTCManager.IsWebSocketConnected;
        public bool ConnectionToWebSocketInProgress => webRTCManager.IsWebSocketConnectionInProgress;

        public bool IsWebRTCActive { get; private set; }
        public bool IsVideoTransmissionActive { get; private set; }
        public bool IsAudioTransmissionActive { get; private set; }

        [Header("Connection Setup")]
        [SerializeField] private string WebSocketServerAddress = "wss://unity-webrtc-signaling.glitch.me";
        [SerializeField] private string StunServerAddress = "stun:stun.l.google.com:19302";
        [SerializeField] private string LocalPeerId = "PeerId";
        [SerializeField] private bool UseHTTPHeader = true;
        [SerializeField] private bool ShowLogs = true;
        [SerializeField] private bool ShowDataChannelLogs = true;

        [Header("WebSocket Connection")]
        [SerializeField] private bool WebSocketConnectionActive;
        [SerializeField] private bool SendWebSocketTestMessage = false;
        public UnityEvent<WebSocketState> WebSocketConnectionChanged;

        [Header("WebRTC Connection")]
        [SerializeField] private bool WebRTCConnectionActive = false;
        public UnityEvent WebRTCConnected;

        [Header("Data Transmission")]
        [SerializeField] private bool SendDataChannelTestMessage = false;
        public UnityEvent<string> DataChannelConnected;
        public UnityEvent<string> DataChannelMessageReceived;

        [Header("Video Transmission")]
        [SerializeField] private bool StartStopVideoTransmission = false;
        [SerializeField] private Vector2Int VideoResolution = new Vector2Int(1280, 720);
        [SerializeField] private Camera StreamingCamera;
        public RawImage OptionalPreviewRawImage;
        public RectTransform ReceivingRawImagesParent;
        public UnityEvent VideoTransmissionReceived;

        [Header("Audio Transmission")]
        [SerializeField] private bool StartStopAudioTransmission = false;
        [SerializeField] private StreamingMicInput streamingMicInput;
        public Transform ReceivingAudioSourceParent;
        public UnityEvent AudioTransmissionReceived;

        private WebRTCManager webRTCManager;

        //these two actions will use to notify the UI
        [HideInInspector]
        public Action OnConnectRequested; 
        //[HideInInspector]
        //public Action OnConnected; 
        [HideInInspector]
        public Action OnWebSocketOpened; 
        
        [HideInInspector]
        public Action OnDisconnected; 

        private void Awake() {
            SimpleWebRTCLogger.EnableLogging = ShowLogs;
            SimpleWebRTCLogger.EnableDataChannelLogging = ShowDataChannelLogs;
            //Application.runInBackground = true;

            LocalPeerId = GenerateCode();

            webRTCManager = new WebRTCManager(LocalPeerId, StunServerAddress, this);

            // register events for webrtc connection
            webRTCManager.OnWebSocketConnection += WebSocketConnectionChanged.Invoke;
            webRTCManager.OnWebRTCConnection += WebRTCConnected.Invoke;
            webRTCManager.OnDataChannelConnection += DataChannelConnected.Invoke;
            webRTCManager.OnDataChannelMessageReceived += DataChannelMessageReceived.Invoke;
            webRTCManager.OnVideoStreamEstablished += VideoTransmissionReceived.Invoke;
            webRTCManager.OnAudioStreamEstablished += AudioTransmissionReceived.Invoke;
        }

        private void Update() {

#if !UNITY_WEBGL || UNITY_EDITOR
            webRTCManager.DispatchMessageQueue();
#endif

            if (SimpleWebRTCLogger.EnableLogging != ShowLogs) {
                SimpleWebRTCLogger.EnableLogging = ShowLogs;
            }

            ConnectClient();

            if (!WebSocketConnectionActive && IsWebSocketConnected) {
                DisconnectClient();
            }

            if (!IsWebSocketConnected) {
                return;
            }

            if (SendWebSocketTestMessage) {
                SendWebSocketTestMessage = !SendWebSocketTestMessage;
                webRTCManager.SendWebSocketMessage($"{webSocketTestMessage} from {LocalPeerId}");
            }

            if (WebRTCConnectionActive && !IsWebRTCActive) {
                IsWebRTCActive = !IsWebRTCActive;
                webRTCManager.InstantiateWebRTC();
            }

            if (!WebRTCConnectionActive && IsWebRTCActive) {
                IsWebRTCActive = !IsWebRTCActive;
                webRTCManager.CloseWebRTC();
            }

            if (SendDataChannelTestMessage) {
                SendDataChannelTestMessage = !SendDataChannelTestMessage;
                SendDataChannelMessage($"{dataChannelTestMessage} from {LocalPeerId}");
            }

            if (StartStopVideoTransmission && !IsVideoTransmissionActive) {
                IsVideoTransmissionActive = !IsVideoTransmissionActive;
                StreamingCamera.gameObject.SetActive(IsVideoTransmissionActive);
                webRTCManager.AddVideoTrack(StreamingCamera, VideoResolution.x, VideoResolution.y);
            }

            if (!StartStopVideoTransmission && IsVideoTransmissionActive) {
                IsVideoTransmissionActive = !IsVideoTransmissionActive;
                StreamingCamera.gameObject.SetActive(IsVideoTransmissionActive);
                webRTCManager.RemoveVideoTrack();
            }


            //if (StartStopAudioTransmission && !IsAudioTransmissionActive) {
            //    IsAudioTransmissionActive = !IsAudioTransmissionActive;
            //    streamingMicInput.gameObject.SetActive(IsAudioTransmissionActive);
            //    streamingMicInput.StartMicrophone();
            //    Debug.LogError("In Update audio testing");
            //    webRTCManager.AddAudioTrack(streamingMicInput);
            //}

            //if (!StartStopAudioTransmission && IsAudioTransmissionActive) {
            //    IsAudioTransmissionActive = !IsAudioTransmissionActive;
            //    //streamingMicInput.Stop();
            //    streamingMicInput.gameObject.SetActive(IsAudioTransmissionActive);
            //    webRTCManager.RemoveAudioTrack();
            //    Debug.LogError("in stop condition");
            //}
        }

        private void OnEnable() {
            ConnectClient();
        }

        private void OnDisable() {
            DisconnectClient();
        }

        private void OnDestroy() {
            DisconnectClient();

            // de-register events for connection
            webRTCManager.OnWebSocketConnection -= WebSocketConnectionChanged.Invoke;
            webRTCManager.OnWebRTCConnection -= WebRTCConnected.Invoke;
            webRTCManager.OnDataChannelConnection += DataChannelConnected.Invoke;
            webRTCManager.OnDataChannelMessageReceived -= DataChannelMessageReceived.Invoke;
            webRTCManager.OnVideoStreamEstablished -= VideoTransmissionReceived.Invoke;
            webRTCManager.OnAudioStreamEstablished -= AudioTransmissionReceived.Invoke;
        }

        private void ConnectClient() {
            if (WebSocketConnectionActive && !ConnectionToWebSocketInProgress && !IsWebSocketConnected) {
                webRTCManager.Connect(WebSocketServerAddress, UseHTTPHeader);
            }
        }

        private void DisconnectClient() {
            // stop websocket
            WebSocketConnectionActive = false;

            // stop webRTC
            IsWebRTCActive = false;
            WebRTCConnectionActive = false;

            // stop video
            StartStopVideoTransmission = false;
            IsVideoTransmissionActive = false;
            if (OptionalPreviewRawImage != null) {
                OptionalPreviewRawImage.texture = null;
            }
            StreamingCamera.gameObject.SetActive(IsVideoTransmissionActive);
            webRTCManager.RemoveVideoTrack();

            // stop audio
            StartStopAudioTransmission = false;
            IsAudioTransmissionActive = false;
            //streamingMicInput.Stop();
            streamingMicInput.StopMicrophone();
            streamingMicInput.gameObject.SetActive(IsAudioTransmissionActive);
            webRTCManager.RemoveAudioTrack();

            webRTCManager.CloseWebRTC();
            webRTCManager.CloseWebSocket();

            StreamingCamera.gameObject.SetActive(false);
            //streamingMicInput.Stop();
            streamingMicInput.gameObject.SetActive(false);
            OnDisconnected?.Invoke();
        }

        public void SetUniquePlayerName(string playerName) {
            LocalPeerId = playerName;
        }


        public void ConnectBtn()
        {
            Connect();
            OnConnectRequested?.Invoke();
        }

        public void Connect() {
            WebSocketConnectionActive = true;
        }

        public void ConnectWebRTC() {
            WebRTCConnectionActive = true;
            //OnConnected?.Invoke();
        }

        public void Disconnect() {
            WebSocketConnectionActive = false;
        }

        public void SendDataChannelMessage(string message) {
            if (!webRTCManager.IsWebSocketConnected) {
                SimpleWebRTCLogger.LogError($"WebSocket not connected on {gameObject.name}");
                return;
            }
            webRTCManager.SendViaDataChannel(message);
        }

        public void SendDataChannelMessageToPeer(string targetPeerId, string message) {
            if (!webRTCManager.IsWebSocketConnected) {
                SimpleWebRTCLogger.LogError($"WebSocket not connected on {gameObject.name}");
                return;
            }
            webRTCManager.SendViaDataChannel(targetPeerId, message);
        }

        #region Video
        public void StartVideoTransmission() {
            if (IsVideoTransmissionActive) {
                // for restarting without stopping
                webRTCManager.RemoveVideoTrack();
                webRTCManager.AddVideoTrack(StreamingCamera, VideoResolution.x, VideoResolution.y);
                Debug.Log("Video Started");
            }
            StartStopVideoTransmission = true;
        }

        public void StopVideoTransmission() {
            StartStopVideoTransmission = false;
        }

        #endregion Video

        #region Audio
        public void StartAudioTransmission() {

            Debug.LogError($"IsAudioTransmissionActive :  {IsAudioTransmissionActive}");
           // if (IsAudioTransmissionActive)
            //{
            //    // for restarting without stopping

            //    //StreamingAudioSource.Play(); //added by MUI
            //    webRTCManager.RemoveAudioTrack();
            //    webRTCManager.AddAudioTrack(streamingMicInput);
            //Debug.LogError("In method StartAudioTransmission() if condition");
            //}
            StartStopAudioTransmission = true;


            
                //IsAudioTransmissionActive = !IsAudioTransmissionActive;
                streamingMicInput.gameObject.SetActive(true);
                webRTCManager.RemoveAudioTrack();
                streamingMicInput.StartMicrophone();
                webRTCManager.AddAudioTrack(streamingMicInput);
            
        }

        public void StopAudioTransmission() {
            StartStopAudioTransmission = false;


            //IsAudioTransmissionActive = !IsAudioTransmissionActive;
            streamingMicInput.audioSource.Stop();
            streamingMicInput.gameObject.SetActive(false);
                webRTCManager.RemoveAudioTrack();
                Debug.LogError("in stop condition");
            
        }

        [ContextMenu("Check For Audio Track")]
        public void  testc()
        {
            Debug.LogError($"AudioTrackExist = {webRTCManager.CheckAudioTrackExist()}");
        } 
        
        [ContextMenu("Check For AudioIsPlaying")]
        public void  TestAudioIsPlaying()
        {
            Debug.LogError($"AudioIsPlaying = {streamingMicInput.audioSource.isPlaying}");
        }

        #endregion Audio

        //Testing buttons

        public void WebSocketConnectionActiveButton()
        {
            WebSocketConnectionActive = true;
            Debug.LogError("Web Socket Conncections is activated");
        }
        
        public void StartVideoTransmissionButton()
        {
            StartStopVideoTransmission = true;
            Debug.LogError("Start Video Transmission is activated");
        }
        
        public void StartAudioTransmissionButton()
        {
            StartStopAudioTransmission = true;
            Debug.LogError("Start Audio Transmission is activated");
        }


        static System.Random random = new System.Random();
        private string GenerateCode()
        {
            // Generate 5 random uppercase letters
            string letters = new string(Enumerable.Range(0, 5)
                .Select(_ => (char)random.Next('A', 'Z' + 1)).ToArray());

            // Generate 3 random digits
            string numbers = new string(Enumerable.Range(0, 3)
                .Select(_ => (char)random.Next('0', '9' + 1)).ToArray());

            return $"{letters}_{numbers}";
        }

        

    }
}