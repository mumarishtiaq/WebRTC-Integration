using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DevelopmentPopupUI : MonoBehaviour
{
    public static DevelopmentPopupUI Instance;

    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private TaskCompletionSource<bool> selectionSource;

    private void Awake()
    {
        Instance = this;
        popupPanel.SetActive(false);
    }

    public void ShowPopup()
    {
        popupPanel.SetActive(true);

        hostButton.onClick.AddListener(() => SelectPlayer(true));
        clientButton.onClick.AddListener(() => SelectPlayer(false));
    }

    public Task<bool> WaitForPlayerSelection()
    {
        selectionSource = new TaskCompletionSource<bool>();
        return selectionSource.Task;
    }

    private void SelectPlayer(bool isHost)
    {
        popupPanel.SetActive(false);
        selectionSource.TrySetResult(isHost);
    }
}
