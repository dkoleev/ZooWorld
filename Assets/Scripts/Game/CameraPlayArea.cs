using UnityEngine;
using ZooWorld.Core;
using ZooWorld.Core.World;

namespace ZooWorld.Game
{
    public class CameraPlayArea : IPlayArea
    {
        public Vector3 Center
        {
            get
            {
                Refresh();
                return _center;
            }
        }
        
        private readonly Camera _camera;
        private readonly float _margin;
        private int _frame = -1;
        private bool _errorReported;
        private Vector3 _center;
        private float _halfWidth;
        private float _halfDepth;
        
        public CameraPlayArea(Camera camera, GameSettings settings)
        {
            _camera = camera;
            _margin = settings.PlayAreaMargin;
        }
        
        public bool Contains(Vector3 position)
        {
            Refresh();
            return Mathf.Abs(position.x - _center.x) <= _halfWidth
                   && Mathf.Abs(position.z - _center.z) <= _halfDepth;
        }

        public Vector3 GetRandomPoint(IRandom random)
        {
            Refresh();
            return _center + new Vector3(
                random.Range(-_halfWidth, _halfWidth), 0f, random.Range(-_halfDepth, _halfDepth));
        }
        
        // Projected once per frame however many animals ask: cheap, and it follows a resized
        // window or a moved camera without anyone having to say so.
        private void Refresh()
        {
            if (_frame == Time.frameCount)
                return;
            
            _frame = Time.frameCount;

            if (!TryGetGroundPoint(Vector2.zero, out var min) || !TryGetGroundPoint(Vector2.one, out var max))
            {
                // The last good area stays in use, and the next frame tries again.
                if (!_errorReported)
                    Debug.LogError("CameraPlayArea needs a camera that looks down at the ground plane (y = 0).", _camera);
                _errorReported = true;
                
                return;
            }

            _errorReported = false;
            _center = (min + max) * 0.5f;
            _halfWidth = Mathf.Max(0f, Mathf.Abs(max.x - min.x) * 0.5f - _margin);
            _halfDepth = Mathf.Max(0f, Mathf.Abs(max.z - min.z) * 0.5f - _margin);
        }

        private bool TryGetGroundPoint(Vector2 viewport, out Vector3 point)
        {
            var ray = _camera.ViewportPointToRay(viewport);
            if (ray.direction.y > -0.01f)
            {
                point = default;
                return false;
            }

            point = ray.GetPoint(-ray.origin.y / ray.direction.y);
            return true;
        }
    }
}