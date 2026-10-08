using UnityEditor;
using UnityEngine;

namespace ZooWorld.Editor
{
    internal class PropPlacerWindow : EditorWindow
    {
        private const string DefaultMaterial = "Assets/Art/Materials/Toon.mat";

        private SerializedObject _settings;
        private Transform _root;
        private Mesh _selected;
        private GameObject _preview;
        private bool _removing;
        private GameObject _hovered;

        private static PropPlacerSettings Settings => PropPlacerSettings.instance;

        [MenuItem("Zoo World/Prop Placer")]
        private static void Open() => GetWindow<PropPlacerWindow>("Prop Placer");

        private void OnEnable()
        {
            // Singletons are created not editable, which greys out their fields in the window.
            Settings.hideFlags &= ~HideFlags.NotEditable;
            if (Settings.material == null)
                Settings.material = AssetDatabase.LoadAssetAtPath<Material>(DefaultMaterial);

            _settings = new SerializedObject(Settings);
            SceneView.duringSceneGui += OnSceneGui;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGui;
            Select(null);
        }

        private void OnGUI()
        {
            _settings.Update();
            var rootId = _settings.FindProperty(nameof(PropPlacerSettings.rootId));

            // The root lives in the scene, so only its id survives an editor restart.
            if (_root == null && GlobalObjectId.TryParse(rootId.stringValue, out var id))
                _root = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as Transform;

            EditorGUI.BeginChangeCheck();
            _root = (Transform)EditorGUILayout.ObjectField("Root", _root, typeof(Transform), true);
            if (EditorGUI.EndChangeCheck())
                rootId.stringValue = _root != null ? GlobalObjectId.GetGlobalObjectIdSlow(_root).ToString() : "";

            EditorGUILayout.PropertyField(_settings.FindProperty(nameof(PropPlacerSettings.material)));
            EditorGUILayout.PropertyField(_settings.FindProperty(nameof(PropPlacerSettings.randomYaw)));
            EditorGUILayout.PropertyField(_settings.FindProperty(nameof(PropPlacerSettings.scaleRange)));
            EditorGUILayout.PropertyField(_settings.FindProperty(nameof(PropPlacerSettings.meshes)), true);

            if (_settings.ApplyModifiedProperties())
                Settings.Save();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Place / Remove (click again or Esc to stop)", EditorStyles.boldLabel);
            foreach (var mesh in Settings.meshes)
            {
                if (mesh == null)
                    continue;

                var isOn = GUILayout.Toggle(_selected == mesh, mesh.name, "Button", GUILayout.Height(28f));
                if (isOn != (_selected == mesh))
                    Select(isOn ? mesh : null);
            }

            EditorGUILayout.Space();
            var removing = GUILayout.Toggle(_removing, "Remove", "Button", GUILayout.Height(28f));
            if (removing != _removing)
            {
                Select(null);
                _removing = removing;
            }
        }

        private void OnSceneGui(SceneView view)
        {
            if (_preview == null && !_removing)
                return;

            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                Select(null);
                e.Use();
                return;
            }

            // Claims the click, otherwise the scene view selects whatever is under the cursor.
            if (e.type == EventType.Layout)
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (_removing)
            {
                Remove(view, e);
                return;
            }

            var onGround = Physics.Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out var hit);
            _preview.SetActive(onGround);
            if (onGround)
                _preview.transform.position = hit.point;

            if (e.type == EventType.MouseMove)
                view.Repaint();

            // Alt + drag stays free for orbiting the camera.
            if (onGround && e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                Place();
                e.Use();
            }
        }

        private void Remove(SceneView view, Event e)
        {
            if (e.type == EventType.MouseMove)
            {
                _hovered = PickProp(e.mousePosition);
                view.Repaint();
            }

            if (_hovered != null && e.type == EventType.Repaint && _hovered.TryGetComponent<Renderer>(out var renderer))
            {
                Handles.color = Color.red;
                Handles.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
            }

            if (_hovered != null && e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                Undo.DestroyObjectImmediate(_hovered);
                e.Use();
            }
        }

        // Only what this tool could have placed is removable: a mesh from the list, under the root when one is set.
        private GameObject PickProp(Vector2 mousePosition)
        {
            var picked = HandleUtility.PickGameObject(mousePosition, false);
            if (picked == null || !picked.TryGetComponent<MeshFilter>(out var filter))
                return null;

            if (!Settings.meshes.Contains(filter.sharedMesh) || (_root != null && !picked.transform.IsChildOf(_root)))
                return null;

            return picked;
        }

        private void Select(Mesh mesh)
        {
            if (_preview != null)
                DestroyImmediate(_preview);

            _removing = false;
            _hovered = null;
            _selected = mesh;
            if (mesh != null)
                _preview = CreatePreview();

            SceneView.RepaintAll();
            Repaint();
        }

        // The preview is the real object, only hidden: placing it means revealing it, so what you see is what you get.
        private GameObject CreatePreview()
        {
            var preview = new GameObject(_selected.name) { hideFlags = HideFlags.HideAndDontSave };
            preview.AddComponent<MeshFilter>().sharedMesh = _selected;
            preview.AddComponent<MeshRenderer>().sharedMaterial = Settings.material;
            if (Settings.randomYaw)
                preview.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            preview.transform.localScale = Vector3.one * Random.Range(Settings.scaleRange.x, Settings.scaleRange.y);
            preview.SetActive(false);
            return preview;
        }

        private void Place()
        {
            _preview.hideFlags = HideFlags.None;
            _preview.transform.SetParent(_root, true);
            Undo.RegisterCreatedObjectUndo(_preview, "Place " + _preview.name);
            _preview = CreatePreview();
        }
    }
}
