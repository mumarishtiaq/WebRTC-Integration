using System.Collections.Generic;
using UnityEngine;

namespace Games.CoinRush
{
    [DisallowMultipleComponent]
    public class GameCoinManager : MonoBehaviour
    {
        public static GameCoinManager instance;

        [SerializeField]
        Coin coinPrefab;

        [SerializeField]
        float m_ArenaRadius = 50;

        [SerializeField]
        float m_SpawnCoinAreaMultiplier = .85f;

        [SerializeField]
        float m_SpawnCoinPositionRandomization = 3;

        bool m_IsHost;

        float m_NextCoinSpawnTime = float.MaxValue;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
            }
        }

        public void Initialize(bool isHost)
        {
            m_IsHost = isHost;
        }

        public void StartTimerToSpawnCoins()
        {
            m_NextCoinSpawnTime = Time.time + GameConfig.initialSpawnDelay;
        }

        public void HostHandleSpawningCoins()
        {
            if (Time.time >= m_NextCoinSpawnTime)
            {
                HostSpawnNextCoinWave();
            }
        }

        public void HostSpawnNextCoinWave()
        {
            m_NextCoinSpawnTime = Time.time + GameConfig.spawnInterval;

            SpawnCoinClusters(1, GameConfig.cluster1);
            SpawnCoinClusters(2, GameConfig.cluster2);
            SpawnCoinClusters(3, GameConfig.cluster3);
        }

        void SpawnCoinClusters(int numInCluster, int numClusters)
        {
            for (int i = 0; i < numClusters; i++)
            {
                SpawnCoinCluster(numInCluster);
            }
        }

        void SpawnCoinCluster(int numCoins)
        {
            var spawnCoinMax = m_ArenaRadius * m_SpawnCoinAreaMultiplier;
            var clusterPosition = new Vector3(UnityEngine.Random.Range(-spawnCoinMax, spawnCoinMax), 0,
                UnityEngine.Random.Range(-spawnCoinMax, spawnCoinMax));

            for (int i = 0; i < numCoins; i++)
            {
                SpawnCoin(clusterPosition);
            }
        }

        void SpawnCoin(Vector3 clusterPosition)
        {
            var position = clusterPosition;
            position.x += UnityEngine.Random.Range(-m_SpawnCoinPositionRandomization,
                m_SpawnCoinPositionRandomization);
            position.z += UnityEngine.Random.Range(-m_SpawnCoinPositionRandomization,
                m_SpawnCoinPositionRandomization);

            var coin = GameObject.Instantiate(coinPrefab);
            coin.transform.position = position;

            coin.networkObject.Spawn();

            coin.Initialize(GameConfig.destroyInterval);
        }

        public void CollectCoin(PlayerAvatar playerAvatar, Coin coin)
        {
            if (m_IsHost && coin.isCollectable)
            {
                coin.OnCollect();

                playerAvatar.ScorePointClientRpc();
            }
        }

        public void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
