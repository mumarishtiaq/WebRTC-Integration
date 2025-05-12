using TMPro;
using UnityEngine;

public class LobbySceneView : SceneViewBase
{
    [Header("Remote Player Data Holder")]
    [SerializeField]
    GameObject remotePayerDataHolder;
    [SerializeField]
    TextMeshProUGUI remotePlayerNameText;

    [SerializeField]
    GameObject _gamesPanel;




    

    public void SetRemotePlayerData(string playerName)
    {
        remotePayerDataHolder.SetActive(true);
        remotePlayerNameText.text = $"{playerName}";
    }

    public void OnGameButtonClicked()
    {

    }
}
