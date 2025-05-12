using UnityEngine;

public class LobbySceneManager : MonoBehaviour
{
    [SerializeField] private LobbySceneView _sceneView;
    private PeerData _peerData => MultiplayerManager.Instance.PeerData;

    private void Start()
    {
        _sceneView.SetPlayerData(_peerData.LP.Name);
        _sceneView.SetRemotePlayerData(_peerData.RP.Name);


        SpawnManager.Instance.SpawnRemotePlayer(LobbyManager.Instance.GetRemotePlayerAvatarIndex(), _peerData.RP.Gender);
        SpawnManager.Instance.SetTransform_LocalPlayer();
        SpawnManager.Instance.TriggerAnimations(ParticipantType.Remote, AnimationType.Sit);
        SpawnManager.Instance.TriggerAnimations(ParticipantType.Local, AnimationType.Sit);



    }
}
