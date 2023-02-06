using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Toolbox.Game
{
    /// <summary>
    /// Owns and controls a group of sub-mobs. Self-destructs when all sub-mobs have been despawned.
    /// </summary>
    [RequireComponent(typeof(SpawnedEntity))]
    public abstract class AbstractSubSpawner : MonoBehaviour, ISpawner, ISubSpawner
    {
        HashSet<SpawnedEntity> SubSpawns = new HashSet<SpawnedEntity>();



        protected virtual void OnDisable()
        {
            //if we get disabled, restore brain functionality to our sub-mobs.
            foreach (var sub in SubSpawns)
            {
                //need to check for null just in case we are calling this
                //during shutdown or a scene-wide purge and we need to
                //know if the objects have been killed yet.
                if (TypeHelper.IsReferenceNull(sub)) continue;
                Cleanup(sub);
            }

            SubSpawns.Clear();
        }

        /// <summary>
        /// Let's this hive mind know when a sub-spawned mob is no longer alive/enabled.
        /// </summary>
        /// <param name="ent"></param>
        public void SubSpawnRemove(SpawnedEntity ent)
        {
            //checking for shutdown conditions
            if (TypeHelper.IsReferenceNull(this))
                return;

            Assert.IsNotNull(ent);
            SubSpawns.Remove(ent);
            Cleanup(ent);
            if (SubSpawns.Count < 1)
            {
                //no sub-spawns left, disable ourself as a means to trigger relenquishment and despawning
                gameObject.GetEntityRoot().gameObject.SetActive(false);
            }
        }

        public void Despawned(SpawnedEntity ent)
        {
            SubSpawnRemove(ent);
        }

        public void Killed(SpawnedEntity ent)
        {
            SubSpawnRemove(ent);
        }

        public void InjectRegisteredSpawn(EntityRoot entity)
        {
            RegisterSpawn(entity.FindComponentInEntity<SpawnedEntity>(true));
        }

        /// <summary>
        /// Use this to add a new mob to this sub-spawner.
        /// </summary>
        /// <param name="ent"></param>
        public virtual void RegisterSpawn(SpawnedEntity ent)
        {
            Assert.IsNotNull(ent);
            SubSpawns.Add(ent);
            ent.SpawnedBy(this);
            Init(ent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sub"></param>
        protected virtual void Init(SpawnedEntity sub) { }

        /// <summary>
        /// Called when this sub-spawner deactivates and needs to clean up each of it's spawns.
        /// </summary>
        /// <param name="ent"></param>
        protected virtual void Cleanup(SpawnedEntity sub) { }
    }
}