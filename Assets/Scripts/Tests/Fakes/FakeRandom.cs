using UnityEngine;
using ZooWorld.Core;

namespace ZooWorld.Tests.Fakes
{
    /// <summary>Returns a fixed point of the requested range: 0 gives min, 1 gives max (max - 1 for ints, where max is exclusive).</summary>
    public class FakeRandom : IRandom
    {
        public float Fraction { get; set; }

        public float Range(float min, float max) => Mathf.Lerp(min, max, Fraction);

        public int Range(int min, int max) => Mathf.Max(min, Mathf.Min(max - 1, (int)Mathf.Lerp(min, max, Fraction)));
    }
}
