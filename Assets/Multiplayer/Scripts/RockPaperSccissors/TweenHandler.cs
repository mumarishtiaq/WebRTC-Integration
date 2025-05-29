using DG.Tweening;
using System;
using UnityEngine;

public static class TweenHandler
{
    public static void Scale(this Transform t,Vector3 target, float duration = 0 , Ease ease = Ease.Unset,Action OnCompleted = null)
    {
        t.DOScale(target, duration).SetEase(ease).OnComplete(()=>OnCompleted?.Invoke());
    }
}
