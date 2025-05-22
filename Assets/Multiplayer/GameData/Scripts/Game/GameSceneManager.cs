using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Games.CoinRush
{
    public class GameSceneManager : MonoBehaviour
    {
        [field: SerializeField]
        public GameSceneView sceneView { get; private set; }

        private PeerData _peerData => MultiplayerManager.Instance.PeerData;



        [SerializeField]
        GameNetworkManager gameNetworkManagerPrefab;

        public static GameSceneManager instance { get; private set; }

        public bool didPlayerPressLeaveButton { get; private set; }

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

        void Start()
        {
            LobbyManager.OnGameStarted?.Invoke();

            if (MultiplayerManager.Instance == null)
            {
                Debug.LogError("Please be sure to start Play mode on the MainMenu scene.");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif

                return;
            }

            ShowArenaPanel();

            // The host instantiates the Game Manager which will control game play throughout this scene
            if (LobbyManager.Instance.isHost)
            {
                GameNetworkManager.Instantiate(gameNetworkManagerPrefab);
            }

            // Since client scenes are actually loaded after the Game Manager is instantiated on host and propagated
            // to all clients, call method to ensure all scores are updated so players list will be visible from start.
            UpdateScores();


        }

        public void SetCountdown(int seconds)
        {
            sceneView.arenaUiOverlayPanelView.ShowCountdown();
            sceneView.arenaUiOverlayPanelView.SetCountdown(seconds);
        }

        public void HideCountdown()
        {
            sceneView.arenaUiOverlayPanelView.HideCountdown();
        }

        public void ShowGameTimer(int seconds)
        {
            sceneView.arenaUiOverlayPanelView.ShowGameTimer(seconds);
        }

        public void UpdateScores()
        {
            sceneView.UpdateScores();
        }

        void ShowArenaPanel()
        {
            sceneView.ShowArenaPanel();

            //sceneView.SetProfileDropdownIndex(ServerlessMultiplayerGameSampleManager.instance.profileDropdownIndex);

            sceneView.SetPlayerData(_peerData.LP.Name);

            ShowInitialGameTime();

            sceneView.SetInteractable(true);
        }

        void ShowInitialGameTime()
        {
            sceneView.arenaUiOverlayPanelView.ShowGameTimer((int)GameConfig.gameDuration);
        }

        public void OnGameOver(GameResultsData results)
        {
            // Update player stats so they're available for the results Panel.
            // Note that we do not need to wait for async to finish writing as they won't be needed again until the
            // end of the next game anyway.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            //CloudSaveManager.instance.UpdatePlayerStats(results);
#pragma warning restore CS4014

            // Save off game results so they can be shown when we return to the main menu.
            // Note: This simplifies exiting the game since it can be gracefully-destructed right now without having
            // to worry about whether the host or client leaves first.

            //TODO GameResults
            //ServerlessMultiplayerGameSampleManager.instance.SetPreviousGameResults(results);
            Debug.LogError("Game scene Manager OnGameOver");

            sceneView.ShowGameResults(results);

        }

        public void OnGameLeaveButtonPressed()
        {
            didPlayerPressLeaveButton = true;

            if(!LobbyManager.Instance.isHost)
                NetworkServiceManager.Instance.Uninitialize();


            GameEndManager.instance.ReturnToLobbyScene();
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
