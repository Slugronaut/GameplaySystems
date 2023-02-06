using UnityEngine;
using System.Collections.Generic;
using Toolbox.Game;
using UnityEngine.Events;
using Toolbox.Lazarus;
using Toolbox.AutoCreate;

namespace Toolbox.Game
{
    /// <summary>
    /// Utility for spawning multitudes of prefabs. It interally
    /// tracks all prefabs spawned and despawned/killed.
    /// 
    /// TODO: listen for pool reclaim event on each entity and react accordingly
    /// 
    /// TODO: add Spawn Region sampling system that allows setting up mob prefab
    ///       and database ids that can be used to spawn approrpiate mobs
    ///       based on geography near the chosen spawn point
    ///       
    /// TODO: Convert database ids to HashedString
    /// 
    /// </summary>
    public class EntitySpawnPoint : MonoBehaviour, ISpawner
    {
        public EntityRoot[] Prefabs;
        [Tooltip("Is one type of prefab randomly chosen per cycle or is each spawn random?")]
        public bool UniformRandom = false;
        [Tooltip("Should this spawner also post the pre-spawn event locally to itself?")]
        public bool PostPreSpawnMsg = true;


        [Tooltip("When the prefabs spawned from this object are reduced below this count it will begin spawning again.")]
        public int MinCount = 0;
        [Tooltip("When this many prefabs are spawned by this object it will stop spawning.")]
        public int MaxCount = 10;
        [Tooltip("Max time in milliseconds per frame that is allowed to be spent spawning entities. If this time is faster than what is set for the Lazurus allowed time, it may start live-instantion of objects instead of using pooled ones.")]
        public float MaxTime = 5;
        [Tooltip("The maximum rate at which entities will spawn, averaged out over a second.")]
        public float SpawnsPerSec = 100;
        [Tooltip("Delay time before spawning for first time since activated.")]
        public float StartDelay = 0;
        public float PositionJitter = 0;
        [Tooltip("If set, will automatically enable self and begin spawning when min count is met.")]
        public bool AutoEnable = true;
        [Tooltip("If set, will automatically disable self and stop spawning when max count is met.")]
        public bool AutoDisable = true;

        

#if UNITY_EDITOR
        public Color GizmoColor = Color.white;
#endif

        [System.Serializable]
        public class SpawnEvent : UnityEvent<EntityRoot> { }
        public SpawnEvent OnPreActivateSpawn = new SpawnEvent();
        public SpawnEvent OnIncreasedActive = new SpawnEvent();
        public SpawnEvent OnReuceByDespawn = new SpawnEvent();
        public SpawnEvent OnReuceByDeath = new SpawnEvent();

        float LastTime;
        float Accum;
        EntityRoot FirstSpawn;
        HashSet<EntityRoot> SpawnedList;
        IPoolSystem Lazarus;


        public int ActiveCount
        {
            get { return SpawnedList == null ? 0 : SpawnedList.Count; }
        }


        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = GizmoColor;
            Gizmos.DrawWireSphere(transform.position, PositionJitter);
        }
        #endif

        void Awake()
        {
            SpawnedList = new HashSet<EntityRoot>();
            Lazarus = AutoCreator.AsSingleton<IPoolSystem>();
        }

        void OnDestroy()
        {
            OnIncreasedActive.RemoveAllListeners();
            OnReuceByDespawn.RemoveAllListeners();
        }

        void OnEnable()
        {
            StartSpawning();
            LastTime = Time.time;
        }

