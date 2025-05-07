using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSceneManager : MonoBehaviour
{
    [SerializeField] private MenuSceneView _sceneView;

    private PeerData _peerData =>MultiplayerManager.Instance.PeerData;

    int m_HostMaxPlayers = 2;

    bool isHost => LobbyManager.Instance.isHost;

    private void Start()
    {
        _sceneView.JoinBtn.onClick.AddListener(OnJoinRoom);
        LobbyManager.OnLobbyChanged += OnLobbyChanged;
    }


    public void ActivateMainMenuUI(string playerName)
    {
        _sceneView.SetInteractable(true);
        _sceneView.SetPlayerData(playerName);
    }
    public void DeActivateMainMenuUI()
    {
        _sceneView.SetInteractable(false);
    }

    
    private async void OnJoinRoom()
    {
        try
        {
            // used for setting interactions off
            _sceneView.SetInteractable(false);
            var playerData = _peerData.LP;

            //checking if the lobby is already created or not 
            LoadingManager.Instance.EnableLoading("Fetching Room Details");

            if(_peerData.LP.Role == PlayerRole.Host)
            {
                LoadingManager.Instance.EnableLoading("Joining Room");
                await JoinAsHost(playerData, _peerData.CommonRoomName);
                LoadingManager.Instance.EnableLoading("Room Joined", true);
                _sceneView.SetInteractable(true);
                _sceneView.WaitingForOtherPlayer(true);

            }
            //client
            else
            {
                    LoadingManager.Instance.EnableLoading("Joining Room");
                (bool DoExist, List<Lobby> lobbies) = await DoLobbyExist();

                //this means lobby is already created by host
                if(DoExist)
                {
                    LoadingManager.Instance.EnableLoading("Joining Room");
                    await JoinAsClient(playerData, lobbies[0]);
                    _sceneView.SetInteractable(true);
                    LoadLobbyScene();

                }
                else
                {
                    LoadingManager.Instance.DisableLoading();
                    _sceneView.SetInteractable(true);
                    ShowAnotherPlayerNotJoinedYetPopup();

                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private async Task<(bool exists, List<Lobby> lobbies)> DoLobbyExist()
    {
        var lobbies = await LobbyManager.Instance.GetPublicLobbies(_peerData.CommonRoomName);
        return (lobbies.Count > 0, lobbies);
    }


    private async Task JoinAsHost(PlayerData player,string commonRoomName)
    {
        var relayJoinCode = await NetworkServiceManager.Instance.InitializeHost(m_HostMaxPlayers);
        if (this == null) return;


        var lobby = await LobbyManager.Instance.CreateLobby(commonRoomName, player.Name, relayJoinCode);

        LoadingManager.Instance.EnableLoading("Loading Room");
        if (this == null) return;

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
                //await LoadLobbyScene();
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
                _sceneView.SetInteractable(true);
            }
        }
    }

    void OnLobbyChanged(Lobby updatedLobby)
    {
        if (isHost)
        {
            //OnHostLobbyChanged(updatedLobby, isGameReady);
            Debug.LogError("Update Lobby on host side");
            //_sceneView.SetRemotePlayerName(_peerData.RP.Name);
            _sceneView.SetInteractable(true);
            _sceneView.WaitingForOtherPlayer(false);

            LoadLobbyScene();
        }
      
    }
    private async Task LoadLobbyScene()
    {
        LoadingManager.Instance.EnableLoading("Joining room");
        await SceneManager.LoadSceneAsync("LobbyScene");
        LoadingManager.Instance.EnableLoading("Room Joined", true);
        //SpawnManager.Instance.SpawnRemotePlayer()
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
    
    void ShowAnotherPlayerNotJoinedYetPopup()
    {
        //TODO
        _sceneView.ShowPopup("Player Unavailable", "Another player has not been joined yet.");
        Debug.LogWarning("Another player has not been joined yet.");

    }

    void OnDestroy()
    {
        LobbyManager.OnLobbyChanged -= OnLobbyChanged;
    }
}
