using Peg.AutoCreate;
using Peg.Lazarus;
using Peg.Lib;
using UnityEngine;
using UnityEngine.Events;

namespace Peg.Behaviours
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

        float LastTime;
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
            LastTime = Time.time;
        }

        void Update()
        {
            float t = Time.time;
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
