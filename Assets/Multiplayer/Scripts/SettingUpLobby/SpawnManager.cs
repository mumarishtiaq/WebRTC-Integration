using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static TicTacToe.GameManager;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
   

   
    [Header("Avatars Data")]
    [SerializeField] private AvatarsData _avatars;


    [Header("Male And Female Spawn Positions")]
    [SerializeField] private Transform _hostSpawnPos;
    [SerializeField] private Transform _clientSpawnPos;

    [HideInInspector]
    public int AvatarIndex = 0;

    bool isHost => LobbyManager.Instance.isHost;

    private PeerData _peerData => MultiplayerManager.Instance.PeerData;

    [SerializeField] private GameObject _localPlayerAvatar;
    [SerializeField] private GameObject _remotePlayerAvatar;

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

    public void SpawnLocalPlayer(PlayerGender playerType)
    {
        if (!_avatars) return;

        var character = SpawnPlayer(AvatarIndex, playerType);
        _localPlayerAvatar = character;
        DontDestroyOnLoad(character);
    } 
    
    public void SpawnRemotePlayer(int avatarIndex , PlayerGender playerType)
    {
        if (!_avatars) return;

        var character = SpawnPlayer(avatarIndex, playerType);
        _remotePlayerAvatar = character;
    }

    private GameObject SpawnPlayer(int avatarIndex, PlayerGender playerType)
    {
        var prefab = playerType == PlayerGender.Male ? _avatars.MalePrefabs[avatarIndex] : _avatars.FemalePrefabs[avatarIndex];

        return Instantiate(prefab);
    }



    

    

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

}
