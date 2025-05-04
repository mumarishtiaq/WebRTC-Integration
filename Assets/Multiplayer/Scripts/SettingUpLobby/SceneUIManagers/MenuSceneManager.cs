using System;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSceneManager : MonoBehaviour
{
    [SerializeField] private Button _joinBtn; 
    [SerializeField] private Button _exitBtn;

    int m_HostMaxPlayers = 2;
    private void Awake()
    {
        _joinBtn.onClick.AddListener(OnJoinRoom);
    }

    private async void OnJoinRoom()
    {
        try
        {
            // used for setting interactions off TODO
            //sceneView.SetInteractable(false);
            var player = MultiplayerManager.Instance._playerData;

            //checking if the lobby is already created or not 
            LoadingManager.Instance.EnableLoading("Fetching Room Details");
            var lobbies = await LobbyManager.Instance.GetPublicLobbies(player.ChannelName);
            LoadingManager.Instance.EnableLoading("Joining Room");

            //this means lobby is not created and this will be the host
            if (lobbies.Count == 0)
            {
                await JoinAsHost(player);
            }
            //this means lobby is already created now this will be client
            else
            {
                await JoinAsClient(player, lobbies[0]);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private async Task JoinAsHost(PlayerData player)
    {
        var relayJoinCode = await NetworkServiceManager.Instance.InitializeHost(m_HostMaxPlayers);
        if (this == null) return;


        var lobby = await LobbyManager.Instance.CreateLobby(player.ChannelName, player.Name, relayJoinCode);

        LoadingManager.Instance.EnableLoading("Loading Room");
        if (this == null) return;

        await LoadLobbyScene();
    }

    private async Task JoinAsClient(PlayerData player, Lobby lobbyToJoin)
    {
        try
        {
            //sceneView.SetInteractable(false); TODO

            
                var lobbyJoined = await LobbyManager.Instance.JoinLobby(lobbyToJoin.Id, player.Name);
                if (this == null) return;

            

            // If lobby no longer exists (i.e. host left) then popup
            if (lobbyJoined == null)
            {
                ShowLobbyNotFoundPopup();
            }
            else
            {
                var relayJoinCode = lobbyJoined.Data[LobbyManager.k_RelayJoinCodeKey].Value;
                await NetworkServiceManager.Instance.InitializeClient(relayJoinCode);
                await LoadLobbyScene();
            }

        }
        catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.LobbyNotFound)
        {
            ShowLobbyNotFoundPopup();
        }
        catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.LobbyFull)
        {
            ShowLobbyFullPopup();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            if (this != null)
            {
                //interactables back to true TODO
                //sceneView.SetInteractable(true);
            }
        }
    }
    private async Task LoadLobbyScene()
    {
        await SceneManager.LoadSceneAsync("LobbyScene");
        LoadingManager.Instance.EnableLoading("Room Joined", true);
    }

    void ShowLobbyNotFoundPopup()
    {
        //TODO
        //sceneView.ShowPopup("Invalid Lobby", "The lobby you attempted to join no longer exists.\n\nPlease try again.");
        Debug.LogWarning("The lobby you attempted to join no longer exists.\n\nPlease try again.");
    }

    void ShowLobbyFullPopup()
    {
        //TODO
        //sceneView.ShowPopup("Lobby Full", "The lobby you attempted to join is full.\n\nPlease try a different lobby.");
        Debug.LogWarning("The lobby you attempted to join is full.\n\nPlease try a different lobby.");

    }
}
