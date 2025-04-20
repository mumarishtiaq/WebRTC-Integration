using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleWebRTC;


public class CallingUIHandler : MonoBehaviour
{
    [Header("Sprites")]

    [SerializeField] private Sprite _audioEnabled;
    [SerializeField] private Sprite _audioDisabled;
    [SerializeField] private Sprite _videoEnabled;
    [SerializeField] private Sprite _videoDisabled;


    [Header("Toggles Component")]
    [SerializeField] private Toggle _audioToggle;
    //[SerializeField] private Toggle _videoToggle;

    [Header("WebRTCConnection ")]
    [SerializeField] private WebRTCConnection _webRtcConnection;

    [Header("Popups ")]
    [SerializeField] private GameObject _connectedPopup;
    [SerializeField] private GameObject _disconnectedPopup;

    [SerializeField] private GameObject _connectingPopup;
    
    [Header("Background ")]
    [SerializeField] private GameObject _loadingBackground;
    [Header("Testing")]
    [SerializeField] private GameObject _webSocketConnectionOpenedPopup;


    [Header("Buttons")]
    [SerializeField] private GameObject _connectBtn;
    [SerializeField] private GameObject _reConnectBtn;
    [SerializeField] private GameObject _disConnectBtn;
    
   



    private void Awake()
    {

        _webRtcConnection = FindObjectOfType<WebRTCConnection>(true);

        if (_audioToggle)
        {
            _audioToggle.onValueChanged.RemoveAllListeners();
            _audioToggle.onValueChanged.AddListener(OnAudioToggled);
        }

        //if (_videoToggle)
        //{
        //    _videoToggle.onValueChanged.RemoveAllListeners();
        //    _videoToggle.onValueChanged.AddListener(OnVideoToggled);
        //}
        //    _audioToggle.isOn = false;
        //    _videoToggle.isOn = false;

        _connectingPopup.SetActive(false);
        _connectedPopup.SetActive(false);
        _disconnectedPopup.SetActive(false);
        _webSocketConnectionOpenedPopup.SetActive(false);
        _audioToggle.gameObject.SetActive(false);
        //_videoToggle.gameObject.SetActive(false);

        //enabling connect and disable re-connect and disConnect
        _connectBtn.SetActive(true);
        _disConnectBtn.SetActive(false);
        _reConnectBtn.SetActive(false);

        _loadingBackground.SetActive(true);

    }
    private void OnEnable()
    {
        _webRtcConnection.OnConnectRequested += OnConnectionRequested;
        //_webRtcConnection.OnConnected += OnConnected;
        _webRtcConnection.OnWebSocketOpened += OnWebSocketOpened;
        _webRtcConnection.WebRTCConnected.AddListener(OnConnected);
        _webRtcConnection.OnDisconnected += OnDisconnected;
        //_webRtcConnection.AudioTransmissionReceived.AddListener(() => _audioToggle.isOn = true);


    }
    private void OnDisable()
    {
        _webRtcConnection.OnConnectRequested -= OnConnectionRequested;
        //_webRtcConnection.OnConnected -= OnConnected;
        _webRtcConnection.OnWebSocketOpened -= OnWebSocketOpened;
        _webRtcConnection.OnDisconnected -= OnDisconnected;
        _webRtcConnection.WebRTCConnected.RemoveListener(OnConnected);
    }

    private void OnWebSocketOpened()
    {
        _webSocketConnectionOpenedPopup?.SetActive(true);
        StartCoroutine(DelayPopupOff());
    }
    private IEnumerator DelayPopupOff()
    {
        yield return new WaitForSeconds(2);
        _webSocketConnectionOpenedPopup.SetActive(false);
        

    }


    private void OnAudioToggled(bool isOn)
    {
        if (isOn)
        {
            _audioToggle.transform.GetChild(0).GetComponent<Image>().sprite = _audioEnabled;
            _webRtcConnection.StartAudioTransmission();
        }
        else
        {
            _audioToggle.transform.GetChild(0).GetComponent<Image>().sprite = _audioDisabled;
            _webRtcConnection.StopAudioTransmission();
        }
    }

    public void OnVideoToggled(bool isOn)
    {
        //if (isOn)
        //{
        //    //_videoToggle.transform.GetChild(0).GetComponent<Image>().sprite = _videoEnabled;
        //    _webRtcConnection.StartVideoTransmission();
        //}
        //else
        //{
        //    //_videoToggle.transform.GetChild(0).GetComponent<Image>().sprite = _videoDisabled;
        //    _webRtcConnection.StopVideoTransmission();
        //}
    }

    private void OnConnected()
    {
        Debug.LogError("cONNECTED");
        _connectingPopup.SetActive(false);
        _connectedPopup.SetActive(true);
        _disconnectedPopup.SetActive(false);

        _audioToggle.gameObject.SetActive(true);
        //_videoToggle.gameObject.SetActive(true);
        _loadingBackground.SetActive(false);




        StartCoroutine(OperationOnConnected());
    }

    private IEnumerator OperationOnConnected()
    {
        yield return new WaitForSeconds(0.5f);
        _connectedPopup.SetActive(false);
        //yield return new WaitForSeconds(0.3f);
        //_videoToggle.isOn = true;
        //yield return new WaitForSeconds(0.3f);
        //_audioToggle.isOn = true;

    }

    private void OnConnectionRequested()
    {
        _connectingPopup.SetActive(true);

        _disconnectedPopup.SetActive(false);

        _connectedPopup.SetActive(false);

        //enabling disconnect and disable connect and reConnect
        _disConnectBtn.SetActive(true);
        _connectBtn.SetActive(false);
        _reConnectBtn.SetActive(false);
        _loadingBackground.SetActive(true);


        Debug.LogError("Connection Requested");
    }

    private void OnDisconnected()
    {
        _connectBtn.SetActive(false);
        _reConnectBtn.SetActive(true);
        _disConnectBtn.SetActive(false);
        _disconnectedPopup.SetActive(true);

        _connectedPopup.SetActive(false);
        _connectingPopup.SetActive(false);

        _audioToggle.gameObject.SetActive(false);
        _audioToggle.isOn = false;
        _loadingBackground.SetActive(true);

    }

}
