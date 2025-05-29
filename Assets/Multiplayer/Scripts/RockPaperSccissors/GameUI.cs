using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using DG.Tweening;

namespace Games.RockPaperScissors
{
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance;

        [Header("Gameplay Buttons")]
        public Button rockButton;
        public Button paperButton;
        public Button scissorsButton;
        public Button rematchButton;

        [Header("Connection Buttons")]
        public Button hostBtn;
        public Button clientBtn;

        [Header("UI Texts")]
        public TMP_Text resultText;
        public TMP_Text _waitingforOtherPlayerTxt;
        public TMP_Text playerChoiceText;
        public TMP_Text opponentChoiceText;


        [Header("Score Texts")]
        public TMP_Text playersScoreText;
        
        [Header("Choices Parent")]
        public Transform _choicesParent; 
        
        [Header("Choices Parent")]
        public GameObject _localPlayerCheckMark;
        public GameObject _remotePlayerCheckMark;


        private bool hasChosen = false;


        private void Awake()
        {
            Instance = this;

            // Game input buttons
            rockButton.onClick.AddListener(() => MakeChoice(ChoiceType.Rock));
            paperButton.onClick.AddListener(() => MakeChoice(ChoiceType.Paper));
            scissorsButton.onClick.AddListener(() => MakeChoice(ChoiceType.Scissors));
            rematchButton.onClick.AddListener(RequestRematch);

            // Host/Client connection buttons
            hostBtn.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartHost();
                HideConnectionButtons();
            });

            clientBtn.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartClient();
                HideConnectionButtons();
            });


        }

        private void Start()
        {

            GameNetworkManager.Instance.player1Score.OnValueChanged += (oldValue, newValue) =>
            {
                UpdateScores();
            };
            
            GameNetworkManager.Instance.player2Score.OnValueChanged += (oldValue, newValue) =>
            {
                UpdateScores();
            };

            SetScoreOnCombine("0", "0");
        }

        private void MakeChoice(ChoiceType choice)
        {
            if (hasChosen) return;

            hasChosen = true;

            _localPlayerCheckMark.SetActive(true);

            _choicesParent.Scale(Vector3.zero,0.6f,Ease.InBack);
            
            if(!_localPlayerCheckMark.activeInHierarchy || !_remotePlayerCheckMark.activeInHierarchy)
                _waitingforOtherPlayerTxt.transform.Scale(Vector3.one, 1f, Ease.OutBack);


            GameNetworkManager.Instance.SubmitChoiceServerRpc(choice);
        }

        public void SetOpponentMadeChoice()
        {
            _remotePlayerCheckMark.SetActive(true);
        }

        private void RequestRematch()
        {
            rematchButton.gameObject.SetActive(false);
            GameNetworkManager.Instance.RequestRematchServerRpc();
        }

        public void DisplayResult(MatchResultData resultData)
        {
            ulong localId = NetworkManager.Singleton.LocalClientId;


            playerChoiceText.text = $"{resultData.localPlayer.name} choose: {resultData.localPlayer.choice}";
            opponentChoiceText.text = $"{resultData.remotePLayer.name} choose: {resultData.remotePLayer.choice}";


            resultText.text = resultData.result;
            resultText.transform.Scale(Vector3.one, 0.6f, Ease.OutBack);

            if(_waitingforOtherPlayerTxt.transform.localScale != Vector3.zero)
                _waitingforOtherPlayerTxt.transform.Scale(Vector3.zero, 0.2f, Ease.InBack);


            rematchButton.gameObject.SetActive(true);
        }

        private void SetScoreOnCombine(string local , string remote)
        {
            playersScoreText.text = $"{local}   -   {remote}";
        }

        public void ResetUIForNewRound()
        {
            hasChosen = false;
            resultText.text = "";
            playerChoiceText.text = "";
            opponentChoiceText.text = "";
            rematchButton.gameObject.SetActive(false);

            _choicesParent.Scale(Vector3.one);
            resultText.transform.Scale(Vector3.zero);
            _waitingforOtherPlayerTxt.transform.Scale(Vector3.zero);


            _localPlayerCheckMark.SetActive(false);
            _remotePlayerCheckMark.SetActive(false);
        }

        private void UpdateScores()
        {
            GameNetworkManager.Instance.GetScores(out int player1Score, out int player2Score);

            var localPlayerId = GameNetworkManager.Instance.GetLocalPlayerID();
            var hostId = GameNetworkManager.Instance.GetHostPlayerId();
            bool isLocalPlayerFirst = hostId == localPlayerId;

            var localScore = isLocalPlayerFirst ? player1Score : player2Score;
            var remoteScore = isLocalPlayerFirst ? player2Score : player1Score;

            SetScoreOnCombine(localScore.ToString(), remoteScore.ToString());

        }

        private void HideConnectionButtons()
        {
            hostBtn.gameObject.SetActive(false);
            clientBtn.gameObject.SetActive(false);
        }
    }
}
