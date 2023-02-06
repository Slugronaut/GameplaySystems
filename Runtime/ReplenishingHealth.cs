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
    [AddComponentMenu("Toolbox/Game/Building Blocks/Health - Replenishing")]
    [DisallowMultipleComponent]
    public class ReplenishingHealth : Health
    {
        public enum Mode
        {
            Points,
            Percent,
        }

        [Space(10)]
        [Header("Replenishing Health")]
        [Tooltip("If set, this health module will replenish health based on the given data below. Don't use enable/disable since that completely disables taking damage.")]
        public bool Running = true;

        [Tooltip("The period of time between replenishments.")]
        public float Period;
        
        [Tooltip("Should the replenishment and delay be independant of timescale?")]
        public bool IgnoreTimescale;

        [Tooltip("The amount to replenish each period.")]
        public float Amount;

        [Tooltip("How the amount should be applied.")]
        public Mode ReplenMode;

        [Tooltip("Time in seconds after taking damage before regeneration kicks in.")]
        public float DelayAfterHit = 0;
        float LastHitTime;


        float LastTime;

        /// <summary>
        /// 
        /// </summary>
        void Update()
        {
            if (IsDead || !Running || _CurrentHealth >= _MaxHealth) return;

            //need to preserve last hit time
            float last = LastHitTime;

            if(IgnoreTimescale)
            {
                float t = Time.unscaledTime;
                if (t - LastTime > Period && t - last > DelayAfterHit)
                {
                    LastTime = t;
                    if (ReplenMode == Mode.Points) Current += Amount;
                    else CurrentPercent += Amount;
                }
            }
            else
            {
                float time = Time.time;
                if (time - LastTime > Period && time - last > DelayAfterHit)
                {
                    LastTime = time;
                    if (ReplenMode == Mode.Points) Current += Amount;
                    else CurrentPercent += Amount;
                }
            }

            LastHitTime = last;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="hp"></param>
        /// <param name="suppressEvents"></param>
        public override void SetHealth(GameObject agent, int hp, bool suppressEvents = false, bool ignoreGodmode = false)
        {
            base.SetHealth(agent, hp, suppressEvents, ignoreGodmode);
            if (IgnoreTimescale) LastHitTime = Time.unscaledTime;
            else LastHitTime = Time.time;
        }


    }
}
