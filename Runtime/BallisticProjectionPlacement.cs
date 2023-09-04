using UnityEngine;


namespace Peg.Game
{
    /// <summary>
    /// Places a transform at a predicted location based on a ballistic raycast.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BallisticProjectionPlacement : MonoBehaviour
    {

        public enum Methods
        {
            Euler,
            Heun,
            SegmentedArc,
        }

        [Tooltip("The transform of the GameObject that will be positioned. If 'DisableIfNoContact' is enabled, this GameObject can be enabled/disabled as well based on the raycast results.")]
        public Transform Obj;
        [Tooltip("If no contact point is found, should the gameobject being placed be disabled?")]
        public bool DisableIfNoContact = true;
        [Tooltip("The length of a single segment when raycasting the trajectory. Smaller values are more computationally intensive but give more accurate results.")]
        static float SegmentLength = 1;
        [Tooltip("The number of subdivisions to apply to the arc when calculating using the Segmented Arc integrator.")]
        public int Subdivisions = 5;
        [Tooltip("The maximum allowed length of the trajectory raycast. Keep this as small as possible when using Segmented Arc!!!")]
        public float MaxLength = 50;
        [Tooltip("The layers to raycast on.")]
        public LayerMask Layers;
        [Tooltip("Should the object be placed with it's forward vector facing against the surface the raycast struck?")]
        public bool DecalPlacement = true;
        [Tooltip("Additional offset applied to the contact point.")]
        public Vector3 Offset;
        [Tooltip("Attempts to make a single calculation on being enabled. Will continue making attempts until a result is found.")]
        public bool CalculateOnce = true;
        [Tooltip("Multiplier applied to the fixed timestep used in the ballistic projection fomula.")]
        public float DeltaTimeMultiplier = 100.0f;
        [Tooltip("A delay before attempt to determine where this projectile will land. Useful when using 'CalculateOnce' due to the fact that the fixed timestep may occur before this object's velocity has been properly set.")]
        public float DelayToStart = 0.2f;


        float StartTime;
        bool Located;
        Rigidbody Body;
        static TrajectorySimulationUtil.Contact Contact = new TrajectorySimulationUtil.Contact();
        bool Run;


        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
        }

        public void OnEnable()
        {
            Run = false;
            StartTime = Time.time;
            Located = false;
            if(DisableIfNoContact)
                Obj.gameObject.SetActive(false);
        }
        
        public void Update()
        {
            if (Run) return;
            if (CalculateOnce)
            {
                if (Located) Run = true;
                else ArcStep(Body);
            }

        }

        public void FixedUpdate()
        {
            if (!CalculateOnce)
                ArcStep(Body);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        void ArcStep(Rigidbody body)
        {
            if (Time.time - StartTime < DelayToStart)
                return;

            //early-out if we determine there isn't enough velocity to warrent checking yet
            if (body.velocity.sqrMagnitude < Thresholds.Five)
                return;
            
            UpdateObject(Contact, TrajectorySimulationUtil.RaycastBallisticArc(Contact, body, Layers, MaxLength, Subdivisions, DeltaTimeMultiplier));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bod"></param>
        void HeunStep(Rigidbody body)
        {
            var forward = body.velocity;
            if (forward.sqrMagnitude < 0.01)
                forward = Obj.forward;
            else forward.Normalize();

            bool result = TrajectorySimulationUtil.RaycastBallistic_Heun(Contact, body.position, forward, Layers, body.velocity.magnitude, MaxLength, SegmentLength);
            UpdateObject(Contact, result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bod"></param>
        void EulerStep(Rigidbody body)
        {
            var forward = body.velocity;
            if (forward.sqrMagnitude < 0.01)
                forward = Obj.forward;
            else forward.Normalize();

            bool result = TrajectorySimulationUtil.RaycastBallistic(Contact, body.position, forward, Layers, body.velocity.magnitude, MaxLength, SegmentLength);
            UpdateObject(Contact, result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="hit"></param>
        void UpdateObject(TrajectorySimulationUtil.Contact contact, bool hit)
        {
            if (hit)
            {
                if (Located)
                {
                    if (Vector3.Distance(contact.Point + Offset, Obj.position) > Thresholds.One)
                        Located = false;
                }

                if (!Located)
                {
                    if (DisableIfNoContact && !Obj.gameObject.activeSelf)
                        Obj.gameObject.SetActive(true);

                    if (DecalPlacement)
                    {
                        Obj.forward = -contact.Normal;
                        Quaternion q = Quaternion.FromToRotation(Vector3.up, contact.Normal);
                        Obj.position = contact.Point + (q * Offset);
                    }
                    else
                    {
                        Obj.position = contact.Point + Offset;
                    }
                    Located = true;
                }
            }
            else if (DisableIfNoContact && Obj.gameObject.activeSelf)
            {
                Located = false;
                Obj.gameObject.SetActive(false);
            }
        }
    }
}
