using System;
using Peg.AutoCreate;
using Peg.Behaviours;
using Peg.Util;
using Peg.Lazarus;
using Peg.Lib;
using Peg.Messaging;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Peg.Game
{
    /// <summary>
    /// Does what it says. Has a statically defined internal limit of affecting up to 50 entities.
    /// </summary>
    public sealed class ExplodeOnMessage : AbstractMessageReciever
    {
        public enum DestroyOption
        {
            None,
            Destroy,
            Relenquish,
        }

        [Tooltip("Time before exploding. Only works for messages and events. Calling 'Explode' directly will not delay.")]
        public float Delay = 0;
        public float ForceMin = 10;
        public float ForceMax = 10;
        public float DamageMin = 0;
        public float DamageMax = 0;
        public AnimationCurve DamageDropoff = AnimationCurve.EaseInOut(0, 1, 1, 0);

        public QueryTriggerInteraction TriggerInteraction;
        public float Radius = 10;
        public LayerMask Layers;
        public ParticleSystem Particles;
        public AudioClip[] AudioClips;
        public ushort AudioIndex = 0;
        public DestroyOption ActionOnDestroy;
        public bool DeparentParticles = false;
        public static int MaxColliders = 50;

        IPoolSystem Lazarus;


        protected override void Awake()
        {
            base.Awake();
            Lazarus = AutoCreator.AsSingleton<IPoolSystem>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msgType"></param>
        /// <param name="msg"></param>
        protected override void HandleMessage(Type msgType, object msg)
        {
            if (Delay > 0) Invoke(nameof(Explode), Delay);
            else Explode();
        }

        /// <summary>
        /// Helper method for allowing attaching directly to Health's unity events.
        /// </summary>
        /// <param name="health"></param>
        /// <param name="diff"></param>
        public void OnHealthChanged(Health health, int diff)
        {
            if (Delay > 0) Invoke(nameof(Explode), Delay);
            else Explode();
        }

        /// <summary>
        /// Causes this object to perform an explosion at it's center.
        /// </summary>
        public void Explode()
        {
            var pos = transform.position;

            //do the light stuff first
            if (AudioClips.Length > 0)
                TempAudioSourcePlayer.Instance.Play((int)AudioIndex, AudioClips[Random.Range(0, AudioClips.Length)], pos);
            
            //now the physics
            var cols = SharedArrayFactory.RequestTempArray<Collider>(MaxColliders);
            int hits = Physics.OverlapSphereNonAlloc(pos, Radius, cols, Layers, TriggerInteraction);
            float f = Random.Range(ForceMin, ForceMax);

            //one last chance to bail...
            if (Radius > 0)
            {
                //yuck, this gets really nasty!
                for (int i = 0; i < hits; i++)
                {
                    if (cols[i].TryGetComponent<Rigidbody>(out var bod))
                        bod.AddExplosionForce(f, pos, Radius);

                    if (DamageMax > 0)
                    {
                        IHealthProxy hpp = cols[i].GetComponent<IHealthProxy>();
                        if (hpp != null)
                        {
                            var hp = hpp.HealthSource;
                            if (hp != null)
                            {
                                float dist = Vector3.Distance(hp.transform.position, pos);
                                hp.CurrentHealth -= (int)MathUtils.EvaluateCurveScale(dist / Radius, DamageMin, DamageMax, DamageDropoff);
                            }
                        }
                    }
                }
            }

            if (ActionOnDestroy == DestroyOption.Destroy)
            {
                if (Particles != null) Particles.transform.SetParent(null, true);
                Destroy(gameObject);
            }
            else if (ActionOnDestroy == DestroyOption.Relenquish)
            {
                if (Particles != null) Particles.transform.SetParent(null, true);
                Lazarus.RelenquishToPool(gameObject);
            }
            else if (DeparentParticles && Particles != null)
                Particles.transform.SetParent(null, true);

            if (Particles != null) Particles.Play(true);

        }

        
    }

    
}
