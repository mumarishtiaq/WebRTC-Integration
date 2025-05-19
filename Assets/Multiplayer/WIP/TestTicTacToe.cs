using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestTicTacToe : MonoBehaviour
{
    [ContextMenu("LoadScene")]
    private void LoadScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("TicTacToeScene", LoadSceneMode.Single);
    }
    
    [ContextMenu("Debug")]
    private void DebugTest()
    {
        if(NetworkManager.Singleton.IsHost)
        {

        Debug.Log($" Count {NetworkManager.Singleton.ConnectedClients.Count}");
        }
    }
}
