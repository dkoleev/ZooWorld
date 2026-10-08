using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZooWorld.Editor
{
    internal static class ZooMeshGenerator
    {
        private const string Folder = "Assets/Art/Meshes";
        
        [MenuItem("Zoo World/Generate Meshes")]
        public static void GenerateAll()
        {
            EnsureFolder(Folder);
            Save(Frog());
            Save(Snake());
            Save(Tree());
            Save(Rock());
            Save(Bush());
            Save(Ground());
            AssetDatabase.SaveAssets();
            Debug.Log("Zoo World meshes generated in " + Folder);
        }

        private static Mesh Frog()
        {
            var skin = Hex("5DBB4A");
            var limb = Hex("3F9A3A");
            var eye = Hex("FFF6D5");
            var pupil = Hex("1E1B26");

            var b = new LowPolyMeshBuilder();
            b.AddBlob(new Vector3(0f, 0.24f, -0.02f), new Vector3(0.30f, 0.22f, 0.34f), skin);
            b.AddBlob(new Vector3(0f, 0.30f, 0.26f), new Vector3(0.24f, 0.17f, 0.20f), skin);
            foreach (var side in new[] { -1f, 1f })
            {
                b.AddBlob(new Vector3(side * 0.13f, 0.46f, 0.28f), Vector3.one * 0.085f, eye);
                b.AddBlob(new Vector3(side * 0.13f, 0.52f, 0.31f), Vector3.one * 0.045f, pupil);
                b.AddBlob(new Vector3(side * 0.28f, 0.11f, -0.18f), new Vector3(0.11f, 0.11f, 0.20f), limb);
                b.AddBlob(new Vector3(side * 0.22f, 0.07f, 0.24f), new Vector3(0.06f, 0.07f, 0.09f), limb);
            }
            return b.Build("Frog");
        }

        private static Mesh Snake()
        {
            var scales = Hex("E0533D");
            var band = Hex("F2C14E");
            var eye = Hex("FFF6D5");
            var pupil = Hex("1E1B26");

            const int segments = 12;
            var centers = new List<Vector3>();
            var radii = new List<float>();
            for (var i = 0; i <= segments; i++)
            {
                // 0 is the tail tip, 1 is the neck; the body thickens toward the head.
                var t = i / (float)segments;
                var radius = Mathf.Lerp(0.03f, 0.11f, Mathf.Sin(t * Mathf.PI * 0.5f));
                centers.Add(new Vector3(Mathf.Sin(t * Mathf.PI * 2f) * 0.16f, radius, Mathf.Lerp(-0.85f, 0.5f, t)));
                radii.Add(radius);
            }

            var b = new LowPolyMeshBuilder();
            b.AddTube(centers, radii, 6, scales, band);
            b.AddBlob(new Vector3(0f, 0.12f, 0.62f), new Vector3(0.15f, 0.10f, 0.19f), scales);
            foreach (var side in new[] { -1f, 1f })
            {
                b.AddBlob(new Vector3(side * 0.09f, 0.20f, 0.68f), Vector3.one * 0.04f, eye);
                b.AddBlob(new Vector3(side * 0.09f, 0.23f, 0.70f), Vector3.one * 0.02f, pupil);
            }
            return b.Build("Snake");
        }

        private static Mesh Tree()
        {
            var bark = Hex("7A5230");
            var b = new LowPolyMeshBuilder();
            b.AddTube(new[] { Vector3.zero, new Vector3(0f, 0.9f, 0f) }, new[] { 0.13f, 0.09f }, 6, bark, bark);
            b.AddCone(new Vector3(0f, 0.6f, 0f), 0.75f, 0.9f, 7, Hex("2F8F4E"));
            b.AddCone(new Vector3(0f, 1.1f, 0f), 0.58f, 0.8f, 7, Hex("3DA35D"));
            b.AddCone(new Vector3(0f, 1.55f, 0f), 0.40f, 0.7f, 7, Hex("57B86B"));
            return b.Build("Tree");
        }

        private static Mesh Rock()
        {
            var b = new LowPolyMeshBuilder();
            b.AddBlob(new Vector3(0f, 0.22f, 0f), new Vector3(0.50f, 0.32f, 0.42f), Hex("9AA0A6"), 1, 0.22f);
            b.AddBlob(new Vector3(0.36f, 0.12f, 0.22f), new Vector3(0.22f, 0.16f, 0.20f), Hex("858B92"), 1, 0.22f);
            return b.Build("Rock");
        }

        private static Mesh Bush()
        {
            var b = new LowPolyMeshBuilder();
            b.AddBlob(new Vector3(0f, 0.28f, 0f), new Vector3(0.42f, 0.32f, 0.40f), Hex("3DA35D"));
            b.AddBlob(new Vector3(0.30f, 0.20f, 0.12f), new Vector3(0.28f, 0.22f, 0.26f), Hex("57B86B"));
            b.AddBlob(new Vector3(-0.28f, 0.18f, -0.10f), new Vector3(0.26f, 0.20f, 0.24f), Hex("2F8F4E"));
            return b.Build("Bush");
        }

        private static Mesh Ground()
        {
            const float width = 80f;
            const float depth = 56f;
            const float cell = 2f;
            var light = Hex("8FD05A");
            var dark = Hex("79BF4C");
            // Far below the plane, so every triangle is wound to face up.
            var below = new Vector3(0f, -1000f, 0f);

            var b = new LowPolyMeshBuilder();
            for (var x = -width * 0.5f; x < width * 0.5f; x += cell)
            {
                for (var z = -depth * 0.5f; z < depth * 0.5f; z += cell)
                {
                    var p00 = new Vector3(x, 0f, z);
                    var p10 = new Vector3(x + cell, 0f, z);
                    var p11 = new Vector3(x + cell, 0f, z + cell);
                    var p01 = new Vector3(x, 0f, z + cell);
                    b.AddTriangle(p00, p01, p11, below, Color.Lerp(dark, light, Patch(x, z, 0f)));
                    b.AddTriangle(p00, p11, p10, below, Color.Lerp(dark, light, Patch(x, z, 1f)));
                }
            }
            return b.Build("Ground");
        }

        // A repeatable 0..1 value per triangle: patchy grass without a texture.
        private static float Patch(float x, float z, float half)
        {
            var value = Mathf.Sin(x * 12.9898f + z * 78.233f + half * 37.719f) * 43758.5453f;
            return value - Mathf.Floor(value);
        }

        // Vertex colours reach the shader untouched, so they are stored in linear space.
        private static Color Hex(string rgb)
        {
            ColorUtility.TryParseHtmlString("#" + rgb, out var color);
            return color.linear;
        }

        private static void Save(Mesh mesh)
        {
            var path = Folder + "/" + mesh.name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return;
            }

            // Overwriting in place keeps the GUID, so prefabs and the scene keep their mesh.
            EditorUtility.CopySerialized(mesh, existing);
            Object.DestroyImmediate(mesh);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
