using Sirenix.OdinInspector;
using UnityEngine;

namespace Toolbox.Game
{
    /// <summary>
    /// General-purpose health script. This version supports replenishment of health over time.
    /// </summary>
    /// <remarks>
    /// It is important to note that the this class caches a
    /// copy of HealthChanged message for reuse. You should not
    /// hang on to the reference of any HealthChanged message after
    /// handling it as it may be volatile.
    /// </remarks>
    [AddComponentMenu("Toolbox/Game/Building Blocks/Replenishing Entity Resource")]
    public class ReplenishingEntityResource : EntityResource
    {
        public enum Mode
        {
            Points,
            Percent,
        }

        [Space(10)]
        [Header("Replenishing Resource")]
        [Tooltip("If set, this resource module will replenish based on the given data below. Don't use enable/disable since that completely disables changing values.")]
        public bool Running = true;

        [Tooltip("The period of time between replenishments.")]
        public float Period;

        [Tooltip("Should the replenishment and delay be independant of timescale?")]
        public bool IgnoreTimescale;

        [Tooltip("The amount to replenish each period.")]
        public float Amount;

        [Tooltip("Scales the amount that is replenished each tick.")]
        public float Multiplier = 1;


        [Tooltip("How the amount should be applied.")]
        public Mode ReplenMode;

        [Tooltip("Time in seconds after a reduction before regeneration kicks in.")]
        public float DelayAfterHit = 0;
        float LastHitTime;

        [Tooltip("When enabled, should it always be set to max?")]
        public bool EnableWithMax = true;


        float LastTime;

        protected override void OnEnable()
        {
            if(EnableWithMax) Current = Max;
            base.OnEnable();
        }

        /// <summary>
        /// 
        /// </summary>
        void Update()
        {
            if (IsDepleted || !Running) return;
            if (Amount > 0 && _Current >= _Max)
            {
                if (EnforceMax) _Current = _Max;
                return;
            }
            else if(Amount < 0 && _Current <= Min)
            {
                if (EnforceMin) _Current = Min;
                return;
            }

            //need to preserve last hit time
            float last = LastHitTime;

            if (IgnoreTimescale)
            {
                float t = Time.unscaledTime;
                if (t - LastTime > Period && t - last > DelayAfterHit)
                {
                    LastTime = t;
                    if (ReplenMode == Mode.Points) Current += Amount * Multiplier;
                    else CurrentPercent += Amount * Multiplier;
                }
            }
            else
            {
                float t = Time.time;
                if (t - LastTime > Period && t - last > DelayAfterHit)
                {
                    LastTime = t;
                    if (ReplenMode == Mode.Points) Current += Amount * Multiplier;
                    else CurrentPercent += Amount * Multiplier;
                }
            }

            if (Current > Max && EnforceMax) Current = Max;
            if (Current < Min && EnforceMin) Current = Min;
            LastHitTime = last;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="hp"></param>
        /// <param name="suppressEvents"></param>
        public override void SetCurrent(GameObject agent, float value, bool suppressEvents = false)
        {
            base.SetCurrent(agent, value, suppressEvents);
            if (IgnoreTimescale) LastHitTime = Time.unscaledTime;
            else LastHitTime = Time.time;
        }


    }
}
