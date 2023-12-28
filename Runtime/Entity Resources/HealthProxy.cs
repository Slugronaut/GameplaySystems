using UnityEngine;


namespace Peg.Game.ConsumableResource
{
    /// <summary>
    /// Used as a dummy that can redirect to an entity's actual health.
    /// </summary>
    /// <remarks>
    /// This is needed because we often want to collect data on raw GameObjects
    /// during physics collisions but we don't know what child we're on due
    /// to the need for multiple children with different collision layers.
    /// </remarks>
    public class HealthProxy : MonoBehaviour, IHealthProxy
    {
        [SerializeField]
        public Health _HealthSource;

        public Health HealthSource { get { return _HealthSource; } }
    }
}
