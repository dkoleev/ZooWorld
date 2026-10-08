using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZooWorld.Editor.Placer
{
    [FilePath("UserSettings/PropPlacer.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class PropPlacerSettings : ScriptableSingleton<PropPlacerSettings>
    {
        public List<Mesh> meshes = new();
        public Material material;
        public string rootId;
        public bool randomYaw = true;
        public Vector2 scaleRange = Vector2.one;

        public void Save() => Save(true);
    }
}
