using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ZooWorld.UI
{
    /// <summary>
    /// Child scope for the UI. Its parent is GameLifetimeScope, chosen by type name in the
    /// inspector, so the Game assembly never has to reference this one.
    /// </summary>
    public class UiLifetimeScope : LifetimeScope
    {
        [SerializeField] private DeathCounterView deathCounter;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(deathCounter);

            builder.RegisterEntryPoint<DeathCounterPresenter>();
        }
    }
}