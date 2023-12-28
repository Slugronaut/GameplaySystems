using UnityEngine;
using UnityEngine.Events;

namespace Peg.Game.ConsumableResource
{
    /// <summary>
    /// Attach to an entity that should die if it is detected that they are falling too fast.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public class DeathByFalling : MonoBehaviour
    {
        [Tooltip("If our vertical veocity reaches this speed it will be assumed that we have fallen too far too survive and the death event will be triggered.")]
        public float DeathSpeed = 25;
        [Tooltip("The distance to scan for ground below us. If no ground is detected, death is immediate. If ground is detected and our speed reaches the above limit, death still occurs.")]
        public float ScanDistance = 100;
        [Tooltip("Layser mask to check for ground. If no ground is detected, death is immediate. If ground is detected and our speed reaches the above limit, death still occurs.")]
        public LayerMask FloorMask;
        public float Radius = 2;
        Rigidbody Body;
        CharacterController Controller;
        //EntityRoot Root;

        public UnityEvent OnDie;

        void Awake()
        {
            //Root = gameObject.GetEntityRoot();
            Body = GetComponent<Rigidbody>();
            Controller = GetComponent<CharacterController>();
        }

        private void OnDisable()
        {
            if(Body != null) Body.velocity = Vector3.zero;
            if(Controller != null && Controller.gameObject.activeInHierarchy && Controller.enabled)
                Controller.SimpleMove(Vector3.zero);
        }

        void Update()
        {
            //if our vertical velocity gets obsurdly high we can infer we are falling
            //even if we aren't it should look ok if we die or take damage due to such speeds
            float s = 0;
            if (Controller != null)
                s = -Controller.velocity.y;
            else if(Body != null)
                s = -Body.velocity.y;
            if (s > DeathSpeed)
            {
                if (!Physics.SphereCast(transform.position + (Vector3.up * ((2 * Radius) + 0.01f)), Radius, Vector3.down, out RaycastHit rayHit, ScanDistance, FloorMask))
                    OnDie.Invoke();
                //CombatCalculator.ForceKill(null, Root, false);

            }
        }
    }
}
