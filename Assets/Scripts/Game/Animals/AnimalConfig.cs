using UnityEngine;
using UnityEngine.AddressableAssets;
using ZooWorld.Game.Movement;

namespace ZooWorld.Game.Animals
{
    [CreateAssetMenu(menuName = "Zoo World/Animal", fileName = "Animal")]
    public class AnimalConfig : ScriptableObject
    {
        [Tooltip("Loaded on the first spawn of this species.")]
        [SerializeField] private AssetReferenceGameObject prefab;
        [SerializeField] private DietConfig diet;
        [SerializeField] private MovementConfig movement;

        public AssetReferenceGameObject Prefab => prefab;
        public DietConfig Diet => diet;
        public MovementConfig Movement => movement;
    }
}
