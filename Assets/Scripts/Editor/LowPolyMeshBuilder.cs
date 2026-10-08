using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ZooWorld.Editor
{
    /// <summary>
    /// Collects flat-shaded, vertex-coloured triangles. Every triangle owns its three vertices,
    /// which keeps the faces hard-edged; the smoothed normals the outline needs go into UV3.
    /// </summary>
    internal sealed class LowPolyMeshBuilder
    {
        private static readonly Vector3[] IcosahedronVertices = CreateIcosahedronVertices();

        private static readonly int[] IcosahedronTriangles =
        {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
            1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
            4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
        };

        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Color> _colors = new List<Color>();

        /// <summary>Adds a triangle, winding it so that it faces away from <paramref name="inside"/>.</summary>
        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 inside, Color color)
        {
            var normal = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(normal, (a + b + c) / 3f - inside) < 0f)
                (b, c) = (c, b);

            _vertices.Add(a);
            _vertices.Add(b);
            _vertices.Add(c);
            _colors.Add(color);
            _colors.Add(color);
            _colors.Add(color);
        }

        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 inside, Color color)
        {
            AddTriangle(a, b, c, inside, color);
            AddTriangle(a, c, d, inside, color);
        }

        /// <summary>A faceted ellipsoid. <paramref name="jitter"/> pushes vertices in and out, which makes rocks.</summary>
        public void AddBlob(Vector3 center, Vector3 radii, Color color, int subdivisions = 1, float jitter = 0f)
        {
            for (var i = 0; i < IcosahedronTriangles.Length; i += 3)
            {
                Subdivide(
                    IcosahedronVertices[IcosahedronTriangles[i]],
                    IcosahedronVertices[IcosahedronTriangles[i + 1]],
                    IcosahedronVertices[IcosahedronTriangles[i + 2]],
                    subdivisions, center, radii, color, jitter);
            }
        }

        public void AddCone(Vector3 baseCenter, float radius, float height, int sides, Color color)
        {
            var apex = baseCenter + Vector3.up * height;
            var inside = baseCenter + Vector3.up * (height * 0.25f);
            for (var i = 0; i < sides; i++)
            {
                var a = baseCenter + RingPoint(i, sides) * radius;
                var b = baseCenter + RingPoint(i + 1, sides) * radius;
                AddTriangle(a, b, apex, inside, color);
                AddTriangle(a, b, baseCenter, inside, color);
            }
        }

        /// <summary>A closed tube through the given centres. The two colours alternate per segment, giving stripes.</summary>
        public void AddTube(IReadOnlyList<Vector3> centers, IReadOnlyList<float> radii, int sides,
            Color colorA, Color colorB)
        {
            var last = centers.Count - 1;
            var rings = new Vector3[centers.Count][];
            for (var i = 0; i <= last; i++)
            {
                var tangent = (centers[Mathf.Min(i + 1, last)] - centers[Mathf.Max(i - 1, 0)]).normalized;
                var side = Mathf.Abs(tangent.y) > 0.99f
                    ? Vector3.right
                    : Vector3.Cross(Vector3.up, tangent).normalized;
                var up = Vector3.Cross(tangent, side);

                rings[i] = new Vector3[sides];
                for (var s = 0; s < sides; s++)
                {
                    var angle = s * 2f * Mathf.PI / sides;
                    rings[i][s] = centers[i] + (side * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radii[i];
                }
            }

            for (var i = 0; i < last; i++)
            {
                var inside = (centers[i] + centers[i + 1]) * 0.5f;
                var color = i % 2 == 0 ? colorA : colorB;
                for (var s = 0; s < sides; s++)
                {
                    var next = (s + 1) % sides;
                    AddQuad(rings[i][s], rings[i][next], rings[i + 1][next], rings[i + 1][s], inside, color);
                }
            }

            AddCap(rings[0], centers[0], centers[1], colorA);
            AddCap(rings[last], centers[last], centers[last - 1], last % 2 == 0 ? colorB : colorA);
        }

        public Mesh Build(string name)
        {
            var mesh = new Mesh { name = name };
            if (_vertices.Count > ushort.MaxValue)
                mesh.indexFormat = IndexFormat.UInt32;

            var triangles = new int[_vertices.Count];
            for (var i = 0; i < triangles.Length; i++)
                triangles[i] = i;

            mesh.SetVertices(_vertices);
            mesh.SetColors(_colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.SetUVs(3, SmoothNormals(mesh.normals));
            mesh.RecalculateBounds();
            return mesh;
        }

        private void AddCap(Vector3[] ring, Vector3 center, Vector3 inside, Color color)
        {
            for (var s = 0; s < ring.Length; s++)
                AddTriangle(ring[s], ring[(s + 1) % ring.Length], center, inside, color);
        }

        private void Subdivide(Vector3 a, Vector3 b, Vector3 c, int depth, Vector3 center, Vector3 radii,
            Color color, float jitter)
        {
            if (depth == 0)
            {
                AddTriangle(
                    Place(a, center, radii, jitter),
                    Place(b, center, radii, jitter),
                    Place(c, center, radii, jitter),
                    center, color);
                return;
            }

            var ab = ((a + b) * 0.5f).normalized;
            var bc = ((b + c) * 0.5f).normalized;
            var ca = ((c + a) * 0.5f).normalized;
            Subdivide(a, ab, ca, depth - 1, center, radii, color, jitter);
            Subdivide(b, bc, ab, depth - 1, center, radii, color, jitter);
            Subdivide(c, ca, bc, depth - 1, center, radii, color, jitter);
            Subdivide(ab, bc, ca, depth - 1, center, radii, color, jitter);
        }

        private static Vector3 Place(Vector3 unit, Vector3 center, Vector3 radii, float jitter) =>
            center + Vector3.Scale(unit * (1f + jitter * Noise(unit)), radii);

        // Depends on the vertex direction alone, so triangles that share a corner still meet.
        private static float Noise(Vector3 unit)
        {
            var value = Mathf.Sin(Vector3.Dot(unit, new Vector3(12.9898f, 78.233f, 37.719f))) * 43758.5453f;
            return (value - Mathf.Floor(value)) * 2f - 1f;
        }

        /// <summary>
        /// Averages face normals over vertices at the same position. Pushing the outline shell
        /// along these keeps it closed where flat shading splits the real normals.
        /// </summary>
        private List<Vector3> SmoothNormals(Vector3[] normals)
        {
            var sums = new Dictionary<Vector3Int, Vector3>();
            for (var i = 0; i < _vertices.Count; i++)
            {
                var key = Quantize(_vertices[i]);
                sums.TryGetValue(key, out var sum);
                sums[key] = sum + normals[i];
            }

            var smooth = new List<Vector3>(_vertices.Count);
            for (var i = 0; i < _vertices.Count; i++)
                smooth.Add(sums[Quantize(_vertices[i])].normalized);
            return smooth;
        }

        private static Vector3Int Quantize(Vector3 position) => Vector3Int.RoundToInt(position * 10000f);

        private static Vector3 RingPoint(int index, int sides)
        {
            var angle = index * 2f * Mathf.PI / sides;
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        }

        private static Vector3[] CreateIcosahedronVertices()
        {
            var t = (1f + Mathf.Sqrt(5f)) * 0.5f;
            var vertices = new[]
            {
                new Vector3(-1f, t, 0f), new Vector3(1f, t, 0f), new Vector3(-1f, -t, 0f), new Vector3(1f, -t, 0f),
                new Vector3(0f, -1f, t), new Vector3(0f, 1f, t), new Vector3(0f, -1f, -t), new Vector3(0f, 1f, -t),
                new Vector3(t, 0f, -1f), new Vector3(t, 0f, 1f), new Vector3(-t, 0f, -1f), new Vector3(-t, 0f, 1f)
            };
            for (var i = 0; i < vertices.Length; i++)
                vertices[i].Normalize();
            return vertices;
        }
    }
}
