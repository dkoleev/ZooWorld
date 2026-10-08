using UnityEngine;

namespace ZooWorld.Game.Game.Debugging
{
    /// <summary>Debug-only speed switch: editor and Development Builds get it, release builds do not.</summary>
    public class DebugTimeScale : MonoBehaviour
    {
        // plain Time.timeScale, so high speeds run that many physics steps per frame and
        // are capped by Time.maximumDeltaTime; drive AnimalWorld.Tick in a loop if real fast-forward is needed.
        private static readonly float[] Speeds = { 0f, 0.5f, 1f, 2f, 5f, 10f };
        private static readonly string[] Labels = { "||", "x0.5", "x1", "x2", "x5", "x10" };

        private int _selected = 2;

        [RuntimeInitializeOnLoadMethod]
        private static void Create()
        {
            if (!Debug.isDebugBuild)
                return;

            DontDestroyOnLoad(new GameObject(nameof(DebugTimeScale), typeof(DebugTimeScale)));
        }

        private void OnGUI()
        {
            // Keeps the buttons a usable size on high-resolution screens.
            var scale = Mathf.Max(1f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);

            const float width = 300f, height = 32f, margin = 4f;
            GUILayout.BeginArea(new Rect(margin, Screen.height / scale - height - margin, width, height));
            var selected = GUILayout.Toolbar(_selected, Labels, GUILayout.Height(height));
            GUILayout.EndArea();
            if (selected == _selected)
                return;

            _selected = selected;
            Time.timeScale = Speeds[selected];
        }
    }
}
