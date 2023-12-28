using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Peg.Collections;
using Peg.UpdateSystem;
using Peg.AutoCreate;
using Peg.Game.ConsumableResource;
using Peg.MessageDispatcher;
using Peg.AutonomousEntities;

namespace Peg.Game.AI
{
    /// <summary>
    /// System for handling target finding for EnemeyTargetFinders.
    /// </summary>
    [AutoCreate]
    public class EnemyTargetFinderSystem : Updatable
    {
        public static EnemyTargetFinderSystem Instance { get; private set; }
        static readonly List<GameObject> TempList = new(10);
        static readonly HashSet<GameObject> TempSet = new();

        public enum DistanceMarginType
        {
            Fixed,
            Percent,
        }

        [Tooltip("How many seconds between each check?")]
        public float NoTargetCooldown = 0.25f;
        public float RetargetCooldown = 3;
        double LastNoTargetTime;
        double LastReTargetTime;

        public float MinRadius = 0;
        public float MaxRadius = 2;
        public int MaxEnts = 50;


        [Tooltip("If the current target goes beyond this distance, it will be set to null.")]
        public float MaxDistance;


        [Tooltip("If a new potential target is found while we already have one, the new one must be closer by this percentage before it will be selected.")]
        [Range(0, 1)]
        public float RetargetThreshold;

        [Tooltip("The manner in which the RetargetThreshold should be calculated.")]
        public DistanceMarginType DistanceType;

        List<EnemyTargetFinder> Comps = new();


        void AutoAwake()
        {
            Instance = this;
        }

        public override void Update()
        {
            double t = Time.timeAsDouble;
            //if (Time.timeAsDouble - LastTime < Cooldown) return;
            //LastTime = Time.timeAsDouble;
            bool noTargetReady = t - LastNoTargetTime > NoTargetCooldown;
            if (noTargetReady) LastNoTargetTime = t;

            bool reTargetReady = t - LastReTargetTime > RetargetCooldown;
            if (reTargetReady) LastReTargetTime = t;

            foreach (var comp in Comps)
            {
                var agentPos = comp.transform.position;

                //no target currently
                if (!comp.HasTarget)
                {
                    if (!noTargetReady) continue;
                    //get all possible targets within range and store the the closest
                    comp.CurrentTarget = GetClosestFromList(agentPos, EnemyTargetFinderSystem.GetGameobjectsWithinRange(MaxEnts, comp.TargetLayer, agentPos, MaxRadius, MinRadius));
                    return;
                }
                //has a target, do we need to change it?
                else
                {
                    var targetPos = comp.CurrentTarget.transform.position;

                    //current target too far away, loose it
                    if(MaxDistance > 0 && Vector3.Distance(agentPos, targetPos) > MaxDistance)
                    {
                        comp.CurrentTarget = null;
                        return;
                    }

                    //is target alive?
                    if(!IsAlive(comp.CurrentTarget))
                    {
                        comp.CurrentTarget = null;
                        return;
                    }

                    //check for new potential targets
                    if (!reTargetReady) continue;
                    comp.CurrentTarget = SwapIfCloser(
                            EnemyTargetFinderSystem.GetGameobjectsWithinRange(MaxEnts, comp.TargetLayer, agentPos, MaxRadius, MinRadius),
                            comp.CurrentTarget, agentPos, RetargetThreshold, DistanceType);
                }
            }
        }

        public void Register(EnemyTargetFinder comp)
        {
            Comps.Add(comp);
        }

        public void UnRegister(EnemyTargetFinder comp)
        {
            Comps.Remove(comp);
        }

