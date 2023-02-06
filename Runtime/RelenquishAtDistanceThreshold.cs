using UnityEngine;
using UnityEngine.Events;
using LazarusPool = Toolbox.Lazarus.Lazarus;

namespace Toolbox.Behaviours
{
    /// <summary>
    /// Relenquishes this entity when it reaches the given distance to the nearest CoU.
    /// </summary>
    public sealed class RelenquishAtDistanceThreshold : MonoBehaviour
    {
        [Tooltip("The CoU id to check. If empty, all CoUs are checked.")]
        public HashedString CouId;

        [Tooltip("The distance to the nearest CoU beyond which this entity will despawn.")]
        public float Threshold = 75;

        [Tooltip("How often to perform the distance check.")]
        public float Interval = 1;

        public UnityEvent OnRelenquished;

        float LastTime;
        Transform Trans;

        void Start()
        {
            Trans = transform;
        }

        void Update()
        {
            float t = Time.time;
            if (t - LastTime > Interval)
            {
                LastTime = t;
                if (CouId.Hash == 0)
                {
                    if (!CenterOfUniverse.IsClosestWithinDistance(Trans.position, Threshold))
                    {
                        LazarusPool.Instance.RelenquishToPool(gameObject);
                        OnRelenquished.Invoke();
                    }
                }
                else
                {
                    if (!CenterOfUniverse.IsClosestWithinDistance(CouId.Hash, Trans.position, Threshold))
                    {
                        LazarusPool.Instance.RelenquishToPool(gameObject);
                        OnRelenquished.Invoke();
                    }
                }
            }
        }

    }
}
