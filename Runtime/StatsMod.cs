using UnityEngine;
using System;
using UnityEngine.Assertions;
using Sirenix.OdinInspector;

namespace Toolbox.Game
{
    /// <summary>
    /// This component registers itself with the named source Stats at creation
    /// and provides a modification to a single stat value on that Stats component.
    /// All StatMods are applied in the order in which they were registered.
    /// </summary>
    public class StatsMod : SerializedMonoBehaviour
    {
        public const int NoLocalEffect = 100;

        [SerializeField]
        StatModifyType _ModType;
        public StatModifyType ModType
        {
            get { return _ModType; }
            set
            {
                if (value != _ModType && SourceStats != null)
                {
                    _ModType = value;
                    SourceStats.PropogateChange(StatName);
                }
                else _ModType = value;
            }
        }
        [PropertyTooltip("An identifying name of this modification.")]
        public HashedString ModId = new HashedString("Stat Mod");
        [Tooltip("The source stats on this entity that should be modified. If it is not present, this component will self-destruct immediately. If this value changes at runtime, this component must be re-enabled for it to take effect.")]
        public HashedString SourceName = new HashedString();
        [Tooltip("The name of the stat this will modify.")]
        public string StatName;
        

        [SerializeField]
        float _LocalEffect;
        public float LocalEffect
        {
            get { return _LocalEffect; }
            set
            {
                if (value != _LocalEffect && SourceStats != null)
                {
                    _LocalEffect = value;
                    SourceStats.PropogateChange(StatName);
                }
                else _LocalEffect = value;
            }
        }

        EntityRoot Root;
        Stats SourceStats;
        bool Registered;

        /// <summary>
        /// Creates a StatsMod component and attaches it to the given GameObject. This GameObject must
        /// be part of an AEH and must have an EntityRoot declared.
        /// </summary>
        /// <param name="target">The GameObject to attach the StatsMod to.</param>
        /// <param name="type">The type of modification.</param>
        /// <param name="effect">The strength of the modification.</param>
        /// <param name="modName">The identifying name of the modifiaction.</param>
        /// <param name="statsId">The name of the Stats collection that is to be affected by this mod.</param>
        /// <param name="statToAffect">The individual stat within the Stats collection that will be affected by this mod.</param>
        /// <returns>The StatsMod componen that was added to the GameObject.</returns>
        public static StatsMod ApplyStatMod(GameObject target, StatModifyType type, float effect, string modName, string statsId, string statToAffect)
        {
            Assert.IsFalse(string.IsNullOrEmpty(modName));
            Assert.IsFalse(string.IsNullOrEmpty(statsId));
            Assert.IsFalse(string.IsNullOrEmpty(statToAffect));

            StatsMod mod = target.AddComponent<StatsMod>();
            mod._ModType = type;
            mod._LocalEffect = effect;
            mod.ModId.Value = modName;
            mod.SourceName.Value = statsId;
            mod.StatName = statToAffect;
            mod.TryApplyMod();
            return mod;
        }

        void Awake()
        {
            Root = gameObject.GetEntityRoot();
        }
        
        void OnEnable()
        {
            TryApplyMod();
        }

        void OnDisable()
        {
            RemoveMod();
        }

        void TryApplyMod()
        {
            //We need to check for this possibility since we may be
            //trying to apply the mod prematurly due to using the
            //static factory method.
            if (Registered || SourceName.NoValue) return;

            SourceStats = Root.FindStats(SourceName.Hash, true) as Stats;
            if (SourceStats == null) Destroy(this);
            else
            {
                SourceStats.Register(StatName, this);
                Registered = true;
            }
        }

        void RemoveMod()
        {
            if (SourceStats != null && Registered)
                SourceStats.Unregister(StatName, this);
        }

        public float QueryModifiedStat(string key, float source)
        {
            //float source = SourceStats.Map[key];
            switch (ModType)
            {
                case StatModifyType.Add: { return source + LocalEffect; }
                case StatModifyType.Subtract: { return source - LocalEffect; }
                case StatModifyType.Multiply: { return source * LocalEffect; }
                case StatModifyType.Divide: { return source / LocalEffect; }
                case StatModifyType.PercentInc: { return source + (source * LocalEffect); }
                case StatModifyType.PercentDec: { return source - (source * LocalEffect); }

                case StatModifyType.Pass: { return source; }
                case StatModifyType.Floor: { return Mathf.Floor(source); }
                case StatModifyType.Ceil: { return Mathf.Ceil(source); }
                case StatModifyType.Round: { return Mathf.Round(source); }
                case StatModifyType.Negative: { return -source; }
                case StatModifyType.Inverse: { return 1 / source; }
                case StatModifyType.Truth: { return (Convert.ToBoolean(source)) ? 1 : 0; }
                default:
                    {
                        Debug.LogWarning("Unsupported application '" + ModType.ToString() + "' for Stats component '" + ModId.Value + "'.");
                        return source;
                    }
            }
        }

    }


    [Serializable]
    public enum StatModifyType
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        PercentInc,
        PercentDec,

        //anything over 100 will be considered to have 'no local effect'
        Pass = StatsMod.NoLocalEffect,
        Floor,
        Ceil,
        Round,
        Negative,
        Inverse,
        Truth,
    }

}