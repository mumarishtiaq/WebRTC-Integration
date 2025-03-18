using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLipSync : MonoBehaviour
{
    public SkinnedMeshRenderer characterFace; // Assign the character's SkinnedMeshRenderer
    public string mouthBlendShapeName = "mouthOpen"; // Name of the blend shape controlling mouth movement

    private int blendShapeIndex;
    private AudioClip microphoneInput;
    private int sampleWindow = 128;
    private float[] audioSamples;
    private bool micActive = false;

    void Start()
    {
        if (characterFace == null)
        {
            Debug.LogError("SkinnedMeshRenderer is not assigned!");
            return;
        }

        // Get the blend shape index
        blendShapeIndex = characterFace.sharedMesh.GetBlendShapeIndex(mouthBlendShapeName);
        if (blendShapeIndex == -1)
        {
            Debug.LogError($"Blend shape '{mouthBlendShapeName}' not found!");
            return;
        }

        // Start microphone
        StartMicrophone();
    }

    void Update()
    {
        if (micActive && Microphone.IsRecording(null))
        {
            float volume = GetAverageVolume();
            float mouthOpenValue = Mathf.Lerp(characterFace.GetBlendShapeWeight(blendShapeIndex), volume * 100f, Time.deltaTime * 10f);

            // Apply threshold to avoid unwanted movement
            if (volume < 0.02f)
            {
                mouthOpenValue = Mathf.Lerp(characterFace.GetBlendShapeWeight(blendShapeIndex), 0, Time.deltaTime * 5f);
            }

            characterFace.SetBlendShapeWeight(blendShapeIndex, mouthOpenValue);
        }
    }

    void StartMicrophone()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneInput = Microphone.Start(null, true, 1, AudioSettings.outputSampleRate);
            audioSamples = new float[sampleWindow];
            micActive = true;
        }
        else
        {
            Debug.LogError("No microphone detected!");
        }
    }

    float GetAverageVolume()
    {
        int micPosition = Microphone.GetPosition(null) - (sampleWindow + 1);
        if (micPosition < 0) return 0;

        microphoneInput.GetData(audioSamples, micPosition);

        float sum = 0f;
        for (int i = 0; i < sampleWindow; i++)
        {
            sum += Mathf.Abs(audioSamples[i]);
        }

        return sum / sampleWindow;
    }
}
