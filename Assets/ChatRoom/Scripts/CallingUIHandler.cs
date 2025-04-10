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
    [SerializeField] private Toggle _videoToggle;

    [Header("WebRTCConnection ")]
    [SerializeField] private WebRTCConnection _webRtcConnection;


    private void Awake()
    {

        _webRtcConnection = FindObjectOfType<WebRTCConnection>(true);

        if (_audioToggle)
        {
            _audioToggle.onValueChanged.RemoveAllListeners();
            _audioToggle.onValueChanged.AddListener(OnAudioToggled);
        }

        if (_videoToggle)
        {
            _videoToggle.onValueChanged.RemoveAllListeners();
            _videoToggle.onValueChanged.AddListener(OnVideoToggled);
        }
            _audioToggle.isOn = false;
            _videoToggle.isOn = false;

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
        if (isOn)
        {
            _videoToggle.transform.GetChild(0).GetComponent<Image>().sprite = _videoEnabled;
            _webRtcConnection.StartVideoTransmission();
        }
        else
        {
            _videoToggle.transform.GetChild(0).GetComponent<Image>().sprite = _videoDisabled;
            _webRtcConnection.StopVideoTransmission();
        }
    }


}
