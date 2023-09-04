using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using Sirenix.OdinInspector;
using Peg.Util;

namespace Peg.Game
{
    /// <summary>
    /// Simple interface for creating character stats that map
    /// strings to floating-point values.
    /// 
    /// TODO:
    ///     -add a listener callback that is triggered when any value is modified.
    ///      This way we can 'push' updated values to their users.
    /// </summary>
    public class Stats : SerializedMonoBehaviour, IStats
    {
        [PropertyTooltip("Identifies this set of stats and allows for easy searching through an entity's hierarchy")]
        public HashedString Name = new HashedString("Derived Stats");

        public HashedString StatsCollectionName { get { return Name; } }

        [PropertyTooltip("The set of stats provided by this component.")]
        public HashMap<string, float> Map;

        public event Action<string> OnStatChanged;

        [Sirenix.Serialization.OdinSerialize]
        HashMap<string, List<StatsMod>> Mods = new HashMap<string, List<StatsMod>>();




        protected void Awake()
        {
            Map.OnValueChanged += PropogateChange;
        }

        protected virtual void OnDestroy()
        {
            Map.OnValueChanged -= PropogateChange;
        }

        protected void OnEnable()
        {
            PropogateChange();
        }

        /// <summary>
        /// Used to notify listeners that a stat has changed value.
        /// </summary>
        /// <param name="key"></param>
        public void PropogateChange(string key)
        {
            if (OnStatChanged != null)
                OnStatChanged(key);
        }

