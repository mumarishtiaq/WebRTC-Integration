using NUnit.Framework;
using System;
using System.Collections.Generic;
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

    [SerializeField] private VivoxParticipant _localParticipant;

    [SerializeField] private GameObject  _partipantAudioTap;

    PeerData _peerData => MultiplayerManager.Instance.PeerData;

    private async void Awake()
    {
       await VivoxVoiceManager.Instance.JoinVoiceChannel(_peerData.CommonRoomName);


        //if (IsMicPermissionGranted())
        {
            // The user authorized use of the microphone.
            //await InitializationAndJoinChannelTest();
        }
        //else
        //{
        //    // We do not have the needed permissions.
        //    // Ask for permissions or proceed without the functionality enabled if they were denied by the user
        //    if (IsPermissionsDenied())
        //    {
        //        m_PermissionAskedCount = 0;
        //        await InitializationAndJoinChannelTest();
        //    }
        //    else
        //    {
        //        AskForPermissions();
        //    }
        //}
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

            SetConnecting(ParticipantType.Local);
            SetConnecting(ParticipantType.Remote);

        }
        else
        {
            SetDisConnected(ParticipantType.Local);
            SetDisConnected(ParticipantType.Remote);
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
            _localParticipant = participant;
            _sceneView.OnToggleSpriteSwap(_localParticipant.IsMuted);
            _sceneView.AudioToggleActiveState(true);
            SpawnManager.Instance.SetupLipSyncComponents(ParticipantType.Local);

        }
    }

    private void AddParticipantAudioTap(VivoxParticipant participant)
    {
        participant.DestroyVivoxParticipantTap();
        _partipantAudioTap = participant.CreateVivoxParticipantTap();

        SpawnManager.Instance.SetupLipSyncComponents(ParticipantType.Remote, _partipantAudioTap);
        //SetupLipSyncComponents(_partipantAudioTap, SpawnManager.Instance.RemotePlayerAvatar.HeadMesh);

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
        if (_localParticipant.IsMuted)
        {
            _localParticipant.UnmutePlayerLocally();
        }
        else
        {
            _localParticipant.MutePlayerLocally();
        }
        _sceneView.OnToggleSpriteSwap(_localParticipant.IsMuted);
        Debug.Log($"Currently local is Muted : {_localParticipant.IsMuted}");


    }

    private void SetupLipSyncComponents(GameObject audioTapObj, SkinnedMeshRenderer rend)
    {
        if (audioTapObj != null)
        {
            //getting and setting up audio source
           var src= audioTapObj.GetComponent<AudioSource>();
            //src.playOnAwake = true;
            //src.loop = true;
            //src.mute = false;
            //src.spatialBlend = 0f;
            //if (!src.isPlaying)
            //    src.Play();


            var lipSync = audioTapObj.AddComponent<OVRLipSyncContext>();
            lipSync.audioSource = src;
            lipSync.audioLoopback = true;

            var morph = audioTapObj.AddComponent<OVRLipSyncContextMorphTarget>();
            morph.skinnedMeshRenderer = rend;
        }
        else
        {
            Debug.Log("Tap Obj is null");
        }
    }





    #region Permissions

    int m_PermissionAskedCount;


#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
    bool IsAndroid12AndUp()
    {
        // android12VersionCode is hardcoded because it might not be available in all versions of Android SDK
        const int android12VersionCode = 31;
        AndroidJavaClass buildVersionClass = new AndroidJavaClass("android.os.Build$VERSION");
        int buildSdkVersion = buildVersionClass.GetStatic<int>("SDK_INT");

        return buildSdkVersion >= android12VersionCode;
    }

    string GetBluetoothConnectPermissionCode()
    {
        if (IsAndroid12AndUp())
        {
            // UnityEngine.Android.Permission does not contain the BLUETOOTH_CONNECT permission, fetch it from Android
            AndroidJavaClass manifestPermissionClass = new AndroidJavaClass("android.Manifest$permission");
            string permissionCode = manifestPermissionClass.GetStatic<string>("BLUETOOTH_CONNECT");

            return permissionCode;
        }

        return "";
    }
#endif

    bool IsMicPermissionGranted()
    {
        bool isGranted = Permission.HasUserAuthorizedPermission(Permission.Microphone);
#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        if (IsAndroid12AndUp())
        {
            // On Android 12 and up, we also need to ask for the BLUETOOTH_CONNECT permission for all features to work
            isGranted &= Permission.HasUserAuthorizedPermission(GetBluetoothConnectPermissionCode());
        }
#endif
        return isGranted;
    }

    void AskForPermissions()
    {
        string permissionCode = Permission.Microphone;

#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        if (m_PermissionAskedCount == 1 && IsAndroid12AndUp())
        {
            permissionCode = GetBluetoothConnectPermissionCode();
        }
#endif
        m_PermissionAskedCount++;
        Permission.RequestUserPermission(permissionCode);
    }

    bool IsPermissionsDenied()
    {
#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        // On Android 12 and up, we also need to ask for the BLUETOOTH_CONNECT permission
        if (IsAndroid12AndUp())
        {
            return m_PermissionAskedCount == 2;
        }
#endif
        return m_PermissionAskedCount == 1;
    }


    #endregion 

}
