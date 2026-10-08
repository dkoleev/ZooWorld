using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ZooWorld.UI
{
    public class TastyLabelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private CanvasGroup group;

        public void SetScreenPosition(Vector3 position) => transform.position = position;

        public void SetText(string mainText) => this.text.text = mainText;

        /// <summary>Pops in, holds, fades out, then reports back so the label can be pooled.</summary>
        public void Play(string mainText, Action<TastyLabelView> finished)
        {
            SetText(mainText);
            group.alpha = 1f;
            transform.localScale = Vector3.one * 0.5f;
            gameObject.SetActive(true);

            DOTween.Sequence()
                .Append(transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack))
                .AppendInterval(0.5f)
                .Append(group.DOFade(0f, 0.3f))
                // Dies with the label, so nothing survives into the next play session.
                .SetLink(gameObject)
                .OnComplete(() => finished(this));
        }
    }
}
