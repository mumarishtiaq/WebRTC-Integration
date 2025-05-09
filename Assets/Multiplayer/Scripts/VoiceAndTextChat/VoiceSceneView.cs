using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VoiceSceneView : MonoBehaviour
{
    [Header("Voice GUI's")]
    [SerializeField] private VoiceStatusGUI _localVoiceStatus;
    [SerializeField] private VoiceStatusGUI _remoteVoiceStatus;

    [Space(10)]
    [Header("Coice Connection Status Colors")]
    [SerializeField] private Color _connectingColor;
    [SerializeField] private Color _connectedColor;
    [SerializeField] private Color _disConnectedColor;



    [Serializable]
    private class VoiceStatusGUI
    {
        public Image StatusIcon;
        public TextMeshProUGUI StatusTxt;
    }

    


    //private void Start()
    //{
    //    VivoxVoiceManager.OnVoiceStatusChanged += SetLocalPlayerVoiceStatus;
    //}


    public void SetParticipantVoiceStatus(ParticipantType type,VoiceStatus status)
    {
        var voiceGUI = type == ParticipantType.Local ? _localVoiceStatus : _remoteVoiceStatus;
        SetVoiceStatus(voiceGUI, status);
    }



    private void SetVoiceStatus(VoiceStatusGUI voiceGUI, VoiceStatus status)
    {
        string txt = "";
        Color color = Color.white;


        if(status == VoiceStatus.Connected)
        {
            txt = "Voice Connected";
            color = _connectedColor;
        } 
        
        if(status == VoiceStatus.Connecting)
        {
            txt = "Connecting Voice...";
            color = _connectingColor;
        }
        
        if(status == VoiceStatus.DisConnected)
        {
            txt = "Voice Disconnected";
            color = _disConnectedColor;
        }


        voiceGUI.StatusTxt.text = txt;
        voiceGUI.StatusIcon.color = color;
        voiceGUI.StatusTxt.color = color;

    }


    //private void OnDestroy()
    //{
    //    VivoxVoiceManager.OnVoiceStatusChanged -= SetLocalPlayerVoiceStatus;

    //}



}
