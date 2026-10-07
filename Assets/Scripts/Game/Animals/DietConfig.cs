using System;
using UnityEngine;
using ZooWorld.Core.Animals;

namespace ZooWorld.Game.Game.Animals
{
    [CreateAssetMenu(menuName = "Zoo World/Diet", fileName = "Diet")]
    public sealed class DietConfig : ScriptableObject, IDiet
    {
        [Tooltip("Stable name used by statistics and by the HUD row that counts this diet.")]
        [SerializeField] private string id;
        [Tooltip("Roles this one eats. A role may list itself.")]
        [SerializeField] private DietConfig[] eats = Array.Empty<DietConfig>();

        public string Id => id;

        public bool CanEat(IDiet other) => Array.IndexOf(eats, other as DietConfig) >= 0;
    }
}