        /// <summary>
        /// Helper for getting closest GO from a list to a worldspace point. 
        /// </summary>
        /// <param name="agentPos"></param>
        /// <param name="gos"></param>
        /// <returns></returns>
        static GameObject GetClosestFromList(Vector3 agentPos, List<GameObject> gos)
        {
            if (gos == null || gos.Count < 1) return null;

            var closerDistance = Mathf.Infinity;
            GameObject closerGO = null;
            foreach (var go in gos)
            {
                var dist = (go.transform.position - agentPos).sqrMagnitude;
                if (dist < closerDistance)
                {
                    closerDistance = dist;
                    closerGO = go;
                }
            }

            return closerGO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        static bool IsAlive(GameObject target)
        {
            Health hp = null;

            //this will be faster if we are querying some deeply nested GameObject of the entity
            DemandHealthComponent.Shared.Reset();
            GlobalMessagePump.Instance.ForwardDispatch(target, DemandHealthComponent.Shared);
            hp = DemandHealthComponent.Shared.Desired;

            return (hp == null) ? false : !hp.IsDead;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="target"></param>
        /// <param name="agentPos"></param>
        /// <param name="margin"></param>
        /// <param name="distanceType"></param>
        /// <returns></returns>
        static GameObject SwapIfCloser(List<GameObject> list, GameObject target, Vector3 agentPos, float margin, DistanceMarginType distanceType)
        {
            Assert.IsNotNull(target);

            float closest;
            Vector3 targetPos = target.transform.position;
            closest = (distanceType == DistanceMarginType.Fixed) ? (agentPos - targetPos).sqrMagnitude : Vector3.Distance(agentPos, targetPos);
            

            var len = list.Count;
            GameObject result = null;
            for (int i = 0; i < len; i++)
            {
                if (distanceType == DistanceMarginType.Fixed)
                {
                    float dist = (list[i].transform.position - agentPos).sqrMagnitude;
                    if (dist + margin < closest)
                    {
                        closest = dist;
                        result = list[i];
                    }
                }
                else
                {
                    float dist = Vector3.Distance(list[i].transform.position, agentPos);
                    if (dist / closest < margin)
                    {
                        closest = dist;
                        result = list[i];
                    }
                }
            }
            if (result == null) return target;
            else return result;
        }


        /// <summary>
        /// Utility for getting a list of Root GameObjects from a list of Colliders.
        /// </summary>
        /// <param name="cols"></param>
        /// <param name="excludeSelf"></param>
        /// <returns></returns>
        public static List<GameObject> ExtractRootGos(Collider[] cols, GameObject selfGO, int len)
        {
            TempSet.Clear();
            TempList.Clear();
            GameObject self = selfGO;

            for (int i = 0; i < len; i++)
            {
                DemandEntityRoot.Shared.Reset();
                GlobalMessagePump.Instance.ForwardDispatch(cols[i].gameObject, DemandEntityRoot.Shared);

                var go = DemandEntityRoot.Shared.Desired.gameObject;
                if (go != self)
                    TempSet.Add(go);
            }

            foreach (var go in TempSet)
                TempList.Add(go);

            return TempList;
        }

        /// <summary>
        /// Utility for getting a list of Root GameObjects from a list of Colliders.
        /// </summary>
        /// <param name="cols"></param>
        /// <param name="excludeSelf"></param>
        /// <returns></returns>
        public static List<GameObject> ExtractRootGos(Collider[] cols, int len)
        {
            TempSet.Clear();
            TempList.Clear();

            for (int i = 0; i < len; i++)
            {
                DemandEntityRoot.Shared.Reset();
                GlobalMessagePump.Instance.ForwardDispatch(cols[i].gameObject, DemandEntityRoot.Shared);

                var go = DemandEntityRoot.Shared.Desired.gameObject;
                TempSet.Add(go);
            }

            foreach (var go in TempSet)
                TempList.Add(go);

            return TempList;
        }

        /// <summary>
        /// Utility for getting a list of GameObjects from a list of Colliders.
        /// </summary>
        /// <param name="cols"></param>
        /// <param name="excludeSelf"></param>
        /// <returns></returns>
        public static List<GameObject> ExtractColliderGos(Collider[] cols, GameObject selfGO, int len)
        {
            TempSet.Clear();
            TempList.Clear();
            GameObject self = selfGO;

            for (int i = 0; i < len; i++)
            {
                var go = cols[i].gameObject;
                if (go != self)
                    TempSet.Add(go);
            }

            foreach (var go in TempSet)
                TempList.Add(go);

            return TempList;
        }

        /// <summary>
        /// Utility for getting a list of GameObjects from a list of Colliders.
        /// </summary>
        /// <param name="cols"></param>
        /// <param name="excludeSelf"></param>
        /// <returns></returns>
        public static List<GameObject> ExtractColliderGos(Collider[] cols, int len)
        {
            TempSet.Clear();
            TempList.Clear();

            for (int i = 0; i < len; i++)
                TempSet.Add(cols[i].gameObject);

            foreach (var go in TempSet)
                TempList.Add(go);

            return TempList;
        }

        /// <summary>
        /// Returns a list of root gameobjects derived from a set of colliders that were detected
        /// within a minimum and maximum range of a world-space position. The list returned is internally
        /// cached and should be considered volitile data. It should be used immediately before calling this
        /// method again and should not be modified or cached elsewhere.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="maxColliders"></param>
        /// <param name="layers"></param>
        /// <param name="pos"></param>
        /// <param name="maxRadius"></param>
        /// <param name="minRadius"></param>
        /// <param name="excludeSelf"></param>
        /// <returns></returns>
        public static List<GameObject> GetGameobjectsWithinRange(GameObject self, int maxColliders, LayerMask layers, Vector3 pos, float maxRadius, float minRadius)
        {
            List<GameObject> colliderGOs = null;

            var cols = SharedArrayFactory.RequestTempArray<Collider>(maxColliders);
            int len = Physics.OverlapSphereNonAlloc(pos, maxRadius, cols, layers);
            colliderGOs = ExtractRootGos(cols, self, len);
#if UNITY_EDITOR
            if (minRadius > 0)
                Debug.LogError("MinRadius not currently supported by EnemyTargetFinderSystem.");
#endif
            //TODO: we also need a min-radius check here!

            return colliderGOs;
        }

        /// <summary>
        /// Returns a list of root gameobjects derived from a set of colliders that were detected
        /// within a minimum and maximum range of a world-space position. The list returned is internally
        /// cached and should be considered volitile data. It should be used immediately before calling this
        /// method again and should not be modified or cached elsewhere.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="maxColliders"></param>
        /// <param name="layers"></param>
        /// <param name="pos"></param>
        /// <param name="maxRadius"></param>
        /// <param name="minRadius"></param>
        /// <param name="excludeSelf"></param>
        /// <returns></returns>
        public static List<GameObject> GetGameobjectsWithinRange(int maxColliders, LayerMask layers, Vector3 pos, float maxRadius, float minRadius)
        {
            List<GameObject> colliderGOs = null;

            var cols = SharedArrayFactory.RequestTempArray<Collider>(maxColliders);
            int len = Physics.OverlapSphereNonAlloc(pos, maxRadius, cols, layers);
            colliderGOs = ExtractRootGos(cols, len);
#if UNITY_EDITOR
            if (minRadius > 0)
                Debug.LogError("MinRadius not currently supported by EnemyTargetFinderSystem.");
#endif
            //TODO: we also need a min-radius check here!

            return colliderGOs;
        }

    }

    

}
