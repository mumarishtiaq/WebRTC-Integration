using System;
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
        if (Enum.TryParse<GameType>(gameName, true, out var gameType))
        {
            Debug.Log($"Parsed GameType: {gameType}");


            string readableName = gameName.Replace("_", " ");
            Debug.Log("Selected Game Type: " + readableName);

            await LobbyManager.Instance.ToggleReadyState();

            //_sceneView.ShowLobbyPanel(AuthenticationService.Instance.PlayerId);
        }
        else
        {
            Debug.LogError("Invalid Game Name");
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
    /// This Method will always invoke on the remote side, means the player who is not initiating to play the game
    /// </summary>
    /// <param name="player">The player who initiated the game, this includes player details along with the game player wants to play</param>
    private void OnPlayerInitiateToPlayGame(Player player)
    {
        Debug.Log($"Player {player.Id} , Name : {player.Profile.Name} want to play a game #GameName");
    }

    private void OnDestroy()
    {
        LobbyManager.OnGameReady -= OnGameReady;
        LobbyManager.OnPlayerInitiateToPlayGame -= OnPlayerInitiateToPlayGame;


    }

    
}
