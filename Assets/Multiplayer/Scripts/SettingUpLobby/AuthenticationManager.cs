using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

/// <summary>
/// This class will Responsible to initialize unity services and Authentication purposes
/// </summary>
public static class AuthenticationManager
{
    public static async Task SignInAnonymously(string profileName)
    {
        try
        {
            await InitialzeUnityServices(profileName);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Profile: {profileName} PlayerId: {AuthenticationService.Instance.PlayerId} ");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    static async Task InitialzeUnityServices(string profileName)
    {
        try
        {
            var unityAuthenticationInitOptions = new InitializationOptions();
            unityAuthenticationInitOptions.SetProfile(profileName);
            await UnityServices.InitializeAsync(unityAuthenticationInitOptions);
            SetupEvents();

        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

     static void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            // Shows how to get an access token
            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

        };
    }
}
