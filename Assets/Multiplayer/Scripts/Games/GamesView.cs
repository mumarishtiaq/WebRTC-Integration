using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GamesView : TimerController
{
    [Header("References")]
    [SerializeField] private Image _bg;
    [SerializeField] private Transform _window;


    private float _openDuration = 0.7f;
    private float _closeDuration = 0.6f;


    [Header("Game Buttons")]
    [SerializeField] private Button _game1Btm;
    [SerializeField] private Button _game2Btm;
    [SerializeField] private Button _game3Btm;

    


    private void Start()
    {
        ResetToDefault();
    }

    private void OnEnable()
    {
        _game1Btm.onClick.RemoveAllListeners();
        _game2Btm.onClick.RemoveAllListeners();
        _game3Btm.onClick.RemoveAllListeners();

        _game1Btm.onClick.AddListener(() => OnGamePlayButtonClicked(GameType.Tik_Tak_Toe));
        _game2Btm.onClick.AddListener(() => OnGamePlayButtonClicked(GameType.Bubble_Shooter));
        _game3Btm.onClick.AddListener(() => OnGamePlayButtonClicked(GameType.Chess));
    }
    private void OnDisable()
    {
        _game1Btm.onClick.RemoveAllListeners();
        _game2Btm.onClick.RemoveAllListeners();
        _game3Btm.onClick.RemoveAllListeners();
    }


    [ContextMenu("Open")]
   public void Open()
    {
        ResetToDefault();
        Fade(0.64f, _openDuration);
        Scale(Vector3.one, _openDuration, Ease.InOutBack);
    }

    [ContextMenu("Close")]
    public void Close()
    {
        DOTween.KillAll();
        Fade(0, _closeDuration);
        Scale(Vector3.zero, _closeDuration, Ease.InBack);
    }


    private void Fade(float endValue = 0,float duration = 0)
    {
        _bg.DOFade(endValue, duration);
    }
    
    private void Scale(Vector3 endValue, float duration = 0,Ease ease = Ease.InBack)
    {
        _window.DOScale(endValue, duration).SetEase(ease);
    }

    private void ResetToDefault()
    {
        DOTween.KillAll();
        Fade();
        Scale(Vector3.zero);
    }

    public void OnGamePlayButtonClicked(GameType gameType)
    {
        string readableName = gameType.ToString().Replace("_", " ");
        Debug.Log("Selected Game Type: " + readableName);

        _timer.StartTimer(10f,
    onComplete: () => OnTimerComplete(),
    onProgress: (remaining) => TimerProgress(remaining)
 );
    }

    private void OnTimerComplete()
    {

    }
    private void TimerProgress(float progress)
    {

    }
}

public enum GameType
{
    None = -1,
    Tik_Tak_Toe = 0,
    Bubble_Shooter = 1,
    Chess = 2
}

