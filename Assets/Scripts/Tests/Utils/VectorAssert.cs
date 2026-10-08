using NUnit.Framework;
using UnityEngine;

namespace ZooWorld.Tests.Utils
{
    internal static class VectorAssert
    {
        public static void AreEqual(Vector3 expected, Vector3 actual) =>
            Assert.That(Vector3.Distance(expected, actual), Is.LessThan(1e-4f), $"Expected {expected:F4} but was {actual:F4}");
    }
}