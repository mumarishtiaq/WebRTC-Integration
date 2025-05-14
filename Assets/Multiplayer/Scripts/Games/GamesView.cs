using DG.Tweening;
using System;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;

public class GamesView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _bg;
    [SerializeField] private Transform _window;


    private float _openDuration = 0.5f;
    private float _closeDuration = 0.4f;


    //[Header("Game Buttons")]
    //[SerializeField] private Button _game1Btm;
    //[SerializeField] private Button _game2Btm;
    //[SerializeField] private Button _game3Btm;

    


    private void Start()
    {
        ResetToDefault();
    }

    [ContextMenu("Open")]
   public void Open()
    {
        ResetToDefault();
        _bg.gameObject.SetActive(true);
        Fade(0.64f, _openDuration);
        Scale(Vector3.one, _openDuration, Ease.OutBack);
    }

    [ContextMenu("Close")]
    public void Close()
    {
        DOTween.KillAll();
        Fade(0, _closeDuration);
        Scale(Vector3.zero, _closeDuration, Ease.InBack,()=>_bg.gameObject.SetActive(false));
    }


    private void Fade(float endValue = 0,float duration = 0)
    {
        _bg.DOFade(endValue, duration);
    }
    
    private void Scale(Vector3 endValue, float duration = 0,Ease ease = Ease.InBack, TweenCallback onComplete = null)
    {
        _window.DOScale(endValue, duration).SetEase(ease).OnComplete(onComplete);
    }

    private void ResetToDefault()
    {
        DOTween.KillAll();
        _bg.gameObject.SetActive(false);
        Fade();
        Scale(Vector3.zero);
    }


    





}

public enum GameType
{
    None = -1,
    Tik_Tak_Toe = 0,
    Bubble_Shooter = 1,
    Chess = 2
}

