using System;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;

namespace ZooWorld.UI
{
    public class LanguageTogglePresenter : IStartable, IDisposable
    {
        private readonly LanguageToggleView _view;
        private AsyncOperationHandle<Locale> _initialLocale;

        [Inject]
        public LanguageTogglePresenter(LanguageToggleView view) => _view = view;

        public void Start()
        {
            _view.Clicked += SelectNextLocale;
            LocalizationSettings.SelectedLocaleChanged += ShowLocale;
            _initialLocale = LocalizationSettings.SelectedLocaleAsync;
            _initialLocale.Completed += OnLocaleReady;
        }

        public void Dispose()
        {
            _view.Clicked -= SelectNextLocale;
            // A static event: left subscribed, it would outlive this play session.
            LocalizationSettings.SelectedLocaleChanged -= ShowLocale;
            if (_initialLocale.IsValid())
                _initialLocale.Completed -= OnLocaleReady;
        }

        private void OnLocaleReady(AsyncOperationHandle<Locale> handle) => ShowLocale(handle.Result);

        private void SelectNextLocale()
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            if (locales.Count == 0)
                return;

            var next = (locales.IndexOf(LocalizationSettings.SelectedLocale) + 1) % locales.Count;
            LocalizationSettings.SelectedLocale = locales[next];
        }

        private void ShowLocale(Locale locale)
        {
            if (locale != null)
                _view.SetLabel(locale.Identifier.Code.ToUpperInvariant());
        }
    }
}
