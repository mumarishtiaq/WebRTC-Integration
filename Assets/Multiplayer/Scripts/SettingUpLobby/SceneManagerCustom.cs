using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneManagerCustom 
{
    public static void LoadScene(SceneType sceneType)
    {
        var sceneName = GetSceneName(sceneType);

        if(sceneName !=string.Empty)
            SceneManager.LoadScene(sceneName);
    }
    
    public static async Task LoadSceneAsync(SceneType sceneType)
    {
        var sceneName = GetSceneName(sceneType);

        if(sceneName !=string.Empty)
            await SceneManager.LoadSceneAsync(sceneName);
    }

    public static void LoadNetworkScene(SceneType sceneType)
    {
        var sceneName = GetSceneName(sceneType);

        if (sceneName != string.Empty)
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

        else
            Debug.LogWarning($"Invalid or empty Scene Name {sceneName}");

    }

    private static string GetSceneName(SceneType sceneType)
    {
        string sceneName = string.Empty;

        switch (sceneType)
        {
            case SceneType.MainMenu:
                sceneName = "MainMenuScene";
                break;
            case SceneType.Lobby:
                sceneName = "LobbyScene";
                break;
            case SceneType.CoinRush:
                sceneName = "CoinRushScene";
                break;
            case SceneType.TicTacToe:
                sceneName = "TicTacToeScene";
                break;
            case SceneType.TicTacToeTest:
                sceneName = "TicTacToeTestScene";
                break;
            default:
                Debug.LogWarning("Un handled Scene type");
                break;
        }

        return sceneName;
    }
}



public enum SceneType
{
    None = -1,
    MainMenu = 0,
    Lobby = 1,
    CoinRush = 2,
    TicTacToe = 3,
    ThirdGame = 4,
    TicTacToeTest = 5,

}



