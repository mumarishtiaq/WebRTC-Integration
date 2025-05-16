using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class LobbySceneManager : MonoBehaviour
{
    [SerializeField] private LobbySceneView _sceneView;
    [SerializeField] private GamesView _gameView;
    private PeerData _peerData => MultiplayerManager.Instance.PeerData;

    private GameType _gameType;

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

                _sceneView.SetLobbyPanelForPlayerWhoInitiate(readableGameName, LobbyManager.playerId);

                _gameView.Close();

                LobbyManager.m_playerSelectedGame = gameType;
                await LobbyManager.Instance.ToggleReadyStateAndSetSelectedGame();
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

    public async void OnAcceptButtonClicked()
    {
        _sceneView.SetInteractable(false);
        _gameView.Close();
        LobbyManager.m_playerSelectedGame = _gameType;
        _sceneView.SwitchReadyState(LobbyManager.playerId);
        await LobbyManager.Instance.ToggleReadyStateAndSetSelectedGame();
        //_sceneView.SetInteractable(true);

    }
    public async void OnRejectButtonClicked()
    {
        _sceneView.SetInteractable(false);
        _gameView.Close();

        _gameType = GameType.None;
        LobbyManager.m_playerSelectedGame = _gameType;
        _sceneView.SwitchReadyState(LobbyManager.playerId);
        await LobbyManager.Instance.ToggleReadyStateAndSetSelectedGame();
        _sceneView.SetInteractable(true);

    }


    /// <summary>
    /// This method will invoke if both the players Ready Flag set to true
    /// </summary>
    private void OnGameReady(List<Player> players)
    {
        Debug.Log("Game is ready to start");
    }

    /// <summary>
    /// This Method will always invoke on the remote side, means the player who is not initiating to play the game, this code will run on the player side who is not initiating game
    /// </summary>
    /// <param name="readyplayer">The player who initiated the game, this includes player details along with the game player wants to play</param>
    private void OnPlayerInitiateToPlayGame(Player readyplayer,string requestedGameName)
    {
        if (Enum.TryParse<GameType>(requestedGameName, true, out var gameType))
        {
            var readyPlayerName = readyplayer.Data[LobbyManager.k_PlayerNameKey].Value;
            string readableGameName = GetReadableGameName(requestedGameName);
            Debug.Log($"{readyPlayerName} wants to play {requestedGameName}");

            _gameType = gameType;

            _sceneView.SetLobbyPanelForPlayerWhoReceiveRequest(LobbyManager.playerId, readyplayer.Id, readyPlayerName, readableGameName);

            _gameView.Close();
        }
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
