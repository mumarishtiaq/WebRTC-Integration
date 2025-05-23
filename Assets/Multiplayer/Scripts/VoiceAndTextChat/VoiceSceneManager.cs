using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using Unity.Services.Vivox.AudioTaps;
using UnityEngine;
using UnityEngine.Android;

public class VoiceSceneManager : MonoBehaviour
{
    [SerializeField] private VoiceSceneView _sceneView;

    //[SerializeField] private VivoxParticipant _localParticipant;

    [SerializeField] private GameObject  _partipantAudioTap;

    PeerData _peerData => MultiplayerManager.Instance.PeerData;

    private bool isChannelAlreadyJoined = false;

    private async void Awake()
    {
        if (VivoxVoiceManager.Instance.isInitializeAndLoggedIn)
        {
            //channel not joined
            if (!VivoxService.Instance.ActiveChannels.ContainsKey(_peerData.CommonRoomName))
            {
                await VivoxVoiceManager.Instance.JoinVoiceChannel(_peerData.CommonRoomName);
            }
        }
    }
   
    private void Start()
    {
        //commented in test

        if(VivoxVoiceManager.Instance.isInitializeAndLoggedIn)
        {
            VivoxService.Instance.ChannelJoined += OnChannelJoined;
            VivoxService.Instance.ConnectionRecovered += OnConnectionRecovered;
            VivoxService.Instance.ConnectionRecovering += OnConnectionRecovering;
            VivoxService.Instance.ConnectionFailedToRecover += OnConnectionFailedToRecover;
            VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
            VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;

            VivoxVoiceManager.OnConnecting = () => SetConnecting();

            if (VivoxService.Instance.ActiveChannels.TryGetValue(_peerData.CommonRoomName, out var participants) && participants.Count == 2)
            {
                SetConnected(ParticipantType.Local);
                SetConnected(ParticipantType.Remote);
                _sceneView.AudioToggleActiveState(true);
                _sceneView.OnToggleSpriteSwap(VivoxVoiceManager.LocalParticipant.IsMuted);
            }

            else
            {
                SetConnecting(ParticipantType.Local);
                SetConnecting(ParticipantType.Remote);
                _sceneView.AudioToggleActiveState(false);
            }

        }
        else
        {
            SetDisConnected(ParticipantType.Local);
            SetDisConnected(ParticipantType.Remote);
            _sceneView.AudioToggleActiveState(false);
        }
    }

    private void OnDestroy()
    {
        VivoxService.Instance.ChannelJoined -= OnChannelJoined;

        VivoxService.Instance.ConnectionRecovered -= OnConnectionRecovered;
        VivoxService.Instance.ConnectionRecovering -= OnConnectionRecovering;
        VivoxService.Instance.ConnectionFailedToRecover -= OnConnectionFailedToRecover;
        VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemoved;
    }



    private void OnChannelJoined(string obj)
    {
        Debug.Log($"Joined Channel Mame : {obj}");
        SetConnected();

    }

    private void OnConnectionRecovered()
    {
        Debug.Log($"Connection Recovered");
        SetConnected();

    }

    private void OnConnectionFailedToRecover()
    {
        Debug.LogWarning($"Connection failed to recover");
        SetDisConnected();
    }

    private void OnConnectionRecovering()
    {
        Debug.LogWarning($"Connection Recovering");
        SetConnecting();
    }


    private void OnParticipantAdded(VivoxParticipant participant)
    {

        //make sure it is always a remote player
        if (!participant.IsSelf)
        {
            Debug.Log($"Participant added : ID : {participant.PlayerId}, {participant.DisplayName} on room {participant.ChannelName}, and Mute = {participant.IsMuted}");
            SetConnected(ParticipantType.Remote);
            AddParticipantAudioTap(participant);

        }
        //binding event for self
        else
        {
            VivoxVoiceManager.LocalParticipant = participant;
            _sceneView.OnToggleSpriteSwap(VivoxVoiceManager.LocalParticipant.IsMuted);
            _sceneView.AudioToggleActiveState(true);
            SpawnManager.Instance.SetupLipSyncComponents(ParticipantType.Local);

        }
    }

    private void AddParticipantAudioTap(VivoxParticipant participant)
    {
        participant.DestroyVivoxParticipantTap();
        _partipantAudioTap = participant.CreateVivoxParticipantTap();

        SpawnManager.Instance.SetupLipSyncComponents(ParticipantType.Remote, _partipantAudioTap);
        DontDestroyOnLoad(_partipantAudioTap);
    }

    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        Debug.Log("Participant removed event " + participant.DisplayName);

        //make sure it is always a remote player
        if (participant.PlayerId != AuthenticationService.Instance.PlayerId)
        {
            Debug.Log($"Participant added : ID : {participant.PlayerId}, {participant.DisplayName} on room {participant.ChannelName}, and Mute = {participant.IsMuted}");
            SetDisConnected(ParticipantType.Remote);  
        }
    }


    private void SetConnected(ParticipantType type = ParticipantType.Local)
    {
        _sceneView.SetParticipantVoiceStatus(type, VoiceStatus.Connected);
    }
    private void SetDisConnected(ParticipantType type = ParticipantType.Local)
    {
        _sceneView.SetParticipantVoiceStatus(type, VoiceStatus.DisConnected);
    }

    private void SetConnecting(ParticipantType type = ParticipantType.Local)
    {
        _sceneView.SetParticipantVoiceStatus(type, VoiceStatus.Connecting);
    }

   
    public void ToggleMute()
    {
        if (VivoxVoiceManager.LocalParticipant.IsMuted)
        {
            VivoxVoiceManager.LocalParticipant.UnmutePlayerLocally();
        }
        else
        {
            VivoxVoiceManager.LocalParticipant.MutePlayerLocally();
        }
        _sceneView.OnToggleSpriteSwap(VivoxVoiceManager.LocalParticipant.IsMuted);
        Debug.Log($"Currently local is Muted : {VivoxVoiceManager.LocalParticipant.IsMuted}");


    }
  

}
