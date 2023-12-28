using System;
using Peg.AutonomousEntities;
using Peg.Game.ConsumableResource;
using Peg.Messaging;
using UnityEngine;
using UnityEngine.Events;

namespace Peg.Game.Spawning
{
    /// <summary>
    /// Attach this to an entity that is spawned at runtime so that
    /// it can let its spawner track its lifetime.
    /// </summary>
    [DisallowMultipleComponent]
    public class SpawnedEntity : LocalListenerMonoBehaviour
    {
        [Serializable]
        public class SpawnEvent : UnityEvent<SpawnedEntity, ISpawner> { }

        //TODO: These three bools should be packed into a single byte and accessed with properties.
        //If used on say, 1k entities that's the difference between 3kb and 1kb of ram!
        [Tooltip("Does this entity let its spawner know it has 'despawned' due to being disabled.")]
        public bool TriggerOnDisable;
        [Tooltip("Does this entity let its spawner know it has 'despawned' due to being relenquished.")]
        public bool TriggerOnRelenquish;
        [Tooltip("Does this entity let its spawner know it has 'despawned' due to dying.")]
        public bool TriggerOnDeath;
        public SpawnEvent OnSpawn;
        public SpawnEvent OnDespawn;
        

        public EntityRoot Root { get; private set; }

        /// <summary>
        /// Set automatically by spawner. Do not change unless you have a good reason.
        /// </summary>
        [HideInInspector]
        public ISpawner Spawner { get; protected set; }

        
        void Awake()
        {
            Root = gameObject.GetEntityRoot();
            DispatchRoot.AddLocalListener<EntityDiedEvent>(HandleDeath);
        }

        protected override void OnDestroy()
        {
            DispatchRoot.RemoveLocalListener<EntityDiedEvent>(HandleDeath);
            base.OnDestroy();
        }

        void OnRelenquish()
        {
            if (TriggerOnRelenquish)
                Despawn();
        }

        void OnDisable()
        {
            if (TriggerOnDisable)
            {
                if (Spawner != null)
                    Spawner.Despawned(this);
                Spawner = null;
            }
        }

        public void SpawnedBy(ISpawner spawner)
        {
            Spawner = spawner;
            OnSpawn.Invoke(this, spawner);
        }

        void HandleDeath(EntityDiedEvent msg)
        {
            if (TriggerOnDeath)
            {
                OnDespawn.Invoke(this, Spawner);
                if (Spawner != null)
                    Spawner.Killed(this);
                Spawner = null;
            }
        }

        public void Despawn()
        {
            OnDespawn.Invoke(this, Spawner);
            if (Spawner != null)
                Spawner.Despawned(this);
            Spawner = null;
        }
    }
}
