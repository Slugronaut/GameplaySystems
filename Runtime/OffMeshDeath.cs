using UnityEngine;

namespace Toolbox.Game
{
    public class OffMeshDeath : MonoBehaviour
    {
        static readonly float DeathTimer = 0.25f;

        UnityEngine.AI.NavMeshAgent Agent;
        bool Killing;

        private void OnDisable()
        {
            Killing = false;
            CancelInvoke();
        }

        void Awake()
        {
            Agent = gameObject.FindComponentInEntity<UnityEngine.AI.NavMeshAgent>();
            if (Agent == null) Destroy(this);
        }

        void Update()
        {
            if (Agent.isActiveAndEnabled && !Agent.isOnNavMesh && !Killing)
            {
                Invoke("KillIfOffMesh", DeathTimer);
                Killing = true;
            }
        }

        void KillIfOffMesh()
        {
            Debug.Log("The fuck, bruh!?");
            if (!Agent.isOnNavMesh && Agent.isActiveAndEnabled && isActiveAndEnabled)
            {
                Health h = gameObject.FindComponentInEntity<Health>(true);
                if (h != null) GlobalMessagePump.Instance.ForwardDispatch<KillEntityCmd>(h.gameObject, new KillEntityCmd(null, h.gameObject));
                else Destroy(gameObject);
            }
            else Killing = false;

        }
    }
}
