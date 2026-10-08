using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ZooWorld.Editor.Simulation
{
    /// <summary>What one run did to one species.</summary>
    public readonly struct SpeciesTally
    {
        public readonly int eaten;
        public readonly int alive;

        public SpeciesTally(int eaten, int alive)
        {
            this.eaten = eaten;
            this.alive = alive;
        }

        // Nothing leaves the world except by being eaten.
        public int Spawned => eaten + alive;
    }

    /// <summary>One species across a whole series; the averages are per run.</summary>
    public readonly struct SpeciesRow
    {
        public readonly string species;
        public readonly float spawned;
        public readonly float eaten;
        public readonly int eatenMin;
        public readonly int eatenMax;
        public readonly float alive;

        public SpeciesRow(string species, float spawned, float eaten, int eatenMin, int eatenMax, float alive)
        {
            this.species = species;
            this.spawned = spawned;
            this.eaten = eaten;
            this.eatenMin = eatenMin;
            this.eatenMax = eatenMax;
            this.alive = alive;
        }
    }

    public static class SimulationReport
    {
        public static List<SpeciesRow> Summarize(IReadOnlyList<IReadOnlyDictionary<string, SpeciesTally>> runs)
        {
            var species = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var run in runs)
                species.UnionWith(run.Keys);

            var rows = new List<SpeciesRow>();
            foreach (var name in species)
            {
                int spawned = 0, eaten = 0, alive = 0, min = int.MaxValue, max = 0;
                foreach (var run in runs)
                {
                    // A species that never showed up in a run still counts there, as zero.
                    run.TryGetValue(name, out var tally);
                    spawned += tally.Spawned;
                    eaten += tally.eaten;
                    alive += tally.alive;
                    min = Math.Min(min, tally.eaten);
                    max = Math.Max(max, tally.eaten);
                }

                float count = runs.Count;
                rows.Add(new SpeciesRow(name, spawned / count, eaten / count, min, max, alive / count));
            }

            return rows;
        }

        public static string ToCsv(IReadOnlyList<SpeciesRow> rows)
        {
            // Invariant numbers: a decimal comma would split the value into two columns.
            var csv = new StringBuilder("species,spawned_avg,eaten_avg,eaten_min,eaten_max,alive_avg\n");
            foreach (var row in rows)
                csv.Append(string.Format(CultureInfo.InvariantCulture, "{0},{1:0.##},{2:0.##},{3},{4},{5:0.##}\n",
                    row.species, row.spawned, row.eaten, row.eatenMin, row.eatenMax, row.alive));
            return csv.ToString();
        }
    }
}
