using Unity.Netcode;
using UnityEngine;

namespace Games.TicTacToe
{
    public class GameNetworkManager : NetworkBehaviour
    {
        //public static GameNetworkManager Instance { get; private set; }

        //[SerializeField] private Transform crossPrefab;
        //[SerializeField] private Transform circlePrefab;
        //[SerializeField] private Transform lineCompletePrefab;

        //private void Awake()
        //{
        //    Instance = this;
        //}

        //[Rpc(SendTo.Server)]
        //public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
        //{
        //    GameManager.Instance.ProcessGridClick(x, y, playerType);
        //}

        //[Rpc(SendTo.Server)]
        //public void RematchRpc()
        //{
        //    GameManager.Instance.Rematch();
        //}

        //public void SpawnPlayerVisual(int x, int y, PlayerType playerType)
        //{
        //    Transform prefab = playerType == PlayerType.Cross ? crossPrefab : circlePrefab;
        //    Transform spawned = Instantiate(prefab, GameVisualManager.GetGridWorldPositionStatic(x, y), Quaternion.identity);
        //    spawned.GetComponent<NetworkObject>().Spawn(true);
        //}

        //public void SpawnWinningLine(GameManager.Line line)
        //{
        //    float eulerZ = 0f;
        //    switch (line.orientation)
        //    {
        //        case GameManager.Orientation.Vertical: eulerZ = 90f; break;
        //        case GameManager.Orientation.DiagonalA: eulerZ = 45f; break;
        //        case GameManager.Orientation.DiagonalB: eulerZ = -45f; break;
        //    }

        //    Transform lineTransform = Instantiate(lineCompletePrefab, GameVisualManager.GetGridWorldPositionStatic(line.centerGridPosition.x, line.centerGridPosition.y), Quaternion.Euler(0, 0, eulerZ));
        //    lineTransform.GetComponent<NetworkObject>().Spawn(true);
        //}
    }
}
