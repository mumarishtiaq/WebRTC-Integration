using Games.CoinRush;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.GraphView.GraphView;

public class LobbySceneManager : MonoBehaviour
{
    [SerializeField] private LobbySceneView _sceneView;
    [SerializeField] private GamesView _gameView;
    private PeerData _peerData => MultiplayerManager.Instance.PeerData;

    private GameType _gameType;

    async void Start()
    {

        LobbyManager.OnGameReady += OnGameReady;
        LobbyManager.OnGameRequestInitiated += OnGameRequestInitiated;
        LobbyManager.OnGameRequestReceived += OnGameRequestReceived;

        _sceneView.SetPlayerData(_peerData.LP.Name);
        _sceneView.SetRemotePlayerData(_peerData.RP.Name);


        SpawnManager.Instance.SpawnRemotePlayer(LobbyManager.Instance.GetRemotePlayerAvatarIndex(), _peerData.RP.Gender);
        SpawnManager.Instance.SetTransform_LocalPlayer();
        SpawnManager.Instance.TriggerAnimations(ParticipantType.Remote, AnimationType.Sit);
        SpawnManager.Instance.TriggerAnimations(ParticipantType.Local, AnimationType.Sit);


        //this flag = false will prevent to continously check for ready state in LobbyManager
        LobbyManager.m_WasGameStarted = false;

    }

    private void OnGameRequestInitiated(string playerID , GameType gameType)
    {
        string readableGameName = GetReadableGameName(gameType.ToString());
        _sceneView.SetLobbyPanelForPlayerWhoInitiateTest( playerID, readableGameName);
        _gameView.Close();
    } 
    
    private void OnGameRequestReceived(Player player , GameType gameType)
    {
        string readableGameName = GetReadableGameName(gameType.ToString());
        _gameType = gameType;
        _sceneView.SetLobbyPanelForPlayerWhoReceiveGameRequest(player, readableGameName);
        _gameView.Close();
    }

    

    public async void OnGamePlayButtonClicked(string gameName)
    {
        try
        {
            _sceneView.SetInteractable(false);
            if (Enum.TryParse<GameType>(gameName, true, out var gameType))
            {
                string readableGameName = GetReadableGameName(gameName);


                _gameView.Close();

                LobbyManager.m_playerSelectedGame = gameType;
                await LobbyManager.Instance.ToggleReadyStateAndSetSelectedGame(true,gameType);
                _sceneView.SetInteractable(true);
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

    public async void OnAcceptButtonPressed()
    {
        _sceneView.SetInteractable(false);
        await LobbyManager.Instance.ToggleReadyStateAndSetSelectedGame(true,_gameType);

    }
    public async void OnRejectButtonPressed()
    {
        _sceneView.SetInteractable(false);
        await LobbyManager.Instance.ToggleReadyStateAndSetSelectedGame(false,GameType.None);
        _sceneView.CloseLobbyPanel();
        
    }


    /// <summary>
    /// This method will invoke if both the players Ready Flag set to true
    /// </summary>
    private void OnGameReady(List<Player> players,GameType selectedGame)
    {
        Debug.Log("Game is ready to start");

        _sceneView.SetReadyStates();
        _sceneView.SetInteractable(false);
        _sceneView.ShowJoining();
        LobbyManager.m_WasGameStarted = true;



        if (LobbyManager.Instance.isHost)
        {
            var sceneType = GetSceneType(selectedGame);

            if(sceneType != SceneType.None)
            {
                StartCoroutine(LoadSceneUntilClientConnects(sceneType));
            }

            else
            {
                Debug.LogWarning("In valid scene type");
            }
        }
        else
        {
            JoinRelayAsClient();
        }
    }
    
    private async void JoinRelayAsClient()
    {
        if(!NetworkServiceManager.Instance.m_NetworkManagerInitialized)
        {
            var relayJoinCode = LobbyManager.Instance.activeLobby.Data[LobbyManager.k_RelayJoinCodeKey].Value;
            await NetworkServiceManager.Instance.InitializeClient(relayJoinCode);
        }
        
    }
    
    private IEnumerator LoadSceneUntilClientConnects(SceneType sceneType)
    {
        yield return new WaitUntil(() => NetworkManager.Singleton.ConnectedClients.Count == 2);
        SceneManagerCustom.LoadNetworkScene(sceneType);
    }


    private SceneType GetSceneType(GameType gameType)
    {
        var sceneType = SceneType.None;
        switch (gameType)
        {
            case GameType.Tic_Tac_Toe:
                sceneType = SceneType.TicTacToe;
                break;
            case GameType.Coin_Rush:
                sceneType = SceneType.CoinRush;
                break;
            case GameType.Chess:
                sceneType = SceneType.ThirdGame;
                break;
            default:
                break;
        }

        return sceneType;
    }
   
    private string GetReadableGameName(string gameName)
    {
        return gameName.Replace("_", " ");
    }

    private void OnDestroy()
    {
        LobbyManager.OnGameReady -= OnGameReady;
        LobbyManager.OnGameRequestInitiated -= OnGameRequestInitiated;
        LobbyManager.OnGameRequestReceived -= OnGameRequestReceived;
    }

    
}
