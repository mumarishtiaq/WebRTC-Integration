using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static TicTacToe.GameManager;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance;
   

   
    [Header("Avatars Data")]
    [SerializeField] private AvatarsData _avatars;


    [Header("Male And Female Spawn Positions")]
    [SerializeField] private Transform _hostSpawnPos;
    [SerializeField] private Transform _clientSpawnPos;

    [HideInInspector]
    public int avatarIndex = 0;

    bool isHost => LobbyManager.Instance.isHost;

    private PeerData _peerData => MultiplayerManager.Instance.PeerData;


    public List<GameObject> MaleNetworkAvatars;
    public List<GameObject> FemaleNetworkAvatars;




    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }


    public void SpawnPlayer(PlayerGender playerType)
    {
        if (!_avatars) return;

        var prefab = playerType == PlayerGender.Male ? _avatars.MalePrefabs[avatarIndex] : _avatars.FemalePrefabs[avatarIndex];

        GameObject character = Instantiate(prefab);


    }

    [Rpc(SendTo.Server)]
    public void SpawnPlayersRpc()
    {
        if (!_avatars) return;

        var prefab = _peerData.LP.Gender == PlayerGender.Male ? MaleNetworkAvatars[avatarIndex] : FemaleNetworkAvatars[avatarIndex];

        GameObject character = Instantiate(prefab);

        Debug.LogError(isHost ? "is host" : "isclient");

        var netObj = character.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(true);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

}
