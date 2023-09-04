using UnityEngine;


namespace Peg.Game
{
    /// <summary>
    /// Useful for marking points that need to be discovered at runtime
    /// without access to prior info.
    /// </summary>
    public class Hardpoint : MonoBehaviour
    {
        [Tooltip("User-friendly, descriptive name of this hardpoint.")]
        public HashedString Id;
        

        #if UNITY_EDITOR
        public Color GizmoColor = new Color(1, 0, 1, 0.33334f);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = GizmoColor;
            Gizmos.DrawSphere(transform.position, 0.2f);
        }
        #endif
    }
}