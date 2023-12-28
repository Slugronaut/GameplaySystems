using Peg.AutonomousEntities;
using Peg.Trackables;
using UnityEngine;


namespace Peg.Game
{
    /// <summary>
    /// Used to adjust the tracking speed of a smooth motion tracking camera.
    /// The adjustments can be made in relative or absolute terms.
    /// </summary>
    public class CameraTrackingAdjustment: Peg.AbstractOperationOnEvent
    {
        public enum AdjustmentType
        {
            Relative,
            Absolute,
        }

        [Tooltip("Optional camera. If left null, the main camera will be used.")]
        public Camera CameraOverride;
        [Tooltip("Is the adjustment speed a relative change or an absolute change?")]
        public AdjustmentType Type;
        [Tooltip("The relative or absolute adjustment to trackng speed for the camera.")]
        public Vector3 Adjustment = new Vector3(7.5f, 1.0f, 0.0f);
        

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
            if (Type == AdjustmentType.Absolute)
            {
                Tracker.X = Adjustment.x;
                Tracker.Y = Adjustment.y;
                Tracker.Z = Adjustment.z;
            }
            else if (Type == AdjustmentType.Relative)
            {
                Tracker.X += Adjustment.x;
                Tracker.Y += Adjustment.y;
                Tracker.Z += Adjustment.z;
            }
        }
        
    }
}
