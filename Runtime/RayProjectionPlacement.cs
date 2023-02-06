using UnityEngine;


namespace Toolbox.Game
{
    /// <summary>
    /// Places a transform at a predicted location based on a ballistic raycast.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RayProjectionPlacement : MonoBehaviour
    {
        [Tooltip("The transform of the GameObject that will be positioned. If 'DisableIfNoContact' is enabled, this GameObject can be enabled/disabled as well based on the raycast results.")]
        public Transform Obj;
        [Tooltip("If no contact point is found, should the gameobject being placed be disabled?")]
        public bool DisableIfNoContact = true;
        [Tooltip("How far to raycast when searching for a contact point.")]
        public float RayLength = 50;
        [Tooltip("The layers to raycast on.")]
        public LayerMask Layers;
        [Tooltip("Should the object be placed with it's forward vector facing against the surface the raycast struck?")]
        public bool DecalPlacement = true;
        [Tooltip("Additional offset applied to the contact point.")]
        public Vector3 Offset;
        [Tooltip("Attempts to make a single calculation on being enabled. Will continue making attempts until a result is found.")]
        public bool CalculateOnce = true;
        [Tooltip("The amount of delay after being enabled before making a single attempt to find a contact point.")]
        public float CalculationDelay = 0.1f;


        bool Run;
        float StartTime;
        bool Located;
        Rigidbody Body;
        static TrajectorySimulationUtil.Contact Contact = new TrajectorySimulationUtil.Contact();



        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
        }

        public void OnEnable()
        {
            Run = false;
            StartTime = Time.time;
            Located = false;
            if (DisableIfNoContact)
                Obj.gameObject.SetActive(false);
        }
        
        public void Update()
        {
            if (Run) return;
            if (CalculateOnce)
            {
                if (Located) Run = true;
                else LinearStep(Body);
            }
        }

        public void FixedUpdate()
        {
            if (!CalculateOnce)
                LinearStep(Body);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        void LinearStep(Rigidbody body)
        {
            if (Time.time - StartTime < CalculationDelay)
                return;

            //early-out if we determine there isn't enough velocity to warrent checking yet
            if (body.velocity.sqrMagnitude < Thresholds.Four)
                return;

            var forward = body.velocity.normalized;
            var pos = Body.position;
            #if UNITY_EDITOR
            Debug.DrawRay(pos, forward * RayLength, Color.magenta, 3.0f);
            #endif
            bool result = Physics.Raycast(pos, forward, out RaycastHit hit, RayLength, Layers, QueryTriggerInteraction.Ignore);
            if(result)
                Contact.Set(hit.collider, hit.point, hit.normal);

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
