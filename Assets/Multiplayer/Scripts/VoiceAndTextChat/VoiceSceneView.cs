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

    [Header("AudioToggle Sprites")]
    [SerializeField] private Sprite _audioEnabledSprite;
    [SerializeField] private Sprite _audioDisabledSprite;

    [Header("AudioToggle Image")]
    [SerializeField] private Image _audioToggleImg;



    [Serializable]
    private class VoiceStatusGUI
    {
        public Image StatusIcon;
        public TextMeshProUGUI StatusTxt;
    }




    private void Start()
    {
        AudioToggleActiveState(false);
    }


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

    public void OnToggleSpriteSwap(bool isMuted) 
    {
        var sprite = isMuted ? _audioDisabledSprite : _audioEnabledSprite;
        _audioToggleImg.sprite = sprite;

    }

    public void AudioToggleActiveState(bool state)
    {
        _audioToggleImg.transform.parent.gameObject.SetActive(state);
    }


    //private void OnDestroy()
    //{
    //    VivoxVoiceManager.OnVoiceStatusChanged -= SetLocalPlayerVoiceStatus;

    //}



}
