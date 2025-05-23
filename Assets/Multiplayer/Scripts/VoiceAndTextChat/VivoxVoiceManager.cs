using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.Android;


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
            isInitializeAndLoggedIn = false;
        }
    }

    public async Task SignInWithPermissions(string playerName)
    {
        if (IsMicPermissionGranted())
        {
            // The user authorized use of the microphone.
            await InitializeAndSignInVivox(playerName);
        }
        else
        {
            // We do not have the needed permissions.
            // Ask for permissions or proceed without the functionality enabled if they were denied by the user
            if (IsPermissionsDenied())
            {
                m_PermissionAskedCount = 0;
                await InitializeAndSignInVivox(playerName);
            }
            else
            {
                AskForPermissions();
            }
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

    void LoginToVivoxService()
    {
        
    }

    #endregion
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
