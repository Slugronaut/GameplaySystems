using UnityEngine;


namespace Toolbox.Game
{
    /// <summary>
    /// Used to mark a point in space that is processed by a spawner using the CullingGroup API.
    /// </summary>
    public class CulledSpawnPoint : MonoBehaviour
    {
        public float Radius = 1;
        public bool CanDisable;
        bool Disabled;

        private void OnEnable()
        {
            if (Disabled)
                SpawnPointPlacement.Instance.RestorePoint(this);
        }

        private void OnDisable()
        {
            if (CanDisable)
            {
                Disabled = true;
                SpawnPointPlacement.Instance.RemovePoint(this);
            }
        }

#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
#endif
    }

}