        void Update()
        {
            float t = Time.time;
            if (t - LastTime < StartDelay) return;

            //Since we may be spawning 'fractions of entities' at a time
            //we need to track the accumulated total over several frames.
            //When it reaches a number at or above 1, we know we can spawn.
            Accum += SpawnsPerSec * Time.deltaTime;
            int count = Mathf.FloorToInt(Accum);
            if (count > 0)
            {
                Accum -= count;
                if (Accum < 0) Accum = 0;
                float startTime = Time.realtimeSinceStartup;

                while (count > 0)
                {
                    //make sure we don't spend too much time spawning this frame
                    if (Time.realtimeSinceStartup - startTime > (MaxTime / 1000))
                        return;

                    SpawnSingle();
                    if (SpawnedList.Count >= MaxCount)
                    {
                        if(AutoDisable)
                            enabled = false;
                        return;
                    }
                    count--;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void StartSpawning()
        {
            Accum = 0;
            if (SpawnsPerSec < 0.0001f) SpawnsPerSec = 0.0001f;
            FirstSpawn = Prefabs[Random.Range(0, Prefabs.Length)];
        }

        /// <summary>
        /// Helper for spawning a single entity randomly.
        /// </summary>
        void SpawnSingle()
        {
            var jitter = Random.insideUnitSphere * PositionJitter;
            jitter = new Vector3(jitter.x, 0.0f, jitter.z);
            var source = UniformRandom ? FirstSpawn : Prefabs[Random.Range(0, Prefabs.Length)];

            var go = Lazarus.Summon(source.gameObject, transform.position + jitter, false);
            var root = go.GetEntityRoot();
            OnPreActivateSpawn.Invoke(root);
            if(PostPreSpawnMsg)
                GlobalMessagePump.Instance.ForwardDispatch(gameObject, PreactiveSpawnEvent.Shared.ChangeValues(root));
            go.SetActive(true);
            SpawnedList.Add(root);
            OnIncreasedActive.Invoke(root);

            var spawnerSource = root.FindComponentInEntity<SpawnedEntity>();
            if (spawnerSource != null)
                spawnerSource.SpawnedBy(this);
        }

        /// <summary>
        /// Resets this spawner's internal counter. This will not affect any currently active
        /// entities spawned by this object. It will loose track of all previously spawn entities.
        /// </summary>
        public void ResetCount()
        {
            SpawnedList.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spawn"></param>
        public void RegisterSpawn(SpawnedEntity spawn)
        {

        }

        /// <summary>
        /// Lets this spawner know the following entity died. If that entity 
        /// belonged to this spawner it will internally reduce its count.
        /// </summary>
        /// <param name="ent"></param>
        public void Killed(SpawnedEntity spawn)
        {
            if (spawn == null) return;
            var ent = spawn.Root;
            DemandEntityRoot.Shared.Reset();

            if (SpawnedList.Contains(ent))
            {
                SpawnedList.Remove(ent);
                if (SpawnedList.Count <= MinCount && !enabled && AutoEnable) enabled = true;
                OnReuceByDeath.Invoke(ent);
            }
        }

        /// <summary>
        /// Injects an entity into this despawner's list of active spawns. It becomes are
        /// part of the entity count used for tracking waves.
        /// </summary>
        /// <param name="ent"></param>
        public void InjectRegisteredSpawn(EntityRoot ent)
        {
            throw new UnityException("Not yet implemented.");
        }

        /// <summary>
        /// Lets this spawner know the following entity was despawned. If that entity 
        /// belonged to this spawner it will internally reduce its count.
        /// </summary>
        /// <param name="ent"></param>
        public void Despawned(SpawnedEntity spawn)
        {
            if (spawn == null) return;
            var ent = spawn.Root;
            if (SpawnedList.Contains(ent))
            {
                SpawnedList.Remove(ent);
                if (SpawnedList.Count <= MinCount && !enabled && AutoEnable) enabled = true;
                OnReuceByDespawn.Invoke(ent);
            }
        }

        /*
        /// <summary>
        /// Handles all death events in scene. If the messsage contains an entity
        /// spawned by this object, it reduces its internal counter.
        /// </summary>
        /// <param name="msg"></param>
        void ReduceCountByDeath(EntityDiedEvent msg)
        {
            if (msg.Target == null) return;
            DemandEntityRoot.Shared.Reset();
            GlobalMessagePump.ForwardDispatch(msg.Target.gameObject, DemandEntityRoot.Shared);
            EntityRoot root = DemandEntityRoot.Shared.Desired;

            if (Spawned.Contains(root))
            {
                Spawned.Remove(root);
                if (Spawned.Count <= MinCount && !enabled && AutoEnable) enabled = true;
                OnReuceByDeath.Invoke(root);
            }

        }

        /// <summary>
        /// Handles all despawn events in scene. If the message contains an entity
        /// spawned by this object, it reduces its internal counter.
        /// </summary>
        /// <param name="msg"></param>
        void ReduceCountByDespawn(EntityDespawnedEvent msg)
        {
            if (Spawned.Contains(msg.Target))
            {
                Spawned.Remove(msg.Target);
                if (Spawned.Count <= MinCount && !enabled && AutoEnable) enabled = true;
                OnReuceByDespawn.Invoke(msg.Target);
            }

        }
        */

        
        /// <summary>
        /// Relenquishes all enities that aren't on camera.
        /// Primarily used by EntitySpawnThrottle.
        /// </summary>
        public void RelenquishNonVisible(Camera camera = null)
        {
            throw new System.Exception("Not implemented.");
            /*
            if (SpawnedList == null || SpawnedList.Count < 1) return;

            Camera cam = camera ?? Camera.main;
            var planes = GeometryUtility.CalculateFrustumPlanes(cam);

            //Find out which active entitities aren't on camera and relenquish them.
            //TODO: if we start tracking relenquished entities at some
            //point, then this loop will need to change to a while(Spawned.Count > 0) loop instead
            int count = SpawnedList.Count;
            List<EntityRoot> keepers = new List<EntityRoot>(count);
            for (int i = 0; i < count; i++)
            {
                Bounds bounds = new Bounds(SpawnedList[i].transform.position, Vector3.one * 3);
                if (!GeometryUtility.TestPlanesAABB(planes, bounds))
                    Lazarus.RelenquishToPool(SpawnedList[i].gameObject);
                else keepers.Add(SpawnedList[i]);
            }
            if (keepers.Count > 0) SpawnedList = keepers;
            else SpawnedList.Clear();

            if (SpawnedList.Count <= MinCount && !enabled && AutoEnable) enabled = true;
            */
        }

    }


    /// <summary>
    /// Interface used by all spawners
    /// </summary>
    public interface ISpawner
    {
        //GameObject Spawn(Vector3 position, Transform parent);
        void RegisterSpawn(SpawnedEntity ent);
        void Despawned(SpawnedEntity ent);
        void Killed(SpawnedEntity ent);
        void InjectRegisteredSpawn(EntityRoot entity);
    }

    public interface ISubSpawner : ISpawner
    { }


}


namespace Toolbox
{
    public class EntitySpawnedEvent : AgentTargetMessage<ISpawner, EntityRoot, EntitySpawnedEvent>
    {
        public static EntitySpawnedEvent Shared = new(null, null);
        public EntitySpawnedEvent() : base() { }
        public EntitySpawnedEvent(ISpawner agent, EntityRoot target) : base(agent, target) { }
    }

    public class EntityDespawnedEvent : AgentTargetMessage<ISpawner, EntityRoot, EntityDespawnedEvent>, IDeferredMessage
    {
        public static EntityDespawnedEvent Shared = new(null, null);
        public EntityDespawnedEvent() : base() { }
        public EntityDespawnedEvent(ISpawner agent, EntityRoot target) : base(agent, target) { }
    }

    /// <summary>
    /// Posted locally to an Entity just after it has spawned but before activation (if using pool).
    /// </summary>
    public class PreactiveSpawnEvent : IMessageEvent
    {
        public EntityRoot Root { get; private set; }
        public static PreactiveSpawnEvent Shared = new PreactiveSpawnEvent();

        public PreactiveSpawnEvent ChangeValues(EntityRoot root)
        {
            Root = root;
            return this;
        }
    }
}



