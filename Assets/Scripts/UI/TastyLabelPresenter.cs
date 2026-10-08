using System;
using System.Collections.Generic;
using MessagePipe;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using ZooWorld.Core.Animals;
using ZooWorld.Core.Events;

namespace ZooWorld.UI
{
    public class TastyLabelPresenter : IStartable, ITickable, IDisposable, IMessageHandler<AnimalAteEvent>
    {
        // World units below the predator, toward the bottom of the screen.
        private const float Offset = 0.9f;

        private readonly ISubscriber<AnimalAteEvent> _ate;
        private readonly Camera _camera;
        private readonly TastyLabelView _prefab;
        private readonly RectTransform _root;
        private readonly LocalizedString _tasty = new(UiStrings.Table, UiStrings.Tasty);

        private readonly List<(TastyLabelView Label, Animal Predator)> _active = new();

        private ObjectPool<TastyLabelView> _pool;
        private IDisposable _subscription;
        private string _text = string.Empty;

        [Inject]
        public TastyLabelPresenter(ISubscriber<AnimalAteEvent> ate, Camera camera, TastyLabelView prefab,
            RectTransform root)
        {
            _ate = ate;
            _camera = camera;
            _prefab = prefab;
            _root = root;
        }

        public void Start()
        {
            _pool = new ObjectPool<TastyLabelView>(
                createFunc: () => UnityEngine.Object.Instantiate(_prefab, _root),
                actionOnRelease: label => label.gameObject.SetActive(false));
            _tasty.StringChanged += SetText;
            _subscription = _ate.Subscribe(this);
        }

        public void Handle(AnimalAteEvent message)
        {
            var label = _pool.Get();
            label.SetScreenPosition(ScreenPositionUnder(message.ate));
            _active.Add((label, message.ate));
            label.Play(_text, Release);
        }

        public void Tick()
        {
            foreach (var (label, predator) in _active)
            {
                // A dead predator's body returns to the pool and may respawn elsewhere;
                // its label stays where it died.
                if (predator.IsAlive)
                    label.SetScreenPosition(ScreenPositionUnder(predator));
            }
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _tasty.StringChanged -= SetText;
        }

        // Also reaches labels already on screen: one shown before the string table loaded, or
        // while the language is being switched.
        private void SetText(string text)
        {
            _text = text;
            foreach (var (label, _) in _active)
                label.SetText(text);
        }

        private void Release(TastyLabelView label)
        {
            _active.RemoveAll(entry => entry.Label == label);
            _pool.Release(label);
        }

        private Vector3 ScreenPositionUnder(Animal predator)
        {
            var screen = _camera.WorldToScreenPoint(predator.Body.DisplayPosition - _camera.transform.up * Offset);
            screen.z = 0f;
            return screen;
        }
    }
}