        /// <summary>
        /// Used to notify listeners that all stats have change value.
        /// </summary>
        public void PropogateChange()
        {
            foreach (var kvp in Map)
                PropogateChange(kvp.Key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="mod"></param>
        public void Register(string key, StatsMod mod)
        {
            Assert.IsFalse(string.IsNullOrEmpty(key));
            Assert.IsNotNull(mod);

            List<StatsMod> queue = null;
            if(Mods.TryGetValue(key, out queue))
                queue.Add(mod);
            else
            {
                queue = new List<StatsMod>(1);
                queue.Add(mod);
                Mods.Add(key, queue);
            }

            PropogateChange(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="mod"></param>
        public void Unregister(string key, StatsMod mod)
        {
            Assert.IsFalse(string.IsNullOrEmpty(key));
            Assert.IsNotNull(mod);

            List<StatsMod> queue = null;
            if (Mods.TryGetValue(key, out queue))
                queue.Remove(mod);

            PropogateChange(key);
        }

        /// <summary>
        /// Returns a stat's value after applying all registered modifiers to it.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float QueryStat(string key)
        {
            float final = Map[key];

            List<StatsMod> queue = null;
            if(Mods.TryGetValue(key, out queue))
            {
                foreach (var q in queue)
                    final = q.QueryModifiedStat(key, final);
            }

            return final;
        }

        /// <summary>
        /// Sets the base value for a named stat.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetStatBase(string key, float value)
        {
            Map[key] = value;
        }

        /// <summary>
        /// Gets the first modifier of a stat with a given name-hash.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="nameHash"></param>
        /// <returns></returns>
        public StatsMod GetFirstMod(string key, int nameHash)
        {
            List<StatsMod> queue = null;
            if (Mods.TryGetValue(key, out queue))
            {
                foreach (var q in queue)
                {
                    if (q.ModId.Hash == nameHash)
                        return q;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns all mods of the given name-hash that are applied
        /// to the given stat.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="nameHash"></param>
        /// <returns></returns>
        public List<StatsMod> GetAllMods(string key, int nameHash)
        {
            List<StatsMod> queue = null;
            if (Mods.TryGetValue(key, out queue))
                return new List<StatsMod>(queue);
            
            return null;
        }
        
    }

    /*
    /// <summary>
    /// Used to convert between a set of base stats (and potentially other data sources)
    /// and a set of derived stats that are related to the base stats in some way defined
    /// by the Convert method.
    /// </summary>
    public abstract class AbstractStatsConverter : SerializedMonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class Converter
        {
            public enum CurveType
            {
                None = 0,
                Clamp = 16,
                ConvertedRange = 48,
                Percent = 256,
            }

            public BaseStatBindingMap.CurveType UseCurve;
            public string ControllingStat;
            [Tooltip("Takes the form of ComponentName:MemberName")]
            public string BindingPath;
            public float Min = 1;
            public float Max = 100;
            public AnimationCurve Curve = AnimationCurve.Linear(0, 0, 1, 1);


            public float CalculateEffect(float input, float statMin, float statMax, float minAdjust = 0, float maxAdjust = 0)
            {
                if (UseCurve == BaseStatBindingMap.CurveType.Clamp)
                    input = Mathf.Clamp(input, Min, Max);
                else if (UseCurve == BaseStatBindingMap.CurveType.ConvertedRange)
                    input = MathUtils.ConvertRange(input, statMin + minAdjust, statMax + maxAdjust, Min, Max);
                else if (UseCurve == BaseStatBindingMap.CurveType.Percent)
                {
                    input = MathUtils.Normalize(input, statMin + minAdjust, statMax + maxAdjust);
                    return MathUtils.EvaluateCurveRange(input, 0, 1, Curve);
                }
                return MathUtils.EvaluateCurveRange(input, Min, Max, Curve);
            }

            public void PushBind(GameObject target, float input, float statMin, float statMax, float minAdjust = 0, float maxAdjust = 0)
            {
                float output = CalculateEffect(input, statMin, statMax, maxAdjust, minAdjust);
                BindingHelper.PushBind(output, target.GetEntityRoot(), BindingPath, null);
            }
        }


        public float StatMin = 1;
        public float StatMax = 20;

        [InspectorMargin(15)]
        public Stats SourceStats;
        public Stats DerivedStats;
        
        [InspectorIndent]
        public Converter[] BaseConverters;
        [InspectorIndent]
        public Converter[] DerivedConverters;


        protected void Start()
        {
            Assert.IsNotNull(SourceStats);
            Assert.IsNotNull(DerivedStats);

            SourceStats.OnStatChanged += ConvertToDerived;
            DerivedStats.OnStatChanged += PushConverted;

            SourceStats.PropogateChange();
            DerivedStats.PropogateChange();
        }

        protected virtual void OnDestroy()
        {
            DerivedStats.OnStatChanged -= PushConverted;
            SourceStats.OnStatChanged -= ConvertToDerived;
        }

        /// <summary>
        /// For a given key, this method will return a conversion forumla
        /// to obtain the derived value of the stat named.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual void ConvertToDerived(string key)
        {
            //push data for bound base stats
            for (int i = 0; i < BaseConverters.Length; i++)
            {
                Converter con = BaseConverters[i];
                float value = SourceStats.QueryStat(con.ControllingStat);
                con.PushBind(gameObject, value, StatMin, StatMax);
            }
        }


        /// <summary>
        /// Applies any registered converters to data that are bound to derived stats.
        /// Invoked automatically when a derived stat changes.
        /// </summary>
        /// <param name="key"></param>
        public virtual void PushConverted(string key)
        {
            for (int i = 0; i < DerivedConverters.Length; i++)
            {
                Converter con = DerivedConverters[i];
                float value = DerivedStats.QueryStat(con.ControllingStat);
                con.PushBind(gameObject, value, StatMin, StatMax);
            }
        }
    }
    */


    /// <summary>
    /// Helper extension methods.
    /// </summary>
    public static partial class GameObjectExtension
    {
        /// <summary>
        /// Extension method for finding the first Stats component on an object with the given name.
        /// </summary>
        /// <returns></returns>
        public static IStats FindStats(this GameObject go, int nameHash)
        {
            var comps = go.FindComponentsInEntity<IStats>();
            if (comps != null)
            {
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i].StatsCollectionName.Hash == nameHash) return comps[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Extension method for finding the first Stats component on an object with the given name.
        /// </summary>
        /// <returns></returns>
        public static IStats FindStats(this EntityRoot entity, int nameHash, bool cache = false)
        {
            IStats stat = null;
            if (cache) stat = entity.LookupHashedIdComponent(nameHash) as IStats;

            if (stat != null) return stat;
            else
            {
                var comps = entity.FindComponentsInEntity<IStats>(cache);
                if (comps != null)
                {
                    for (int i = 0; i < comps.Length; i++)
                    {
                        if (comps[i].StatsCollectionName.Hash == nameHash)
                        {
                            if (cache && comps[i] != null) entity.AddHashedComponentToLookup(nameHash, comps[i]);
                            return comps[i];
                        }
                    }
                }
            }
            return null;
        }

    }
    

    /// <summary>
    /// Interface used by Stats components.
    /// </summary>
    public interface IStats
    {
        HashedString StatsCollectionName { get; }
        float QueryStat(string key);
        void SetStatBase(string key, float value);
        event Action<string> OnStatChanged;
    }
}
