using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Games.TicTacToe
{
    public class GameManager : NetworkBehaviour
    {


        public static GameManager Instance { get; private set; }


        public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
        public class OnClickedOnGridPositionEventArgs : EventArgs
        {
            public int x;
            public int y;
            public PlayerType playerType;
        }
        public event EventHandler OnGameStarted;
        public event EventHandler<OnGameWinEventArgs> OnGameWin;
        public class OnGameWinEventArgs : EventArgs
        {
            public Line line;
            public PlayerType winPlayerType;
        }
        public event EventHandler OnCurrentPlayablePlayerTypeChanged;
        public event EventHandler OnRematch;
        public event EventHandler OnGameTied;
        public event EventHandler OnScoreChanged;
        public event EventHandler OnPlacedObject;


        public enum PlayerType
        {
            None,
            Cross,
            Circle,
        }

        public enum Orientation
        {
            Horizontal,
            Vertical,
            DiagonalA,
            DiagonalB,
        }

        public struct Line
        {
            public List<Vector2Int> gridVector2IntList;
            public Vector2Int centerGridPosition;
            public Orientation orientation;
        }


        public PlayerType localPlayerType;
        private NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();
        private PlayerType[,] playerTypeArray;
        private List<Line> lineList;
        private NetworkVariable<int> playerCrossScore = new NetworkVariable<int>();
        private NetworkVariable<int> playerCircleScore = new NetworkVariable<int>();


        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("More than one GameManager instance!");
            }
            Instance = this;

            playerTypeArray = new PlayerType[3, 3];

            lineList = new List<Line> {
            // Horizontal
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), },
                centerGridPosition = new Vector2Int(1, 0),
                orientation = Orientation.Horizontal,
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1), },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Horizontal,
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2), },
                centerGridPosition = new Vector2Int(1, 2),
                orientation = Orientation.Horizontal,
            },

            // Vertical
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2), },
                centerGridPosition = new Vector2Int(0, 1),
                orientation = Orientation.Vertical,
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2), },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Vertical,
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(2,0), new Vector2Int(2,1), new Vector2Int(2,2), },
                centerGridPosition = new Vector2Int(2, 1),
                orientation = Orientation.Vertical,
            },

            // Diagonals
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(0,0), new Vector2Int(1,1), new Vector2Int(2,2), },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalA,
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(0,2), new Vector2Int(1,1), new Vector2Int(2,0), },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalB,
            },
        };


            
           
        }


        private void Start()
        {
            Debug.Log("OnNetworkSpawn: " + NetworkManager.Singleton.LocalClientId);
            if (NetworkManager.Singleton.LocalClientId == 0)
            {
                localPlayerType = PlayerType.Cross;
            }
            else
            {
                localPlayerType = PlayerType.Circle;
            }

            if (IsServer)
            {
                //NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
                NetworkManager_OnClientConnectedCallback();
            }

            currentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
            {
                OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
            };

            playerCrossScore.OnValueChanged += (int prevScore, int newScore) =>
            {
                OnScoreChanged?.Invoke(this, EventArgs.Empty);
            };
            playerCircleScore.OnValueChanged += (int prevScore, int newScore) =>
            {
                OnScoreChanged?.Invoke(this, EventArgs.Empty);
            };
        }






        //public override void OnNetworkSpawn()
        //{
        //    Debug.Log("OnNetworkSpawn: " + NetworkManager.Singleton.LocalClientId);
        //    if (NetworkManager.Singleton.LocalClientId == 0)
        //    {
        //        localPlayerType = PlayerType.Cross;
        //    }
        //    else
        //    {
        //        localPlayerType = PlayerType.Circle;
        //    }

        //    if (IsServer)
        //    {
        //        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        //    }

        //    currentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        //    {
        //        OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        //    };

        //    playerCrossScore.OnValueChanged += (int prevScore, int newScore) =>
        //    {
        //        OnScoreChanged?.Invoke(this, EventArgs.Empty);
        //    };
        //    playerCircleScore.OnValueChanged += (int prevScore, int newScore) =>
        //    {
        //        OnScoreChanged?.Invoke(this, EventArgs.Empty);
        //    };
        //}

        private void NetworkManager_OnClientConnectedCallback(ulong obj = 1)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
            {
                // Start Game
                currentPlayablePlayerType.Value = PlayerType.Cross;
                //TriggerOnGameStartedRpc();
                StartCoroutine(TriggerOnGameStartedNextFrame());
                Debug.LogError($"NetworkManager_OnClientConnectedCallback , I am Host = {IsHost}");
            }
        }
        private IEnumerator TriggerOnGameStartedNextFrame()
        {
            yield return new WaitForSeconds(2);
            TriggerOnGameStartedRpc();
        }



        [Rpc(SendTo.ClientsAndHost)]
        private void TriggerOnGameStartedRpc()
        {
            OnGameStarted?.Invoke(this, EventArgs.Empty);
            Debug.LogError($"TriggerOnGameStartedRpc , I am Host = {IsHost}");

        }

        [Rpc(SendTo.Server)]
        public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
        {
            Debug.Log("ClickedOnGridPosition " + x + ", " + y);

            if (playerType != currentPlayablePlayerType.Value)
            {
                Debug.Log("in");
                return;
            }

            if (playerTypeArray[x, y] != PlayerType.None)
            {
                // Already occupied
                return;
            }

            playerTypeArray[x, y] = playerType;
            TriggerOnPlacedObjectRpc();

            OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
            {
                x = x,
                y = y,
                playerType = playerType,
            });

            switch (currentPlayablePlayerType.Value)
            {
                default:
                case PlayerType.Cross:
                    currentPlayablePlayerType.Value = PlayerType.Circle;
                    break;
                case PlayerType.Circle:
                    currentPlayablePlayerType.Value = PlayerType.Cross;
                    break;
            }

            TestWinner();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void TriggerOnPlacedObjectRpc()
        {
            OnPlacedObject?.Invoke(this, EventArgs.Empty);
        }

        private bool TestWinnerLine(Line line)
        {
            return TestWinnerLine(
                playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
                playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
                playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
            );
        }

        private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
        {
            return
                aPlayerType != PlayerType.None &&
                aPlayerType == bPlayerType &&
                bPlayerType == cPlayerType;
        }

        private void TestWinner()
        {
            for (int i = 0; i < lineList.Count; i++)
            {
                Line line = lineList[i];
                if (TestWinnerLine(line))
                {
                    // Win!
                    Debug.Log("Winner!");
                    currentPlayablePlayerType.Value = PlayerType.None;
                    PlayerType winPlayerType = playerTypeArray[line.centerGridPosition.x, line.centerGridPosition.y];
                    switch (winPlayerType)
                    {
                        case PlayerType.Cross:
                            playerCrossScore.Value++;
                            break;
                        case PlayerType.Circle:
                            playerCircleScore.Value++;
                            break;
                    }
                    TriggerOnGameWinRpc(i, winPlayerType);
                    break;
                }
            }

            bool hasTie = true;
            for (int x = 0; x < playerTypeArray.GetLength(0); x++)
            {
                for (int y = 0; y < playerTypeArray.GetLength(1); y++)
                {
                    if (playerTypeArray[x, y] == PlayerType.None)
                    {
                        hasTie = false;
                        break;
                    }
                }
            }

            if (hasTie)
            {
                TriggerOnGameTiedRpc();
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void TriggerOnGameTiedRpc()
        {
            OnGameTied?.Invoke(this, EventArgs.Empty);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void TriggerOnGameWinRpc(int lineIndex, PlayerType winPlayerType)
        {
            Line line = lineList[lineIndex];
            OnGameWin?.Invoke(this, new OnGameWinEventArgs
            {
                line = line,
                winPlayerType = winPlayerType,
            });
        }

        [Rpc(SendTo.Server)]
        public void RematchRpc()
        {
            for (int x = 0; x < playerTypeArray.GetLength(0); x++)
            {
                for (int y = 0; y < playerTypeArray.GetLength(1); y++)
                {
                    playerTypeArray[x, y] = PlayerType.None;
                }
            }
            currentPlayablePlayerType.Value = PlayerType.Cross;

            TriggerOnRematchRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void TriggerOnRematchRpc()
        {
            OnRematch?.Invoke(this, EventArgs.Empty);
        }

        public PlayerType GetLocalPlayerType()
        {
            return localPlayerType;
        }

        public PlayerType GetCurrentPlayablePlayerType()
        {
            return currentPlayablePlayerType.Value;
        }

        public void GetScores(out int playerCrossScore, out int playerCircleScore)
        {
            playerCrossScore = this.playerCrossScore.Value;
            playerCircleScore = this.playerCircleScore.Value;
        }

        [ContextMenu("LeaveGame")]
        public void LeaveGame()
        {
            if (IsHost)
            {
                HostLeaveGame();
            }
            else if (IsClient)
            {
                ClientLeaveGameServerRpc();
                ShutdownAndReturnToLobby(); // Only for the client side
            }
        }

        // ----- Host Leave -----
        private void HostLeaveGame()
        {
            Debug.Log("Host is leaving the game (server remains active).");
            NotifyClientHostLeftClientRpc();
            LoadLobbySceneLocally();
        }

        [ClientRpc]
        private void NotifyClientHostLeftClientRpc()
        {
            if (!IsHost)
            {
                Debug.LogWarning($"Host has left the game. ");
                // Optional: Show popup
                //GameOverUI.Instance?.ShowHostLeftMessage();
                LoadLobbySceneLocally();
            }
        }

        // ----- Client Leave -----
        [ServerRpc(RequireOwnership = false)]
        private void ClientLeaveGameServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            Debug.Log($"Client {clientId} has left the game.");
            // Optional: Handle server-side cleanup if needed

            //Unload scene
            //LoadLobbySceneLocally();

        }

        private void ShutdownAndReturnToLobby()
        {
            NetworkManager.Singleton.Shutdown();
            LoadLobbySceneLocally();
        }

        // ----- Shared -----
        private async void LoadLobbySceneLocally()
        {
            //show popup
            await SceneManager.LoadSceneAsync("TicTacToeTestScene");
            //close popup
        }


    }
}