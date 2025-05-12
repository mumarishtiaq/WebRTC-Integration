using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class TimerController : MonoBehaviour
{
    private float timer;
    private float duration;
    private bool timerRunning = false;

    

    private float nextSecondMark;

    public GameObject TimerObj;
    public Image TimerImage;
    public TextMeshProUGUI TimerTxt;



    void Update()
    {
        if (!timerRunning) return;

        timer -= Time.deltaTime;

        // Call progress callback once per second
        if (timer <= nextSecondMark)
        {
            nextSecondMark = Mathf.Floor(timer);
            //onProgressUpdate?.Invoke(Mathf.Clamp(timer, 0f, duration));
            var p = Mathf.Clamp(timer, 0f, duration);
            SetUI(p);
        }

        if (timer <= 0f)
        {
            timerRunning = false;
            TimerEnded();
        }
    }

    /// <summary>
    /// Starts the timer
    /// </summary>
    /// <param name="durationInSeconds">Total duration in seconds</param>
    /// <param name="onComplete">Callback when time ends (optional)</param>
    /// <param name="onProgress">Callback every second with remaining time (optional)</param>
    public void StartTimer(float durationInSeconds, Action onComplete = null, Action<float> onProgress = null)
    {
        duration = durationInSeconds;
        timer = durationInSeconds;
        timerRunning = true;
        //onTimerComplete = onComplete;
        //onProgressUpdate = onProgress;
        nextSecondMark = Mathf.Floor(timer); // Setup first second tick
        TimerObj.SetActive(true);

        Debug.Log($"Timer started for {durationInSeconds} seconds.");
    }

    private void SetUI(float progress)
    {
        TimerImage.fillAmount = progress;
        TimerTxt.text = progress.ToString();
    }

    private void TimerEnded()
    {
        Debug.Log("Timer completed.");
        TimerObj.SetActive(false);
        //onTimerComplete?.Invoke();
    }
}
