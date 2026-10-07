using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZooWorld.Core;
using ZooWorld.Core.Events;
using ZooWorld.Core.World;
using ZooWorld.Game.Game.Animals;

namespace ZooWorld.Game.Game
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameSettings settings;
        [SerializeField] private Camera gameCamera;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(settings);
            builder.RegisterComponent(gameCamera);

            builder.Register<IRandom, UnityRandom>(Lifetime.Singleton);
            builder.Register<IPlayArea, CameraPlayArea>(Lifetime.Singleton);
            builder.Register<AnimalWorld>(Lifetime.Singleton);
            builder.Register<DeathStats>(Lifetime.Singleton);
            builder.Register<AnimalCatalog>(Lifetime.Singleton);
            builder.Register<AnimalFactory>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<AnimalSpawner>().AsSelf();
            builder.RegisterEntryPoint<AnimalSimulation>();
            builder.RegisterEntryPoint<GameBootstrap>();
            
            var messagePipe = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<AnimalDiedEvent>(messagePipe);
            builder.RegisterMessageBroker<AnimalAteEvent>(messagePipe);
            
            // Statistics count from the first death whether or not any UI ever asks for them.
            builder.RegisterBuildCallback(resolver => resolver.Resolve<DeathStats>());
        }
    }
}
