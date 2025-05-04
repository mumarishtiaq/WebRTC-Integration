using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using System.Linq;
using Unity.Services.Authentication;

public class LobbyManager : MonoBehaviour
{

    // Lobby data key used to lookup each players' name as displayed in the lobby.
    public const string k_PlayerNameKey = "playerName";

    // Lobby data key used to check if each player has clicked the [Ready] button.
    public const string k_IsReadyKey = "isReady";

    public bool isHost { get; private set; }

    // Lobby data for host name.
    public const string k_HostNameKey = "hostName";

    // Lobby data for host's Relay Join Code. Used to allow all players to initialize Relay so NGO
    // (Netcode for GameObjects) can synchronize multiplayer game play between players.
    public const string k_RelayJoinCodeKey = "relayJoinCode";

    //Common room name between 2 peers, so they can joim in same room/lobby
    public const string k_CommonRoomName = "RoomName";

    public List<Player> players { get; private set; }

    public static string playerId => AuthenticationService.Instance.PlayerId;

    string m_PlayerName;

    bool m_IsPlayerReady = false;

    bool m_WasGameStarted = false;

    // Frequency for host to call SendHeartbeatPingAsync to keep lobby active.
    // Note that if called to frequently, this will result in rate limit exceptions.
    const float k_HostHeartbeatFrequency = 15;

    public static LobbyManager Instance { get; private set; }

    public List<Lobby> lobbiesList { get; private set; } = new List<Lobby>();

    public Lobby activeLobby { get; private set; }

    public static event Action OnPlayerNotInLobbyEvent;

    float heartbeatTimer;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    async void Update()
    {
        HandleLobbyHeartbeat();
    }

    private async void HandleLobbyHeartbeat()
    {
        try
        {
            if (activeLobby != null)
            {
                if (isHost)
                {
                    heartbeatTimer -= Time.deltaTime;
                    if (heartbeatTimer < 0f)
                    {
                        heartbeatTimer = k_HostHeartbeatFrequency;
                        await LobbyService.Instance.SendHeartbeatPingAsync(activeLobby.Id);
                    }
                }
            }


        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }


    [ContextMenu("Create Lobby")]
    private void LobbyCreateTest()
    {

        CreateLobby(MultiplayerManager.Instance._playerData.ChannelName, MultiplayerManager.Instance._playerData.Name, "TestRelayCode");
    }

    [ContextMenu("GetPublicLobbiesTest")]
    private void GetPublicLobbiesTest()
    {
        GetPublicLobbies(MultiplayerManager.Instance._playerData.ChannelName);
    }
    public async Task<Lobby> CreateLobby(string lobbyName, string hostName, string relayJoinCode)
    {
        try
        {
            isHost = true;
            m_PlayerName = hostName;
            m_WasGameStarted = false;
            m_IsPlayerReady = false;


            await DeleteAnyActiveLobbyWithNotify();
            if (this == null) return default;

            var options = new CreateLobbyOptions();
            options.IsPrivate = false;
            options.Data = new Dictionary<string, DataObject>
                {
                    { k_HostNameKey, new DataObject(DataObject.VisibilityOptions.Public, hostName) },
                    { k_RelayJoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) },
                    { k_CommonRoomName, new DataObject(DataObject.VisibilityOptions.Public, lobbyName,DataObject.IndexOptions.S1) }
                };

            options.Player = CreatePlayerData();

            activeLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 2, options);
            if (this == null) return default;

            players = activeLobby?.Players;

            Log(activeLobby);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return activeLobby;
    }
    public async Task DeleteAnyActiveLobbyWithNotify()
    {
        try
        {
            if (activeLobby != null && isHost)
            {
                await LobbyService.Instance.DeleteLobbyAsync(activeLobby.Id);
                if (this == null) return;

                OnPlayerNotInLobby();
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task<Lobby> JoinLobby(string lobbyId, string playerName)
    {
        try
        {
            await PrepareToJoinLobby(playerName);

            if (this == null) return default;

            var options = new JoinLobbyByIdOptions();
            options.Player = CreatePlayerData();

            activeLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
            if (this == null) return default;

            players = activeLobby?.Players;
        }
        catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.LobbyNotFound)
        {
            // Catch the lobby-not-found exception and rethrow so caller can pop a message.
            if (this == null) return null;

            activeLobby = null;

            throw;
        }
        catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.LobbyFull)
        {
            if (this == null) return null;

            activeLobby = null;

            throw;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return activeLobby;
    }


    public async Task<List<Lobby>> GetPublicLobbies(string commonRoomName)
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                     new QueryFilter(
                        field: QueryFilter.FieldOptions.S1, // Use S1–S5 for string filters
                         op: QueryFilter.OpOptions.EQ,
                         value: commonRoomName)
                }

            };
            var lobbiesQuery = await LobbyService.Instance.QueryLobbiesAsync(options);
            if (this == null) return default;

            lobbiesList = lobbiesQuery.Results;

            Debug.Log($"Founded {lobbiesList.Count} Lobbies!!!");
            foreach (var lobby in lobbiesList)
            {
                Log(lobby);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return lobbiesList;
    }

    async Task PrepareToJoinLobby(string playerName)
    {
        isHost = false;
        m_PlayerName = playerName;
        m_WasGameStarted = false;
        m_IsPlayerReady = false;

        if (activeLobby != null)
        {
            Debug.Log("Already in a lobby when attempting to join so leaving old lobby.");
            await LeaveJoinedLobby();
        }
    }
    public async Task LeaveJoinedLobby()
    {
        try
        {
            await RemovePlayer(playerId);
            if (this == null) return;

            OnPlayerNotInLobby();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    public async Task RemovePlayer(string playerId)
    {
        try
        {
            if (activeLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(activeLobby.Id, playerId);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    public void OnPlayerNotInLobby()
    {
        if (activeLobby != null)
        {
            activeLobby = null;

            //TODO:
            OnPlayerNotInLobbyEvent?.Invoke();
        }
    }
    Player CreatePlayerData()
    {
        var player = new Player();
        player.Data = CreatePlayerDictionary();

        return player;
    }

    Dictionary<string, PlayerDataObject> CreatePlayerDictionary()
    {
        var playerDictionary = new Dictionary<string, PlayerDataObject>
            {
                { k_PlayerNameKey,  new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, m_PlayerName) },
                { k_IsReadyKey,  new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, m_IsPlayerReady.ToString()) },
            };

        return playerDictionary;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #region Debugging

    public static void Log(Lobby lobby)
    {
        if (lobby is null)
        {
            Debug.Log("No active lobby.");

            return;
        }

        var lobbyData = lobby.Data.Select(kvp => $"{kvp.Key} is {kvp.Value.Value}");
        var lobbyDataStr = string.Join(", ", lobbyData);

        Debug.Log($"Lobby Named:{lobby.Name}, " +
            $"Players:{lobby.Players.Count}/{lobby.MaxPlayers}, " +
            $"IsPrivate:{lobby.IsPrivate}, " +
            $"IsLocked:{lobby.IsLocked}, " +
            $"LobbyCode:{lobby.LobbyCode}, " +
            $"Id:{lobby.Id}, " +
            $"Created:{lobby.Created}, " +
            $"HostId:{lobby.HostId}, " +
            $"EnvironmentId:{lobby.EnvironmentId}, " +
            $"Upid:{lobby.Upid}, " +
            $"Lobby.Data:{lobbyDataStr}");
    }

    #endregion Debugging
}
