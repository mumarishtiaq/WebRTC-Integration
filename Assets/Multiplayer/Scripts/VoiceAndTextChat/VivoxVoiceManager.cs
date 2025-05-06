using System;
using Unity.Services.Vivox;
using UnityEngine;
using static UnityEditor.Progress;

public class VivoxVoiceManager : MonoBehaviour
{
    public static VivoxVoiceManager Instance;
    public bool isInitializeAndLoggedIn { get; private set; }


    private void Awake()
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

    public async void InitializeAndSignInVivox(string playerName)
    {
        try
        {
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

    public async void JoinVoiceChannel(string commonRoomName)
    {
        if(isInitializeAndLoggedIn)
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
