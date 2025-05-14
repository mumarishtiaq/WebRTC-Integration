using System;
using System.Linq;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;

public class LobbySceneManager : MonoBehaviour
{
    [SerializeField] private LobbySceneView _sceneView;
    [SerializeField] private GamesView _gameView;
    private PeerData _peerData => MultiplayerManager.Instance.PeerData;

    private void Start()
    {

        LobbyManager.OnGameReady += OnGameReady;
        LobbyManager.OnPlayerInitiateToPlayGame += OnPlayerInitiateToPlayGame;

        _sceneView.SetPlayerData(_peerData.LP.Name);
        _sceneView.SetRemotePlayerData(_peerData.RP.Name);


        SpawnManager.Instance.SpawnRemotePlayer(LobbyManager.Instance.GetRemotePlayerAvatarIndex(), _peerData.RP.Gender);
        SpawnManager.Instance.SetTransform_LocalPlayer();
        SpawnManager.Instance.TriggerAnimations(ParticipantType.Remote, AnimationType.Sit);
        SpawnManager.Instance.TriggerAnimations(ParticipantType.Local, AnimationType.Sit);
    }


    public async void OnGamePlayButtonClicked(string gameName)
    {
        try
        {
            if (Enum.TryParse<GameType>(gameName, true, out var gameType))
            {
                string readableGameName = GetReadableGameName(gameName);
                Debug.Log("Selected Game Type: " + readableGameName);



                _sceneView.SetLobbyPanelForPlayerWhoInitiate(gameName, LobbyManager.playerId);

                await LobbyManager.Instance.ToggleReadyStateAndSetSelectedGame(gameType);
            }
            else
            {
                Debug.LogError("Invalid Game Name");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    /// <summary>
    /// This method will invoke if both the players Ready Flag set to true
    /// </summary>
    private void OnGameReady()
    {
        Debug.Log("Game is ready to start");
    }

    /// <summary>
    /// This Method will always invoke on the remote side, means the player who is not initiating to play the game, this code will run on the player side who is not initiating game
    /// </summary>
    /// <param name="readyplayer">The player who initiated the game, this includes player details along with the game player wants to play</param>
    private void OnPlayerInitiateToPlayGame(Player readyplayer,string requestedGameName)
    {
        var readyPlayerName = readyplayer.Data[LobbyManager.k_PlayerNameKey].Value;
        Debug.Log($"{readyPlayerName} wants to play {requestedGameName}");

        _sceneView.SetLobbyPanelForPlayerWhoReceiveRequesr(LobbyManager.playerId, readyPlayerName, requestedGameName);
    }

    private string GetReadableGameName(string gameName)
    {
        return gameName.Replace("_", " ");
    }

    private void OnDestroy()
    {
        LobbyManager.OnGameReady -= OnGameReady;
        LobbyManager.OnPlayerInitiateToPlayGame -= OnPlayerInitiateToPlayGame;


    }

    
}
