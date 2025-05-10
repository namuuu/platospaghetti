using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CrossFade : SceneTransition
{
    public CanvasGroup crossFade;

    public override IEnumerator AnimateTransitionIn()
    {
        crossFade.alpha = 0;
        var transitionTweener = crossFade.DOFade(1, 1);
        yield return transitionTweener.WaitForCompletion();
    }

    public override IEnumerator AnimateTransitionOut()
    {
        crossFade.alpha = 1;
        var transitionTweener = crossFade.DOFade(0, 1);
        yield return transitionTweener.WaitForCompletion();
    }
}
