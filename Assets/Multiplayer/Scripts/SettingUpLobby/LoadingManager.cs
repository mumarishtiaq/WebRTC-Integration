using TMPro;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [SerializeField] private GameObject _animationObj;
    [SerializeField] private TextMeshProUGUI _loadingMsg;
    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void EnableLoading(string msg = "Loading ...",bool isDisableAfterInterval = false,float interval = 2f)
    {
        _animationObj.SetActive(true);
        _loadingMsg.text = msg;

        if(isDisableAfterInterval)
            Invoke(nameof(DisableLoading),interval);
    }
    public void DisableLoading()
    {
        _animationObj.SetActive(false);
    }


    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
