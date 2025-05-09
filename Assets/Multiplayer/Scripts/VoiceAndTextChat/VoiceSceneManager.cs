using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

public class VoiceSceneManager : MonoBehaviour
{
    [SerializeField] private VoiceSceneView _sceneView;
    

    private async Task InitializationAndJoinChannelTest()
    {
        await UnityServices.InitializeAsync();

        await AuthenticationManager.SignInAnonymously(PlayerName);

        await VivoxVoiceManager.Instance.InitializeAndSignInVivox(PlayerName);

        await VivoxVoiceManager.Instance.JoinVoiceChannel(ChannelName);
    }
    private void Start()
    {
        VivoxService.Instance.ChannelJoined += OnChannelJoined;
        VivoxService.Instance.ConnectionRecovered += OnConnectionRecovered;
        VivoxService.Instance.ConnectionRecovering += OnConnectionRecovering;
        VivoxService.Instance.ConnectionFailedToRecover += OnConnectionFailedToRecover;
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;


        VivoxVoiceManager. OnConnecting = () => SetConnecting();



        SetConnecting(ParticipantType.Remote);

    }

    private void OnDestroy()
    {
        VivoxService.Instance.ChannelJoined -= OnChannelJoined;

        VivoxService.Instance.ConnectionRecovered -= OnConnectionRecovered;
        VivoxService.Instance.ConnectionRecovering -= OnConnectionRecovering;
        VivoxService.Instance.ConnectionFailedToRecover -= OnConnectionFailedToRecover;
        VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;

    }

    #region Testing

    public string PlayerName = "Umar";
    public string ChannelName = "John_Marry";

    public void JoinChannelTest()
    {
        InitializationAndJoinChannelTest();
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

        Debug.Log($"Participant added {participant.DisplayName} on room {participant.ChannelName}, and Mute = {participant.IsMuted}");
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
    #endregion Testing

}
