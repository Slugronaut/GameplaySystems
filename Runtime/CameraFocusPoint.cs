using System.Collections;
using Peg.AutonomousEntities;
using Peg.Trackables;
using UnityEngine;


namespace Peg.Game
{
    /// <summary>
    /// Used to adjust the x, y, and z, axis on which the camera is centered. The motion
    /// can be blended over time and allows the camera to adjust even on axies that are locked
    /// in the smooth motion tracker.
    /// </summary>
    public class CameraFocusPoint : Peg.AbstractOperationOnEvent
    {
        static Coroutine Co;
        static float Threshold = 0.5f;

        [Tooltip("Optional camera. If left null, the main camera will be used.")]
        public Camera CameraOverride;
        [Tooltip("How long does it take for lock axies to reach their target?")]
        public float BlendTime;
        [Tooltip("Does the x-axis of the camera lock to this object's x-axis?")]
        public bool X;
        [Tooltip("Does the x-axis of the camera lock to this object's x-axis?")]
        public bool Y;
        [Tooltip("Does the x-axis of the camera lock to this object's x-axis?")]
        public bool Z;

        /// <summary>
        /// A list of gameobject tags that are allowed to trigger this event if used as a physical trigger.
        /// </summary>
        public string[] AllowedTags;

        SmoothFollowTrackables Tracker;


        protected override void Awake()
        {
            base.Awake();
            if (CameraOverride == null)
                CameraOverride = Camera.main;

            Tracker = CameraOverride.gameObject.FindComponentInEntity<SmoothFollowTrackables>();
        }

        #if TOOLBOX_2DCOLLIDER
        void OnTriggerEnter2D(Collider2D other)
        #else
        void OnTriggerEnter(Collider other)
        #endif
        {
            Focus(other.gameObject);
        }

        void Focus(GameObject other)
        {
            if (TriggerPoint == EventAndCollisionTiming.Triggered)
            {
                if (AllowedTags == null || AllowedTags.Length < 0)
                    TryPerformOp();
                else
                {
                    for (int i = 0; i < AllowedTags.Length; i++)
                    {
                        if (other.CompareTag(AllowedTags[i]))
                            TryPerformOp();
                    }
                }
            }
        }

        public override void PerformOp()
        {
            if (Mathf.Approximately(BlendTime,0))
                Tracker.BlendFrom = MaskedPos(Tracker.BlendFrom, transform.position, X, Y, Z);
            else
            {
                if (Co != null)
                    StopCoroutine(Co);
                Co = StartCoroutine(BlendPosition());
            }
        }
        
        Vector3 CurrVel;
        IEnumerator BlendPosition()
        {
            Transform trackerTrans = Tracker.transform;
            var myTrans = transform;
            CurrVel = Vector3.zero;

            while(true)
            {
                var myPos = myTrans.position;
                var trackPos = trackerTrans.position;
                var blendTarget = MaskedPos(trackPos, myPos, X, Y, Z);
                var pos = Vector3.SmoothDamp(trackPos, blendTarget, ref CurrVel, BlendTime);
                trackerTrans.position = pos;
                Tracker.BlendFrom = pos;

                bool xReady = true;
                bool yReady = true;
                bool zReady = true;
                if (X)
                    xReady = (Mathf.Abs(myPos.x - pos.x) < Threshold) ? true : false;
                if (Y)
                    yReady = (Mathf.Abs(myPos.y - pos.y) < Threshold) ? true : false;
                if (Z)
                    zReady = (Mathf.Abs(myPos.z - pos.z) < Threshold) ? true : false;
                if (xReady && yReady && zReady)
                    break;


                yield return null;
            }
            Co = null;
        }

        /// <summary>
        /// Helper method that chooses the components of a vector based on a mask for each axis.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="updated"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 MaskedPos(Vector3 original, Vector3 updated, bool x, bool y, bool z)
        {
            return new Vector3(!x ? original.x : updated.x,
                               !y ? original.y : updated.y,
                               !z ? original.z : updated.z);
        }

        public void OnDrawGizmos()
        {
            float scale = 2.0f;
            var pos = transform.position;
            Gizmos.color = Color.green;
            Gizmos.DrawRay(pos, Vector3.right * scale);
            Gizmos.DrawRay(pos, Vector3.up * scale);
            Gizmos.DrawRay(pos, Vector3.forward * scale);
        }
    }
}
