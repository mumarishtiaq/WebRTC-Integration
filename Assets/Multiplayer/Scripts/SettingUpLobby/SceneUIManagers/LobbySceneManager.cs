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
                Debug.Log("Selected Game Type: " + readableGameName);


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
            var gameSceneName = GetSceneName(selectedGame);

            if(gameSceneName != string.Empty)
            {

                //NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
                StartCoroutine(LoadSceneUntilClientConnects(gameSceneName));
            }

            else
            {
                Debug.LogWarning("In valid scene name");
            }
        }
        else
        {
            JoinRelayAsClient();
        }
    }

    [ContextMenu("GetClientsCount")]
    private async void GetClientsCount()
    {
        Debug.Log($"Connected Clients {NetworkManager.Singleton.ConnectedClients.Count}");
    } 
    
    [ContextMenu("JoinRelay")]
    private async void JoinRelayAsClient()
    {
        if(!NetworkServiceManager.Instance.m_NetworkManagerInitialized)
        {
            var relayJoinCode = LobbyManager.Instance.activeLobby.Data[LobbyManager.k_RelayJoinCodeKey].Value;
            await NetworkServiceManager.Instance.InitializeClient(relayJoinCode);
        }
        
    }
    
    [ContextMenu("LoadScene")]
    private  void LoadScene()
    {
        StartCoroutine(LoadSceneCouroutine());
    }

    private IEnumerator LoadSceneCouroutine()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton.ConnectedClients.Count == 2);
        NetworkManager.Singleton.SceneManager.LoadScene("CoinRushScene", LoadSceneMode.Single);
    }

    [ContextMenu("ToggleReadyState")]
    private async void ToggleReadyState()
    {
        await LobbyManager.Instance.ToggleReadyStateAndSetSelectedGame(false, GameType.None);
        LobbyManager.m_WasGameStarted = false;
    }

    private IEnumerator LoadSceneUntilClientConnects(string gameSceneName)
    {
        yield return new WaitUntil(() => NetworkManager.Singleton.ConnectedClients.Count == 2);
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }


    private string GetSceneName(GameType gameType)
    {
        string gameSceneName = string.Empty;
        switch (gameType)
        {
            case GameType.Tic_Tac_Toe:
                gameSceneName = "TicTacToeScene";
                break;
            case GameType.Coin_Rush:
                gameSceneName = "CoinRushScene";
                break;
            case GameType.Chess:
                gameSceneName = "ChessScene";
                break;
            default:
                break;
        }

        return gameSceneName;
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
