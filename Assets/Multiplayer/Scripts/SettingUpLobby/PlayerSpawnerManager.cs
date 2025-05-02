using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnerManager : NetworkBehaviour
{
    [SerializeField] private PlayerType _localPlayerType;

    [Header("Male And Female Prefabs")]
    [SerializeField] private GameObject _malePrefab;
    [SerializeField] private GameObject _femalePrefabs;


    [Header("Male And Female Spawn Positions")]
    [SerializeField] private Transform _hostSpawnPos;
    [SerializeField] private Transform _clientSpawnPos;

    private Transform _playerParent;




    public override void OnNetworkSpawn()
    {
        Debug.Log($"local client id : {NetworkManager.Singleton.LocalClientId}");

        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            _localPlayerType = PlayerType.Male;
        }
        else
        {
            _localPlayerType = PlayerType.Female;
        }



        SpawnPrefabRpc(GetLocalPlayerType());
    }

    //[ContextMenu("Spawn")]

    [Rpc(SendTo.Server)]

    private void SpawnPrefabRpc(PlayerType localPlayerType)
    {

        var prefab = localPlayerType == PlayerType.Male?_malePrefab:_femalePrefabs;
        var spawnPos = localPlayerType == PlayerType.Male? _hostSpawnPos : _clientSpawnPos;


        GameObject character = Instantiate(prefab, spawnPos.position, Quaternion.identity);
        var netObj = character.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(true);
        }
    }


    public PlayerType GetLocalPlayerType()
    {
        return _localPlayerType;
    }
}

public enum PlayerType
{
    None,
    Male,
    Female
}

