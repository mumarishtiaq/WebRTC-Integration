using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.MaterialProperty;

public class LoadingAnimationDot : MonoBehaviour
{
    [SerializeField] private Transform[] circles;
    private float minScaleFactor = 0.1f;
    private float maxScaleFactor = 1f;
    private float scaleDuration = 0.35f;
    private float delayBetween = 0.15f;

    private List<Tween> tweens;

    private void Awake()
    {
        // Initialize list
        tweens = new List<Tween>(circles.Length);

        // Prepare all circles at min scale and create paused tweens
        for (int i = 0; i < circles.Length; i++)
        {
            var circle = circles[i];
            circle.localScale = Vector3.one * minScaleFactor;

            float delay = i * delayBetween;
            var tween = circle.DOScale(maxScaleFactor, scaleDuration)
                               .SetDelay(delay)
                               .SetEase(Ease.InOutSine)
                               .SetLoops(-1, LoopType.Yoyo)
                               .SetAutoKill(false)
                               .Pause();
            tweens.Add(tween);
        }
    }

    private void OnEnable()
    {
        // Resume all tweens when enabled
        foreach (var tween in tweens)
        {
            tween.Play();
        }
    }

    private void OnDisable()
    {
        // Pause all tweens when disabled
        foreach (var tween in tweens)
        {
            tween.Pause();
        }
    }
}
