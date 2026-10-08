using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZooWorld.UI
{
    public class LanguageToggleView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text label;

        public event Action Clicked;

        public void SetLabel(string text) => label.text = text;

        private void Awake() => button.onClick.AddListener(OnClick);

        private void OnDestroy() => button.onClick.RemoveListener(OnClick);

        private void OnClick() => Clicked?.Invoke();
    }
}
