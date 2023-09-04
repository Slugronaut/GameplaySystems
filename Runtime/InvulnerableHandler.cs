using UnityEngine;


namespace Peg.Game
{
    /// <summary>
    /// Provides a simple interface for invoking invulnerability using UnityEvents.
    /// </summary>
    public class InvulnerableHandler : MonoBehaviour
    {
        [Tooltip("How long the effect will last.")]
        public float Time = 1;
        [Tooltip("The Health object that will be affected.")]
        public Health Health;

        /// <summary>
        /// A simple method that can be rigged to a UnityEvent.
        /// </summary>
        public void Invoke()
        {
            Health.ProcInvincibility(Time);
        }
    }
}
