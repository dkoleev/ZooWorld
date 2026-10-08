using UnityEditor;
using UnityEngine;

namespace ZooWorld.Editor
{
    public static class CameraGroundBoundsGizmo
    {
        private static readonly Vector2[] Corners = { new(0f, 0f), new(0f, 1f), new(1f, 1f), new(1f, 0f) };

        // Outline of what the camera sees on the ground plane (y = 0), same projection as CameraPlayArea.
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void Draw(Camera camera, GizmoType gizmoType)
        {
            var points = new Vector3[Corners.Length];
            for (var i = 0; i < Corners.Length; i++)
            {
                var ray = camera.ViewportPointToRay(Corners[i]);
                if (ray.direction.y > -0.01f)
                    return;

                points[i] = ray.GetPoint(-ray.origin.y / ray.direction.y);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawLineStrip(points, true);
        }
    }
}
