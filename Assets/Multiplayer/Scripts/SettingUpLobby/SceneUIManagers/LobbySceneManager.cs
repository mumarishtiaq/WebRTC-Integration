using UnityEngine;

public class LobbySceneManager : MonoBehaviour
{
    [SerializeField] private LobbySceneView _sceneView;
    private PeerData _peerData => MultiplayerManager.Instance.PeerData;

    private void Start()
    {
        _sceneView.SetPlayerData(_peerData.LP.Name);
        _sceneView.SetRemotePlayerData(_peerData.RP.Name);



    }
}
