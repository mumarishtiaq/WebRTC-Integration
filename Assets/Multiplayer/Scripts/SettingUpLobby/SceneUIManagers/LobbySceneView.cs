using NUnit.Framework;
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


    public void SetLobbyPanelForPlayerWhoInitiate(string gameName, string playerID)
    {
        //spawning player icons
        _lobbyPanelView.SpawnPlayerIcons(playerID);
        
        //sequencing player, the player who initiate to play will display on left side
        _lobbyPanelView.SequencePlayers(playerID, true);

        //set player icon ready state
        _lobbyPanelView.SetReadyState(playerID,true);


        _lobbyPanelView.SetButtonsState(true);

        //opening lobby panel
        _lobbyPanelView.OpenLobbyPanel("You want to play", gameName);



    }


    public void SetLobbyPanelForPlayerWhoReceiveRequesr(string playerID,string readyPlayerName,string requestedGameName)
    {
        //spawning player icons
        _lobbyPanelView.SpawnPlayerIcons(playerID);

        //sequencing player, the player who initiate to play will display on left side
        _lobbyPanelView.SequencePlayers(playerID,isInitiator: false);

        //set player icon ready state
        _lobbyPanelView.SetReadyState(playerID, false);

        _lobbyPanelView.SetButtonsState(false);


        //opening lobby panel
        _lobbyPanelView.OpenLobbyPanel($"{readyPlayerName} wants to play", requestedGameName);
    }




}
