using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

namespace Games.RockPaperScissors
{
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance;

        [Header("Gameplay Buttons")]
        public Button rockButton, paperButton, scissorsButton;
        public Button rematchButton;

        [Header("Connection Buttons")]
        public Button hostBtn, clientBtn;

        [Header("UI Texts")]
        public TMP_Text resultText;
        public TMP_Text playerChoiceText;
        public TMP_Text opponentChoiceText;


        [Header("Score Texts")]
        public TMP_Text playerScoreText;
        public TMP_Text opponentScoreText;


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
            playerScoreText.text = "0";
            opponentScoreText.text = "0";
        }

        private void MakeChoice(ChoiceType choice)
        {
            if (hasChosen) return;

            hasChosen = true;
            resultText.text = "Waiting for opponent...";
            GameNetworkManager.Instance.SubmitChoiceServerRpc(choice);
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

            playerScoreText.text = resultData.localPlayer.score.ToString();
            opponentScoreText.text = resultData.remotePLayer.score.ToString(); 


            resultText.text = resultData.result;
            rematchButton.gameObject.SetActive(true);
        }

        public void ResetUIForNewRound()
        {
            hasChosen = false;
            resultText.text = "";
            playerChoiceText.text = "";
            opponentChoiceText.text = "";
            rematchButton.gameObject.SetActive(false);
        }

        private void HideConnectionButtons()
        {
            hostBtn.gameObject.SetActive(false);
            clientBtn.gameObject.SetActive(false);
        }
    }
}
