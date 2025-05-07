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
    
    // Lobby data key used to get each player selected avatar index, will used to fetch remote player's selected avatar index.
    public const string k_PlayerAvatarIndex = "playerAvatarIndex";

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

    float m_NextHostHeartbeatTime;

    float m_NextUpdatePlayersTime;

    public static event Action<Lobby> OnLobbyChanged;



    // Frequency for host to call SendHeartbeatPingAsync to keep lobby active.
    // Note that if called to frequently, this will result in rate limit exceptions.
    const float k_HostHeartbeatFrequency = 15;

    // Frequency to call GetLobbyAsync to update player state, such as join/leave and ready state.
    // Note that if called to frequently, this will result in rate limit exceptions.
    const float k_UpdatePlayersFrequency = 5f;

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
        try
        {
            if (activeLobby != null && !m_WasGameStarted)
            {
                if (isHost && Time.realtimeSinceStartup >= m_NextHostHeartbeatTime)
                {
                    await PeriodicHostHeartbeat();

                    // Exit this update now so we'll only ever update 1 item (heartbeat or lobby changes) in 1 Update().
                    return;
                }

                if (Time.realtimeSinceStartup >= m_NextUpdatePlayersTime)
                {
                    await PeriodicUpdateLobby();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    async Task PeriodicHostHeartbeat()
    {
        try
        {
            // Set next heartbeat time before calling Lobby Service since next update could also trigger a
            // heartbeat which could cause throttling issues.
            m_NextHostHeartbeatTime = Time.realtimeSinceStartup + k_HostHeartbeatFrequency;

            await LobbyService.Instance.SendHeartbeatPingAsync(activeLobby.Id);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    async Task PeriodicUpdateLobby()
    {
        try
        {
            // Set next update time before calling Lobby Service since next update could also trigger an
            // update which could cause throttling issues.
            m_NextUpdatePlayersTime = Time.realtimeSinceStartup + k_UpdatePlayersFrequency;

            var updatedLobby = await LobbyService.Instance.GetLobbyAsync(activeLobby.Id);
            if (this == null) return;

            UpdateLobby(updatedLobby);
        }

        // Handle lobby no longer exists (host canceled game and returned to main menu).
        catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.LobbyNotFound)
        {
            if (this == null) return;

            //TODO
            //ServerlessMultiplayerGameSampleManager.instance.SetReturnToMenuReason(
            //    ServerlessMultiplayerGameSampleManager.ReturnToMenuReason.LobbyClosed);

            OnPlayerNotInLobby();
        }

        // Handle player no longer allowed to view lobby (host booted player so player is no longer in the lobby).
        catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.Forbidden)
        {
            if (this == null) return;

            //TODO
            //ServerlessMultiplayerGameSampleManager.instance.SetReturnToMenuReason(
            //    ServerlessMultiplayerGameSampleManager.ReturnToMenuReason.PlayerKicked);

            OnPlayerNotInLobby();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    [ContextMenu("Create Lobby")]
    private void LobbyCreateTest()
    {

        CreateLobby(MultiplayerManager.Instance.PeerData.CommonRoomName, MultiplayerManager.Instance.PeerData.LP.Name, "TestRelayCode");
    }

    [ContextMenu("GetPublicLobbiesTest")]
    private void GetPublicLobbiesTest()
    {
        GetPublicLobbies(MultiplayerManager.Instance.PeerData.CommonRoomName);
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

    void UpdateLobby(Lobby updatedLobby)
    {
        // Since this is called after an await, ensure that the Lobby wasn't closed while waiting.
        if (activeLobby == null || updatedLobby == null) return;

        Test(updatedLobby);
        if (DidPlayersChange(activeLobby.Players, updatedLobby.Players))
        {
            activeLobby = updatedLobby;
            players = activeLobby?.Players;
            if (updatedLobby.Players.Exists(player => player.Id == playerId))
            {
               
                OnLobbyChanged?.Invoke(updatedLobby);

                
            }
            else
            {
                
                OnPlayerNotInLobby();
            }
        }
    }

    public void Test(Lobby updatedLobby)
    {
        Debug.Log($"Player count : {updatedLobby.Players.Count}");
        foreach (var player in updatedLobby.Players)
        {
            Debug.Log($"Id : {player.Id} ," /*+ $"Name : {player.Profile.Name} " */+ $"Avatar Index : {player.Data[k_PlayerAvatarIndex].Value}");

        }
    }

    public int GetRemotePlayerAvatarIndex()
    {
        if (activeLobby == null) return 0;
        foreach (var player in activeLobby.Players)
        {
            if (player.Id != playerId)
            {
                return Convert.ToInt32(player.Data[k_PlayerAvatarIndex].Value);
            }
        }
        return 0;
    }

    static bool DidPlayersChange(List<Player> oldPlayers, List<Player> newPlayers)
    {
        if (oldPlayers.Count != newPlayers.Count)
        {
            return true;
        }

        for (int i = 0; i < newPlayers.Count; i++)
        {
            if (oldPlayers[i].Id != newPlayers[i].Id ||
                oldPlayers[i].Data[k_IsReadyKey].Value != newPlayers[i].Data[k_IsReadyKey].Value)
            {
                return true;
            }
        }

        return false;
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
            Debug.LogWarning($"This player is no longer in the lobby so returning to main menu.");
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
            
            { k_PlayerAvatarIndex,  new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, SpawnManager.Instance.AvatarIndex.ToString()) }
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
