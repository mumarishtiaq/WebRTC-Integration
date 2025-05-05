using System;
using System.Threading.Tasks;
using UnityEngine;


public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    [SerializeField] private EnvironmentType _currentEnvironment;

    public PeerData PeerData;

    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private MenuSceneManager _menuSceneManager;

    public bool isInitialized { get; private set; }


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


        if(!_spawnManager)
            _spawnManager = GetComponent<SpawnManager>();

    }
    async void Start()
    {
        if (_currentEnvironment == EnvironmentType.Development)
        {
            // Show popup and wait for selection
            await ShowDevelopmentPopupAndWait();
        }

        await InitializeMultiplayer();
    }

    private async Task ShowDevelopmentPopupAndWait()
    {
        Debug.Log("Development mode detected. Showing player selection popup...");
        LoadingManager.Instance.DisableLoading();
        // Show your popup UI
        DevelopmentPopupUI.Instance.ShowPopup();

        // Wait until player makes a selection
        bool isHost = await DevelopmentPopupUI.Instance.WaitForPlayerSelection();

        // Populate dummy data based on selection
        PeerData = await PlayerDataFetcher.PopulateDummyData(isHost);
    }

    private async Task InitializeMultiplayer()
    {
        try
        {
            _menuSceneManager.DeActivateMainMenuUI();
            LoadingManager.Instance.EnableLoading("Loading Player");
            DontDestroyOnLoad(gameObject);

            if(_currentEnvironment == EnvironmentType.Release)
                PeerData = await PlayerDataFetcher.FetchDataFromApp();


            await AuthenticationManager.SignInAnonymously(PeerData.LP.Name);

            // Check that scene has not been unloaded while processing async wait to prevent throw.
            if (this == null) return;

            isInitialized = true;

            _menuSceneManager.ActivateMainMenuUI(PeerData.LP.Name);
            _spawnManager.SpawnPlayer(PeerData.LP.Gender);
            LoadingManager.Instance.DisableLoading();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
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



public enum EnvironmentType
{
    Development,
    Release
}

