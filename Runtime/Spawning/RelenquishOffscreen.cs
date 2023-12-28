using Peg.AutoCreate;
using Peg.Lazarus;
using Peg.Lib;
using UnityEngine;
using UnityEngine.Events;

namespace Peg.Game.Spawning
{
    /// <summary>
    /// Relenquishes this entity when it goes offscreen.
    /// Based on viewport.
    /// </summary>
    public sealed class RelenquishOffscreen : MonoBehaviour
    {
        [Tooltip("How often to perform the viewport check.")]
        public float Interval = 1;

        public UnityEvent OnRelenquished;

        double LastTime;
        Transform Trans;
        Camera Cam;
        readonly IPoolSystem Lazarus = AutoCreator.AsSingleton<IPoolSystem>();

        void Start()
        {
            Trans = transform;
            Cam = Camera.main;
        }

        private void OnEnable()
        {
            LastTime = Time.timeAsDouble;
        }

        void Update()
        {
            var t = Time.timeAsDouble;
            if (t - LastTime > Interval)
            {
                LastTime = t;
                if (!MathUtils.IsInViewport(Cam, Trans.position, -0.2f, -0.2f))
                {
                    Lazarus.RelenquishToPool(gameObject);
                    OnRelenquished.Invoke();
                }
            }
        }

    }
}
