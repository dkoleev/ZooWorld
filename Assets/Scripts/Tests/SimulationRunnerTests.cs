using NUnit.Framework;
using ZooWorld.Editor.Simulation;

namespace ZooWorld.Tests
{
    public sealed class SimulationRunnerTests
    {
        [TestCase(3000f, false, 3000)]
        [TestCase(60f, true, 3000)]
        [TestCase(0.1f, true, 5)]
        [TestCase(0f, false, 1)]
        [TestCase(-5f, true, 1)]
        [TestCase(0.001f, true, 1)]   // shorter than a tick still simulates one
        public void ToTicksConvertsAndNeverGoesBelowOne(float duration, bool inSeconds, int expected)
        {
            Assert.That(SimulationRunner.ToTicks(duration, inSeconds, 0.02f), Is.EqualTo(expected));
        }
    }
}
