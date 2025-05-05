using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuSceneView : SceneViewBase
{
    [SerializeField] private Button _joinBtn;
    [SerializeField] private Button _exitBtn;

    [SerializeField] private GameObject _waitingForOtherPlayerObj;
    [SerializeField] private GameObject _waitingForOtherPlayerAnimation;

    


    public Button JoinBtn { get => _joinBtn; }

    private void Start()
    {

        _waitingForOtherPlayerObj.SetActive(false);
        _waitingForOtherPlayerAnimation.SetActive(false);
    }

    public void WaitingForOtherPlayer(bool isWaiting)
    {
        _joinBtn.interactable = !isWaiting;
        _waitingForOtherPlayerObj.SetActive(isWaiting);
        _waitingForOtherPlayerAnimation.SetActive(isWaiting);
        Debug.Log("In WaitingForOtherPlayer");
    }

    
}
