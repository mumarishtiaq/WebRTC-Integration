using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Games.TicTacToe
{
    public class GameOverUI : MonoBehaviour
    {


        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI resultTextMesh;
        [SerializeField] private Color winColor;
        [SerializeField] private Color loseColor;
        [SerializeField] private Color tieColor;
        [SerializeField] private Button rematchButton;
        [SerializeField] private Transform opponentLeftPopup;
        [SerializeField] private TextMeshProUGUI opponentLeftTxt;
        [SerializeField] private Button leaveButton;


        public static GameOverUI instance;


        private void Awake()
        {
            rematchButton.onClick.AddListener(() =>
            {
                GameManager.Instance.RematchRpc();
            });
            instance = this;
        }
        private void Start()
        {
            GameManager.Instance.OnGameWin += GameManager_OnGameWin;
            GameManager.Instance.OnRematch += GameManager_OnRematch;
            GameManager.Instance.OnGameTied += GameManager_OnGameTied;

            Hide();
            opponentLeftPopup.DOScale(0, 0);
            leaveButton.interactable = true;



        }

        public void OnLeft(string opponentName)
        {
            opponentLeftTxt.text = opponentName + " has left the game";
            opponentLeftPopup.DOScale(1, 0.5f);
            leaveButton.interactable = false;
        }

        private void GameManager_OnGameTied(object sender, System.EventArgs e)
        {
            resultTextMesh.text = "TIE!";
            resultTextMesh.color = tieColor;
            Show();
        }

        private void GameManager_OnRematch(object sender, System.EventArgs e)
        {
            Hide();
        }

        private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
        {
            if (e.winPlayerType == GameManager.Instance.GetLocalPlayerType())
            {
                resultTextMesh.text = "YOU WIN!";
                resultTextMesh.color = winColor;
            }
            else
            {
                resultTextMesh.text = "YOU LOSE!";
                resultTextMesh.color = loseColor;
            }
            Show();
        }

        private void Show()
        {
            panel.SetActive(true);
        }

        private void Hide()
        {
            panel.SetActive(false);
        }
    }
}