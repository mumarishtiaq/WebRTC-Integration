using System;
using UnityEngine;


public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    public PlayerData _playerData;

    [SerializeField] private SpawnManager _spawnManager;
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
        try
        {
            LoadingManager.Instance.EnableLoading("Loading Player");
            DontDestroyOnLoad(gameObject);
            await AuthenticationManager.SignInAnonymously(_playerData.Name);

            _spawnManager.SpawnPlayer(_playerData.Type);
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

    [ContextMenu("Populate")]
    private void PopulateDummyForSecondInstance()
    {
        _playerData = new PlayerData { 
            Id = "defg",
            Name = "Marry",
            Type = PlayerType.Female,
            ChannelName = "John_Marry"
        };
    }
}
[Serializable]
public class PlayerData
{
    public string Id;
    public string Name;
    public PlayerType Type;
    public string ChannelName;
}

