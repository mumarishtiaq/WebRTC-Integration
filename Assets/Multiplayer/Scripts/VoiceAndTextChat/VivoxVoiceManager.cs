using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;


public class VivoxVoiceManager : MonoBehaviour
{
    public static VivoxVoiceManager Instance;
    public bool isInitializeAndLoggedIn { get; private set; }

    public static Action OnConnecting;

    public static VivoxParticipant LocalParticipant;



    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    



    public async Task InitializeAndSignInVivox(string playerName)
    {
        try
        {
            OnConnecting?.Invoke();
            await VivoxService.Instance.InitializeAsync();
            var options = new LoginOptions
            {
                DisplayName = playerName,
            };
            await VivoxService.Instance.LoginAsync(options);

            isInitializeAndLoggedIn = true;
            Debug.Log("Vivox initialize and Sign in ");
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex); //TODO}

        }
    }

    public async Task JoinVoiceChannel(string commonRoomName)
    {
        if (isInitializeAndLoggedIn)
            await VivoxService.Instance.JoinGroupChannelAsync(commonRoomName, ChatCapability.TextAndAudio);

        else
        {
            Debug.LogWarning("Voice Not Initialized");
        }

    }

    public async void LeaveVoiceChannel(string commonRoomName)
    {
        await VivoxService.Instance.LeaveChannelAsync(commonRoomName);

        await VivoxService.Instance.LogoutAsync();
    }



    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }


    }
}
public enum VoiceStatus 
{ 
    Connected,
    Connecting,
    DisConnected 
}
public enum ParticipantType 
{ 
   Local,
   Remote
}
