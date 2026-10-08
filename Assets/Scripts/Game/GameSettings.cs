using UnityEngine;

namespace ZooWorld.Game
{
    [CreateAssetMenu(menuName = "Zoo World/Game Settings", fileName = "GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Spawning")]
        [SerializeField, Min(0.1f)] private float minSpawnInterval = 1f;
        [SerializeField, Min(0.1f)] private float maxSpawnInterval = 2f;
        [Tooltip("Safety limit on the population; spawning pauses while it is reached. 0 means no limit.")]
        [SerializeField, Min(0)] private int maxAlive = 200;
        [SerializeField, Min(0f)] private float spawnHeight = 1f;

        [Header("Wandering")]
        [SerializeField, Min(0.1f)] private float wanderInterval = 3f;
        [Tooltip("How far inside the screen edge animals turn back, in world units.")]
        [SerializeField, Min(0f)] private float playAreaMargin = 2.5f;

        public float MinSpawnInterval => minSpawnInterval;
        public float MaxSpawnInterval => maxSpawnInterval;
        public int MaxAlive => maxAlive;
        public float SpawnHeight => spawnHeight;
        public float WanderInterval => wanderInterval;
        public float PlayAreaMargin => playAreaMargin;

        private void OnValidate() => maxSpawnInterval = Mathf.Max(maxSpawnInterval, minSpawnInterval);
    }
}