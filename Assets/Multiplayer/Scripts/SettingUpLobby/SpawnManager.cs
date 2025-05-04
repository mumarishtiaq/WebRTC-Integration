using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    

    [Header("Avatars Data")]
    [SerializeField] private AvatarsData _avatars;


    [Header("Male And Female Spawn Positions")]
    [SerializeField] private Transform _hostSpawnPos;
    [SerializeField] private Transform _clientSpawnPos;

    private Transform _playerParent;


    public void SpawnPlayer(PlayerType playerType)
    {
        if (!_avatars) return;

        var prefab = playerType == PlayerType.Male ? _avatars.MalePrefabs[0] : _avatars.FemalePrefabs[0];

        GameObject character = Instantiate(prefab);
    }

}
