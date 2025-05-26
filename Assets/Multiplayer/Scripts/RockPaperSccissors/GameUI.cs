using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    public Button rockButton, paperButton, scissorsButton;
    public TMP_Text resultText;

    private void Awake()
    {
        Instance = this;

        rockButton.onClick.AddListener(() => MakeChoice(ChoiceType.Rock));
        paperButton.onClick.AddListener(() => MakeChoice(ChoiceType.Paper));
        scissorsButton.onClick.AddListener(() => MakeChoice(ChoiceType.Scissors));
    }

    void MakeChoice(ChoiceType choice)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            GameNetworkManager.Instance.SubmitChoiceServerRpc(choice);
            resultText.text = "Waiting for other player...";
        }
    }

    public void DisplayResult(string result)
    {
        resultText.text = result;
    }
}
