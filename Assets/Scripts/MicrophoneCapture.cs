using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneCapture : MonoBehaviour
{
    public AudioClip microphoneClip;
    private int sampleRate = 44100;  // Standard audio sample rate

    void Start()
    {
        // Check if a microphone is available
        if (Microphone.devices.Length > 0)
        {
            string micName = Microphone.devices[0];  // Get default mic
            microphoneClip = Microphone.Start(micName, true, 10, sampleRate);
            Debug.Log("Microphone started: " + micName);
        }
        else
        {
            Debug.LogError("No microphone found!");
        }
    }
}
