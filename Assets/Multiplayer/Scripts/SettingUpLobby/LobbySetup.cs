using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class LobbySetup : MonoBehaviour
{

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        Debug.Log(UnityServices.State);
        SetupEvents();
        await SigninAnonymously();


    }
    private async Task SigninAnonymously()
    {
        try
        {

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var instance = AuthenticationService.Instance;
            Debug.Log($"Player ID : {instance.PlayerId}, Name : {instance.PlayerName}, AccessToken : {instance.AccessToken}");
        }
        catch(AuthenticationException ex)
        {
            Debug.LogException(ex);
        } 
        catch(RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
       
    }
    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            // Shows how to get a playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

        };
    }





}
