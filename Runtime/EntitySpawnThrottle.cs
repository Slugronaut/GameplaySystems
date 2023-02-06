using UnityEngine;
using Toolbox.Behaviours;
using Sirenix.OdinInspector;

namespace Toolbox.Game
{
    /// <summary>
    /// A kind of AI that can be activated and it will harrass
    /// the player with a seemingly large number of mobs.
    /// 
    /// TODO: EntitySpawnPoint.UniformRandom doesn't work with this.
    /// 
    /// </summary>
    [RequireComponent(typeof(EntitySpawnPoint))]
    [DisallowMultipleComponent]
    public class EntitySpawnThrottle : MonoBehaviour
    {
        [Tooltip("Should entities spawn for the countdown of a timer or should they spawn based on total spawn count?")]
        public CountMode SpawnMode;
        [Tooltip("How does this system decided to position the spawned entities?")]
        public TrackingMode TrackMode;
        [Tooltip("If no target is specified, this will use the nearest Center-Of-Universe.")]
        public bool TargetNearestCou = true;
        [Tooltip("Transform of the entity that this system will be harrassing. The scene will be checked for the closest off-screen spawn node. If null, spawning will occur based on EntitySpawnPoint settings.")]
        [HideIf("TargetNearestCou")]
        [Indent(1)]
        public Transform Target;
        [Tooltip("Used to scale the spawn/timer count by a difficulty factor.")]
        [Range(1, 20)]
        public float DifficultyScale = 1;
        [Tooltip("If in MobCount mode it tells the system how many mobs to spawn total. If in Timer mode it tells the system how long in seconds to spawn mobs for.")]
        public int TotalCount = 50;
        [Tooltip("Tells the system how many mobs should be active at any given time during the lifetime of this system.")]
        public int MinActive = 10;
        public int MaxActive = 10;
        [Tooltip("If set, when this system is enabled and begins spawning it will first relenquish all previously spawned off-screen entities. This allows for a large pool to use in the next round of spawning.")]
        public bool RelenquishActiveOnEnable = true;
        [Tooltip("If set, will post a local message for each spawned entity that informs what the attack target should be.")]
        public bool SetTargetForAI = true;
        

        float StartTime;
        Vector3 StaticOffscreenPos;
        
        public enum TrackingMode
        {
            Closest,
            SecondClosest,
            Furthest,
            SecondFurthest,
            RandomOffscreen,
            //JustOffCamera,
            Static,
            OffScreenStatic
        }

        public enum CountMode
        {
            MobCount,
            Timer,
            SpawnPoint,
        }
        
        static SupplyAITargetCmd Cmd = new(null);

        EntitySpawnPoint SpawnPoint;
        int VirtualCount;

        void Awake()
        {
            SpawnPoint = GetComponent<EntitySpawnPoint>();
            SpawnPoint.OnIncreasedActive.AddListener(HandleCount);
            SpawnPoint.OnReuceByDespawn.AddListener(HandleReduceCount);
            StaticOffscreenPos = transform.position;
        }

        void OnDestroy()
        {
            SpawnPoint.OnIncreasedActive.RemoveListener(HandleCount);
            SpawnPoint.OnReuceByDespawn.RemoveListener(HandleReduceCount);
        }

        void OnEnable()
        {
            //this resets our static off-screen spawn mode - order is important here!
            transform.position = StaticOffscreenPos;
            StaticOffscreenPos = transform.position;

            if (SpawnMode == CountMode.SpawnPoint) return;

            int currCount = Mathf.CeilToInt(Random.Range(MinActive, MaxActive) * DifficultyScale);


            SpawnPoint.AutoEnable = true;
            SpawnPoint.MaxCount = currCount;
            SpawnPoint.MinCount = currCount - 1;
            VirtualCount = 0;
            StartTime = Time.time;
            SpawnPoint.enabled = true;
            if (RelenquishActiveOnEnable) SpawnPoint.RelenquishNonVisible();
        }

        void OnDisable()
        {
            if (SpawnMode == CountMode.SpawnPoint) return;
            SpawnPoint.MinCount = -1;
            SpawnPoint.enabled = false;
            SpawnPoint.StopAllCoroutines();
        }

        void Update()
        {
            if (SpawnMode != CountMode.SpawnPoint && TotalCount >= 0)
            {
                if (SpawnMode == CountMode.MobCount && VirtualCount >= TotalCount) enabled = false;
                else if (SpawnMode == CountMode.Timer && Time.time - StartTime >= TotalCount) enabled = false;
            }

            bool targetWasNull = false;
            if(Target == null && TargetNearestCou)
            {
                targetWasNull = true;
                var cou = CenterOfUniverse.GetClosest(transform.position);
                if (cou != null) Target = cou.transform;
            }

            //teleport to spawn point neartes our target
            if (TrackMode == TrackingMode.OffScreenStatic)
                transform.position = SpawnPointPlacement.GetClosestToPoint(StaticOffscreenPos);
            else if (TrackMode != TrackingMode.Static && Target != null)
            {
                if(TrackMode == TrackingMode.Closest) transform.position = SpawnPointPlacement.GetClosestToPoint(Target.position);
                else if(TrackMode == TrackingMode.SecondClosest) transform.position = SpawnPointPlacement.GetSecondClosestToPoint(Target.position);
                else if (TrackMode == TrackingMode.Furthest) transform.position = SpawnPointPlacement.GetFurthestFromPoint(Target.position);
                else if (TrackMode == TrackingMode.SecondFurthest) transform.position = SpawnPointPlacement.GetSecondFurthestFromPoint(Target.position);
                else if (TrackMode == TrackingMode.RandomOffscreen) transform.position = SpawnPointPlacement.GetRandomOffCamera();
            }

            if (targetWasNull)
                Target = null;
        }

        void HandleCount(EntityRoot ent)
        {
            if (SpawnMode != CountMode.MobCount) return;

            VirtualCount++;
            if(SetTargetForAI && Target != null)
                GlobalMessagePump.Instance.ForwardDispatch(ent.gameObject, Cmd.Change(Target.gameObject) as SupplyAITargetCmd);
        }

        public void HandleReduceCount(EntityRoot ent)
        {
            if (SpawnMode != CountMode.MobCount) return;
            VirtualCount--;
        }
    }


    /// <summary>
    /// Send to an entity to inform their AI blackboard what target they should attack/chase.
    /// </summary>
    public class SupplyAITargetCmd : TargetMessage<GameObject, SupplyAITargetCmd>
    {
        public SupplyAITargetCmd(GameObject target) : base(target) { }
    }
}
