using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    public Button rockButton, paperButton, scissorsButton;
    public TMP_Text resultText;
    public TMP_Text playerChoiceText;
    public TMP_Text opponentChoiceText;
    public TMP_Text timerText;


    public Button hostBtn, clientBtn;

    private bool hasChosen = false;

    private void Awake()
    {
        Instance = this;

        rockButton.onClick.AddListener(() => MakeChoice(ChoiceType.Rock));
        paperButton.onClick.AddListener(() => MakeChoice(ChoiceType.Paper));
        scissorsButton.onClick.AddListener(() => MakeChoice(ChoiceType.Scissors));


        hostBtn.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
        clientBtn.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
    }

    public void StartCountdown(float time)
    {
        hasChosen = false;
        resultText.text = "";
        playerChoiceText.text = "";
        opponentChoiceText.text = "";
        timerText.text = $"{time:0.0}s";
    }

    public void UpdateCountdown(float time)
    {
        timerText.text = $"{Mathf.Max(0f, time):0.0}s";
    }

    private void MakeChoice(ChoiceType choice)
    {
        if (hasChosen) return;

        hasChosen = true;
        GameNetworkManager.Instance.SubmitChoiceServerRpc(choice);
        resultText.text = "Waiting for opponent...";
    }

    public void DisplayResult(string result, ulong p1Id, ChoiceType p1Choice, ulong p2Id, ChoiceType p2Choice)
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;

        if (localId == p1Id)
        {
            playerChoiceText.text = $"You chose: {p1Choice}";
            opponentChoiceText.text = $"Opponent chose: {p2Choice}";
        }
        else
        {
            playerChoiceText.text = $"You chose: {p2Choice}";
            opponentChoiceText.text = $"Opponent chose: {p1Choice}";
        }

        resultText.text = result;
        timerText.text = "";
    }
}
