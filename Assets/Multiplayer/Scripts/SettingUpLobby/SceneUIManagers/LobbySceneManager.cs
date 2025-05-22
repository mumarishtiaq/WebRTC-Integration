using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

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

        LobbyManager.OnPlayersReadyStateChanged += OnPlayersReadyStateChangedNew;
        LobbyManager.OnPlayerDeclined += OnPlayerDeclined;


        _sceneView.SetPlayerData(_peerData.LP.Name);
        _sceneView.SetRemotePlayerData(_peerData.RP.Name);

        SpawnManager.Instance.TogglePlayersVisiblity(true);

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
                await LobbyManager.Instance.ToggleReadyStateAndSetSelectedGame(true, gameType,false);
                //await LobbyManager.Instance.UpdatePlayerData(gameType, GameRequestStatus.Pending);

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

    [ContextMenu("Accept")]
    public async void OnAcceptButtonPressed()
    {
        _sceneView.SetInteractable(false);
        await LobbyManager.Instance.ToggleReadyStateAndSetSelectedGame(true, _gameType,false);
        //await LobbyManager.Instance.UpdatePlayerData(_gameType, GameRequestStatus.Accepted);


    }
    [ContextMenu("Reject")]

    public async void OnRejectButtonPressed()
    {
        _sceneView.SetInteractable(false);
        await LobbyManager.Instance.ToggleReadyStateAndSetSelectedGame(false, GameType.None, true);
        //await LobbyManager.Instance.UpdatePlayerData(GameType.None, GameRequestStatus.Declined);
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


        LobbyManager.OnPlayersReadyStateChanged -= OnPlayersReadyStateChangedNew;
        LobbyManager.OnPlayerDeclined -= OnPlayerDeclined;

    }

  

    private void OnPlayersReadyStateChanged(List<Player> players, bool isGameReady)
    {
        _sceneView.UpdatePlayerIcons(players);
        if (!isGameReady)
        {
            var localPlayer = new LobbyPlayerData(GetPlayerByID(players, LobbyManager.playerId));
            var remotePlayer = new LobbyPlayerData(GetRemotePlayer(players));

            var readyPlayer = GetReadyPlayer(players);

            if (readyPlayer != null)
            {
                var readyPlayerSelectedGame = Enum.Parse<GameType>(readyPlayer.Data[LobbyManager.k_SelectedGameKey].Value);

                string readableGameName = GetReadableGameName(readyPlayerSelectedGame.ToString());
                bool isSendingRequest = readyPlayer.Id == LobbyManager.playerId;

                if (isSendingRequest)
                {
                    Debug.Log($"You want to Play {readyPlayerSelectedGame}");
                    _sceneView.SetLobbyPanel(readableGameName, isSendingRequest);
                }
                else
                {
                    _gameType = readyPlayerSelectedGame;
                    string readyPlayerName = readyPlayer.Data[LobbyManager.k_PlayerNameKey].Value;
                    Debug.Log($"{readyPlayerName} wants to Play {readyPlayerSelectedGame}");

                    _sceneView.SetLobbyPanel(readableGameName, isSendingRequest, readyPlayerName);

                }
            }
            //if the ready player us null this means that the player who sent game request has canceled the request becuause if no player is ready this means initiator has canceled as the initiator has by default ready state
            else
            {
                var remotePlayerName = GetRemotePlayer(players).Data[LobbyManager.k_PlayerNameKey].Value;
                Debug.Log($"{remotePlayerName} has canceled to play");

            }
        }
        else
        {
            Debug.LogError("Game is Ready To start");

            foreach (var player in players)
            {
                var name = player.Data[LobbyManager.k_PlayerNameKey].Value;
                var isReady = bool.Parse(player.Data[LobbyManager.k_IsReadyKey].Value);
                var game = player.Data[LobbyManager.k_SelectedGameKey].Value;

                Debug.LogError($"{name} -- > IsReady = {isReady} --> Game = {game}");

            }
        }
    }

    private void OnPlayersReadyStateChangedNew(List<Player> players, bool isGameReady)
    {
        _sceneView.UpdatePlayerIcons(players);

            var localPlayer = new LobbyPlayerData(GetPlayerByID(players, LobbyManager.playerId));
            var remotePlayer = new LobbyPlayerData(GetRemotePlayer(players));

        if (!isGameReady)
        {

           
            //sending Game request
            if (localPlayer.IsReady && !remotePlayer.IsReady && localPlayer.SelectedGame != GameType.None && remotePlayer.SelectedGame == GameType.None)
            {
                Debug.Log($"You Want to play {localPlayer.SelectedGame} my state = {localPlayer.IsReady} Another Player state = {remotePlayer.IsReady} , Remote Player Selected Game {remotePlayer.SelectedGame}");

                string readableGameName = GetReadableGameName(localPlayer.SelectedGame.ToString());
                _sceneView.SetLobbyPanel(readableGameName, true);
                _sceneView.ToggleDeclinedPopup();

            }

            //receiving Game request

            if (!localPlayer.IsReady && remotePlayer.IsReady && localPlayer.SelectedGame == GameType.None && remotePlayer.SelectedGame != GameType.None)
            {
                 LobbyManager.Instance.ToggleReadyStateAndSetSelectedGame(false, GameType.None, false);

                Debug.Log($"{remotePlayer.Name} Wants to play {localPlayer.SelectedGame} my state = {localPlayer.IsReady} Another Player state = {remotePlayer.IsReady} ");


                _gameType = remotePlayer.SelectedGame;
                string readableGameName = GetReadableGameName(remotePlayer.SelectedGame.ToString());

                _sceneView.SetLobbyPanel(readableGameName, false, remotePlayer.Name);
                _sceneView.ToggleDeclinedPopup();
            }
        }
        //game is ready tp start
        else
        {
            Debug.LogError("Game is Ready To start");
            _sceneView.ToggleDeclinedPopup();
            OnGameReadyToStart(remotePlayer.SelectedGame);
        }
    }

    private void OnPlayerDeclined(List<Player> players)
    {
        var localPlayer = new LobbyPlayerData(GetPlayerByID(players, LobbyManager.playerId));
        var remotePlayer = new LobbyPlayerData(GetRemotePlayer(players));
        if (remotePlayer.IsDeclined && !localPlayer.IsDeclined)
        {
            _sceneView.ToggleDeclinedPopup($"{remotePlayer.Name} has canceled your request",true);
            Debug.Log($"{remotePlayer.Name} has canceled my state = {localPlayer.IsReady} my game = {localPlayer.SelectedGame}");
            _sceneView.CloseLobbyPanel();
            LobbyManager.Instance.ToggleReadyStateAndSetSelectedGame(false, GameType.None, false);

            return;

        }
    }

    private void OnGameReadyToStart(GameType selectedGame)
    {
        _sceneView.SetReadyStates();
        _sceneView.SetInteractable(false);
        _sceneView.ShowJoining();
        LobbyManager.m_WasGameStarted = true;



        if (LobbyManager.Instance.isHost)
        {
            var sceneType = GetSceneType(selectedGame);

            if (sceneType != SceneType.None)
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



    private Player GetReadyPlayer(List<Player> players)
    {
        return players.FirstOrDefault(p => bool.Parse(p.Data[LobbyManager.k_IsReadyKey].Value) == true);
    } 
    private Player GetPlayerByID(List<Player> players, string id)
    {
        return players.FirstOrDefault(p => p.Id == id);
    }
    private Player GetRemotePlayer(List<Player> players)
    {
        return players.FirstOrDefault(p => p.Id != LobbyManager.playerId);
    }

  


}



public class LobbyPlayerData
{
    public string Id;
    public string Name;
    public bool IsReady = false;
    public bool IsDeclined = false;
    public GameType SelectedGame;

    public LobbyPlayerData(Player player)
    {
        Id = player.Id;
        Name = player.Data[LobbyManager.k_PlayerNameKey].Value;
        IsReady = bool.Parse(player.Data[LobbyManager.k_IsReadyKey].Value);
        IsDeclined = bool.Parse(player.Data[LobbyManager.k_IsDeclinedKey].Value);
        SelectedGame = Enum.Parse<GameType>(player.Data[LobbyManager.k_SelectedGameKey].Value);
    }
}

