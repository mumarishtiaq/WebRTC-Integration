using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnerManager : NetworkBehaviour
{
    [SerializeField] private PlayerGender _localPlayerType;

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
            _localPlayerType = PlayerGender.Male;
        }
        else
        {
            _localPlayerType = PlayerGender.Female;
        }



        SpawnPrefabRpc(GetLocalPlayerType());
    }

    //[ContextMenu("Spawn")]

    [Rpc(SendTo.Server)]

    private void SpawnPrefabRpc(PlayerGender localPlayerType)
    {

        var prefab = localPlayerType == PlayerGender.Male?_malePrefab:_femalePrefabs;
        var spawnPos = localPlayerType == PlayerGender.Male? _hostSpawnPos : _clientSpawnPos;


        GameObject character = Instantiate(prefab, spawnPos.position, Quaternion.identity);
        var netObj = character.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(true);
        }
    }


    public PlayerGender GetLocalPlayerType()
    {
        return _localPlayerType;
    }
}



