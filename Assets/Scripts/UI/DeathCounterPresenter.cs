using System;
using System.Collections.Generic;
using UnityEngine.Localization;
using VContainer;
using VContainer.Unity;
using ZooWorld.Core;

namespace ZooWorld.UI
{
    public class DeathCounterPresenter : IStartable, IDisposable
    {
        private readonly DeathStats _stats;
        private readonly DeathCounterView _view;
        private readonly List<Binding> _bindings = new();

        [Inject]
        public DeathCounterPresenter(DeathStats stats, DeathCounterView view)
        {
            _stats = stats;
            _view = view;
        }

        public void Start()
        {
            foreach (var row in _view.Rows)
            {
                var text = new LocalizedString(UiStrings.Table, row.StringKey)
                {
                    Arguments = new object[] { _stats.GetDeaths(row.DietId) }
                };
                var binding = new Binding(row, text);
                // StringChanged fires now and again whenever the locale changes.
                text.StringChanged += binding.Show;
                _bindings.Add(binding);
            }

            _stats.Changed += Refresh;
        }

        public void Dispose()
        {
            _stats.Changed -= Refresh;
            foreach (var binding in _bindings)
                binding.text.StringChanged -= binding.Show;
        }

        private void Refresh()
        {
            foreach (var binding in _bindings)
            {
                binding.text.Arguments[0] = _stats.GetDeaths(binding.row.DietId);
                binding.text.RefreshString();
            }
        }

        private sealed class Binding
        {
            public readonly DeathCounterView.Row row;
            public readonly LocalizedString text;

            public Binding(DeathCounterView.Row row, LocalizedString text)
            {
                this.row = row;
                this.text = text;
            }

            public void Show(string value) => row.SetText(value);
        }
    }
}
