using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbySceneView : SceneViewBase
{
    [Header("Remote Player Data Holder")]
    [SerializeField]
    GameObject remotePayerDataHolder;
    [SerializeField]
    TextMeshProUGUI remotePlayerNameText;

    [SerializeField]
    LobbyPanelView _lobbyPanelView;



    public void SetRemotePlayerData(string playerName)
    {
        remotePayerDataHolder.SetActive(true);
        remotePlayerNameText.text = $"{playerName}";
    }

    public void SetLobbyPanelForPlayerWhoInitiateTest(string playerID, string requestedGameName)
    {
        if (!_lobbyPanelView.IsPanelOpened)
        {
            //spawning players and sequence players icons so they player who initiate game request will appear on left side
            _lobbyPanelView.SpawnPlayerIcons(playerID);

            //set button visiblity as per reqest sender and receiving roles
            var isInitiator = playerID == LobbyManager.playerId;
            _lobbyPanelView.SetButtonsVisiblity(isInitiator);

            //Opening panel 
            _lobbyPanelView.OpenLobbyPanel("You want to play", requestedGameName);
        }
        _lobbyPanelView.SetReadyStates();
    }  
    
    public void SetLobbyPanelForPlayerWhoReceiveGameRequest(Player player, string requestedGameName)
    {
        if (!_lobbyPanelView.IsPanelOpened)
        {
            //spawning players and sequence players icons so they player who initiate game request will appear on left side
            _lobbyPanelView.SpawnPlayerIcons(player.Id);

            //set button visiblity as per reqest sender and receiving roles
            var isInitiator = player.Id == LobbyManager.playerId;
            _lobbyPanelView.SetButtonsVisiblity(isInitiator);

            //Opening panel 
            var playerName = player.Data[LobbyManager.k_PlayerNameKey].Value;
            _lobbyPanelView.OpenLobbyPanel($"{playerName} wants to play", requestedGameName);
        }
        _lobbyPanelView.SetReadyStates();
    }


    public void SetReadyStates()
    {
        _lobbyPanelView.SetReadyStates();
    }

    public void CloseLobbyPanel()
    {
        _lobbyPanelView.Close();
        SetInteractable(true);
    }

    public void ShowJoining()
    {
        _lobbyPanelView.SetJoiningObj(true);
    }



    public void SetLobbyPanel(string gameName, bool isSendingRequest,string readyPlayerName = default)
    {
        //if (!_lobbyPanelView.IsPanelOpened)
        {
            //set button visiblity as per reqest sender and receiving roles
           
            _lobbyPanelView.SetButtonsVisiblity(isSendingRequest);

            //Opening panel 
            var title = isSendingRequest ? "You want to play":$"{readyPlayerName} wants to play" ;
            _lobbyPanelView.OpenLobbyPanel(title,gameName);
        }
    }

    public void UpdatePlayerIcons(List<Player> players)
    {
        _lobbyPanelView.SpawnPlayerIconsNew(players);
    }

    //public void UpdateReadyStates(List<Player> players)
    //{
    //    foreach (var p in players)
    //    {
    //        _lobbyPanelView.SetReadyState(p.Id);
    //    }
    //}








}
