using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using ZooWorld.Editor.Simulation;

namespace ZooWorld.Tests
{
    public sealed class SimulationReportTests
    {
        private static IReadOnlyDictionary<string, SpeciesTally> Run(params (string species, int eaten, int alive)[] tallies)
        {
            var run = new Dictionary<string, SpeciesTally>();
            foreach (var (species, eaten, alive) in tallies)
                run[species] = new SpeciesTally(eaten, alive);
            return run;
        }

        [Test]
        public void AveragesAndBoundsEachSpeciesAcrossRuns()
        {
            var rows = SimulationReport.Summarize(new[]
            {
                Run(("Snake", 1, 3), ("Frog", 4, 2)),
                Run(("Snake", 3, 3), ("Frog", 8, 0))
            });

            Assert.That(rows, Has.Count.EqualTo(2));

            // Sorted by name, so the table does not reshuffle between series.
            Assert.That(rows[0].species, Is.EqualTo("Frog"));
            Assert.That(rows[0].spawned, Is.EqualTo(7f));
            Assert.That(rows[0].eaten, Is.EqualTo(6f));
            Assert.That(rows[0].eatenMin, Is.EqualTo(4));
            Assert.That(rows[0].eatenMax, Is.EqualTo(8));
            Assert.That(rows[0].alive, Is.EqualTo(1f));

            Assert.That(rows[1].species, Is.EqualTo("Snake"));
            Assert.That(rows[1].spawned, Is.EqualTo(5f));
            Assert.That(rows[1].eaten, Is.EqualTo(2f));
            Assert.That(rows[1].eatenMin, Is.EqualTo(1));
            Assert.That(rows[1].eatenMax, Is.EqualTo(3));
            Assert.That(rows[1].alive, Is.EqualTo(3f));
        }

        [Test]
        public void SpeciesMissingFromARunCountsAsZeroThere()
        {
            var rows = SimulationReport.Summarize(new[] { Run(("Frog", 4, 2)), Run() });

            Assert.That(rows, Has.Count.EqualTo(1));
            Assert.That(rows[0].spawned, Is.EqualTo(3f));
            Assert.That(rows[0].eaten, Is.EqualTo(2f));
            Assert.That(rows[0].eatenMin, Is.EqualTo(0));
            Assert.That(rows[0].eatenMax, Is.EqualTo(4));
            Assert.That(rows[0].alive, Is.EqualTo(1f));
        }

        [Test]
        public void NoRunsGiveNoRows()
        {
            Assert.That(SimulationReport.Summarize(new IReadOnlyDictionary<string, SpeciesTally>[0]), Is.Empty);
        }

        [Test]
        public void CsvKeepsADecimalPointUnderACommaCulture()
        {
            var previous = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
            try
            {
                var rows = SimulationReport.Summarize(new[] { Run(("Frog", 2, 1)), Run(("Frog", 3, 1)) });

                Assert.That(SimulationReport.ToCsv(rows), Is.EqualTo(
                    "species,spawned_avg,eaten_avg,eaten_min,eaten_max,alive_avg\n" +
                    "Frog,3.5,2.5,2,3,1\n"));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previous;
            }
        }
    }
}
