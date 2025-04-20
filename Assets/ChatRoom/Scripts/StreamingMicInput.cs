using System;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Threading;


[RequireComponent(typeof(AudioSource))]
public class StreamingMicInput : MonoBehaviour
{
    public AudioSource audioSource = null;

    [SerializeField]
    [Range(0.0f, 100.0f)]
    [Tooltip("Microphone input volume control.")]
    private float micInputVolume = 100;

    [SerializeField]
    [Tooltip("Requested microphone input frequency")]
    private int micFrequency = 48000;

    [Tooltip("Will contain the string name of the selected microphone device - read only.")]
    public string selectedDevice;

    // PRIVATE MEMBERS
    private bool micSelected = false;
    private int minFreq, maxFreq;
    private bool focused = true;
    private bool initialized = false;
    void Awake()
    {
        // First thing to do, cache the unity audio source (can be managed by the
        // user if audio source can change)
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!audioSource) return; // this should never happen
    }

    void Start()
    {
        audioSource.loop = true;     // Set the AudioClip to loop
        audioSource.mute = false;

        InitializeMicrophone();
    }

    /// <summary>
    /// Initializes the microphone.
    /// </summary>
    private void InitializeMicrophone()
    {
        if (initialized)
        {
            return;
        }
        if (Microphone.devices.Length == 0)
        {
            return;
        }
        selectedDevice = Microphone.devices[0].ToString();
        micSelected = true;
        GetMicCaps();
        initialized = true;
    }

    private void Update()
    {
        if (!focused)
        {
            if (Microphone.IsRecording(selectedDevice))
            {
                StopMicrophone();
            }
            return;
        }
        if (!Application.isPlaying)
        {
            StopMicrophone();
            return;
        }

        // Lazy Microphone initialization (needed on Android)
        if (!initialized)
        {
            InitializeMicrophone();
        }

        audioSource.volume = (micInputVolume / 100);

        if (!Microphone.IsRecording(selectedDevice))
        {
            StartMicrophone();
        }
    }

    /// <summary>
    /// Raises the application focus event.
    /// </summary>
    /// <param name="focus">If set to <c>true</c>: focused.</param>
    void OnApplicationFocus(bool focus)
    {
        focused = focus;

        if (!focused)
            StopMicrophone();
    }
    /// <summary>
    /// Gets the mic caps.
    /// </summary>
    public void GetMicCaps()
    {
        if (micSelected == false) return;

        //Gets the frequency of the device
        Microphone.GetDeviceCaps(selectedDevice, out minFreq, out maxFreq);

        if (minFreq == 0 && maxFreq == 0)
        {
            Debug.LogWarning("GetMicCaps warning:: min and max frequencies are 0");
            minFreq = 44100;
            maxFreq = 44100;
        }

        if (micFrequency > maxFreq)
            micFrequency = maxFreq;
    }

    public void StartMicrophone()
    {
        if (micSelected == false) return;

        //Starts recording
        audioSource.clip = Microphone.Start(selectedDevice, true, 1, micFrequency);

        Stopwatch timer = Stopwatch.StartNew();

        // Wait until the recording has started
        while (!(Microphone.GetPosition(selectedDevice) > 0) && timer.Elapsed.TotalMilliseconds < 1000)
        {
            Thread.Sleep(50);
        }

        if (Microphone.GetPosition(selectedDevice) <= 0)
        {
            throw new Exception("Timeout initializing microphone " + selectedDevice);
        }
        // Play the audio source
        audioSource.Play();
    }

    /// <summary>
    /// Stops the microphone.
    /// </summary>
    public void StopMicrophone()
    {
        if (micSelected == false) return;

        // Overriden with a clip to play? Don't stop the audio source
        if ((audioSource != null) &&
            (audioSource.clip != null) &&
            (audioSource.clip.name == "Microphone"))
        {
            audioSource.Stop();
        }

        Microphone.End(selectedDevice);
    }
    /// <summary>
    /// Raises the application pause event.
    /// </summary>
    /// <param name="pauseStatus">If set to <c>true</c>: paused.</param>
    void OnApplicationPause(bool pauseStatus)
    {
        focused = !pauseStatus;

        if (!focused)
            StopMicrophone();
    }

    void OnDisable()
    {
        StopMicrophone();
    }

}
