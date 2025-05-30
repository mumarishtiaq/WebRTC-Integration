using DG.Tweening;
using System;
using UnityEngine;

public static class TweenHandler
{
    public static void Scale(this Transform t,Vector3 target, float duration = 0 , Ease ease = Ease.Unset,Action OnCompleted = null)
    {
        t.DOScale(target, duration).SetEase(ease).OnComplete(()=>OnCompleted?.Invoke());
    } 
    public static void MoveX(this Transform t,float target, float duration = 0 , Action OnCompleted = null)
    {
        t.DOLocalMoveX(target, duration).OnComplete(() => OnCompleted?.Invoke());
    } 
    public static void MoveX(this Transform t,Vector3 target, float duration = 0 , Action OnCompleted = null)
    {
        t.DOMove(target, duration).OnComplete(() => OnCompleted?.Invoke());
    }
}
