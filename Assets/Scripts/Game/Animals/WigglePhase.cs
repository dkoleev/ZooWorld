using UnityEngine;

namespace ZooWorld.Game.Animals
{
    // Feeds the Toon shader's wiggle with the distance travelled nose-first, so the wave
    // advances only with movement and turning on the spot does not shift it.
    [RequireComponent(typeof(Renderer))]
    public class WigglePhase : MonoBehaviour
    {
        private static readonly int PhaseId = Shader.PropertyToID("_WigglePhase");

        private Renderer _renderer;
        private MaterialPropertyBlock _block;
        private Vector3 _lastPosition;
        private float _phase;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _block = new MaterialPropertyBlock();
        }

        // Pooled animals are re-enabled at a new spot; without this the jump would count as travel.
        private void OnEnable() => _lastPosition = transform.position;

        private void LateUpdate()
        {
            var position = transform.position;
            _phase += Vector3.Dot(position - _lastPosition, transform.forward);
            _lastPosition = position;

            // A property block opts this renderer out of the SRP Batcher; fine for a handful of snakes.
            _block.SetFloat(PhaseId, _phase);
            _renderer.SetPropertyBlock(_block);
        }
    }
}
