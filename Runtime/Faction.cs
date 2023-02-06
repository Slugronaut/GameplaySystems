using UnityEngine;
using System;
using System.Collections.Generic;
using Toolbox.Game.Messages;
using Toolbox.Messaging;

namespace Toolbox.Game
{
    /// <summary>
    /// Represents a set of alignments and enemies.
    /// </summary>
    /// <remarks>
    /// Alignments and enemeies use bit-masks so relationships
    /// are entirely boolean and reciprocating. If two entities
    /// share any one faction they are considered to be aligned.
    /// </remarks>
    [AddComponentMenu("Toolbox/Game/Building Blocks/Faction")]
    [DisallowMultipleComponent]
    public sealed class Faction : LocalListenerMonoBehaviour
    {
        /// <summary>
        /// Edit these as needed per project.
        /// </summary>
        public enum Alignment
        {
            Player          = 1 << 0,
            Ally            = 1 << 1,
            Townsfolk       = 1 << 2,
            Law             = 1 << 3,
            Mob             = 1 << 4,
            Beast           = 1 << 5,
            Alignment7      = 1 << 6,
            Alignment8      = 1 << 7,
            //.. this can go all the way to 31 if we wished.
        }

        public enum FilterType
        {
            IncludeAllies,
            IncludeEnemies,
        }

        public enum RelationshipType
        {
            Any,
            Enemy,
            Ally,
        }

        [Tooltip("A bitmask representing all factions affiliated with this entity.")]
        [MaskedEnum]
        public Alignment Alignments;

        static DemandFactionComponent DemandFac = new DemandFactionComponent(null);

        

        public static implicit operator int(Faction faction)
        {
            return (int)faction.Alignments;
        }

        void Awake()
        {
            DispatchRoot.AddLocalListener<DemandFactionComponent>(OnDemandedMe);
        }

        protected override void OnDestroy()
        {
            DispatchRoot.RemoveLocalListener<DemandFactionComponent>(OnDemandedMe);
            base.OnDestroy();
        }

        void OnDemandedMe(DemandFactionComponent msg)
        {
            msg.Respond(this);
        }

        /// <summary>
        /// Returns true if the other faction has a given relationship type with this faction.
        /// The other faction can be null which will result in false being returned unless 
        /// the relationship type passed is Any - in which case true will be returned.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool HasRelationshipOf(Faction other, RelationshipType type)
        {
            if (type == RelationshipType.Any) return true;
            else if (other == null) return false;
            else if (type == RelationshipType.Ally && IsAlignedWith(other)) return true;
            else if (type == RelationshipType.Enemy && !IsAlignedWith(other)) return true;
            else return false;
        }

        /// <summary>
        /// Flips the required alignment mask bits necessary to make this alignment an enemy of the provided one.
        /// </summary>
        /// <param name="factionName">The name of the faction as expressed in the <c>Alginment</c> enum.</param>
        public void SetAsAligned(string factionName)
        {
            int mask = -1;
            mask = (int)Enum.Parse(typeof(Alignment), factionName);
            SetAsAligned(mask);
        }

        /// <summary>
        /// Flips the required alignment mask bits necessary to make this alignment an ally of the provided one.
        /// </summary>
        /// <param name="mask"></param>
        public void SetAsAligned(int mask)
        {
            Alignments = (Alignment)((int)Alignments | mask);
        }

        /// <summary>
        /// Flips the required alignment mask bits necessary to make this alignment an enemy of the provided one.
        /// </summary>
        /// <param name="factionName">The name of the faction as expressed in the <c>Alginment</c> enum.</param>
        public void SetAsEnemy(string factionName)
        {
            int mask = -1;
            mask = (int)Enum.Parse(typeof(Alignment), factionName);
            SetAsEnemy(mask);
        }

        /// <summary>
        /// Flips the required alignment mask bits necessary to make this alignment an enemy of the provided one.
        /// </summary>
        /// <param name="mask"></param>
        public void SetAsEnemy(int mask)
        {
            Alignments = (Alignment)((int)Alignments ^ mask);
        }

        /// <summary>
        /// Compares this faction's alignment with another.
        /// </summary>
        /// <param name="mask">The bitmask representing a Faction alignment.</param>
        /// <returns><c>true</c> if this alignment is considered allied with the supplied one, <c>false</c> otherwise.</returns>
        public bool IsAlignedWith(int mask)
        {
            return ((int)Alignments & mask) != 0;
        }

        /// <summary>
        /// Given a collection of GameObjects that are assumed to have Factions
        /// somehwere in their AeH, this will filter out all GameObjects of
        /// a particular alliance class.
        /// </summary>
        /// <param name="entsIn"></param>
        /// <returns>A new list with the entities that have a given relation to the source faction.</returns>
        public static List<GameObject> FilterByFaction(FilterType filterBy, Faction source, List<GameObject> entsIn)
        {
            int count = 0;
            if (count < 0) return null;
            var entsOut = new List<GameObject>(count);

            
            for (int i = 0; i < count; i++)
            {
                Faction fac = null;
                GlobalMessagePump.Instance.ForwardDispatch<DemandFactionComponent>(entsIn[i], new DemandFactionComponent((x) => { fac = x; }));

                if (fac != null)
                {
                    if (filterBy == FilterType.IncludeAllies && fac.IsAlignedWith(source))
                        entsOut.Add(entsIn[i].GetEntityRoot().gameObject);
                    else if (filterBy == FilterType.IncludeEnemies && !fac.IsAlignedWith(source))
                        entsOut.Add(entsIn[i].GetEntityRoot().gameObject);
                }
            }

            return entsOut;
        }

        /// <summary>
        /// Given a collection of GameObjects that are assumed to have Factions
        /// somehwere in their AeH, this will filter out all GameObjects of
        /// a particular alliance class. This version will modify the incoming list
        /// rather than allocate and return a new one.
        /// </summary>
        /// <param name="entsIn"></param>
        public void FilterbyFactionNonAlloc(FilterType filterBy, Faction source, List<GameObject> entsIn)
        {
            int count = 0;
            if (count < 0) return;
            for (int i = 0; i < count; i++)
            {
                DemandFac.Reset();
                GlobalMessagePump.Instance.ForwardDispatch(entsIn[i], DemandFac);
                Faction fac = DemandFac.Desired;

                if (fac != null)
                {
                    if (filterBy == FilterType.IncludeAllies && !fac.IsAlignedWith(source))
                    {
                        entsIn.RemoveAt(i);
                        i--;
                    }
                    else if (filterBy == FilterType.IncludeEnemies && fac.IsAlignedWith(source))
                    {
                        entsIn.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        
        /// <summary>
        /// Given a collection of Factions this will filter 
        /// out all of a particular alliance class.
        /// </summary>
        /// <param name="entsIn"></param>
        /// <returns>A new list with the entities that have a given relation to the source faction.</returns>
        public static List<Faction> FilterByFaction(FilterType filterBy, Faction source, List<Faction> entsIn)
        {
            int count = 0;
            if (count < 0) return null;
            var entsOut = new List<Faction>(count);

            Faction fac = null;
            for (int i = 0; i < count; i++)
            {
                fac = entsIn[i];
                
                if (fac != null)
                {
                    if (filterBy == FilterType.IncludeAllies && fac.IsAlignedWith(source))
                        entsOut.Add(fac);
                    else if (filterBy == FilterType.IncludeEnemies && !fac.IsAlignedWith(source))
                        entsOut.Add(fac);
                }
            }

            return entsOut;
        }

        /// <summary>
        /// Given a collection of Factions this will filter 
        /// out all of a particular alliance class. 
        /// This version will modify the incoming list
        /// rather than allocate and return a new one.
        /// </summary>
        /// <param name="entsIn"></param>
        public void FilterbyFactionNonAlloc(FilterType filterBy, Faction source, List<Faction> entsIn)
        {
            int count = 0;
            if (count < 0) return;


            Faction fac = null;
            for (int i = 0; i < count; i++)
            {
                fac = entsIn[i];

                if (fac != null)
                {
                    if (filterBy == FilterType.IncludeAllies && !fac.IsAlignedWith(source))
                    {
                        entsIn.RemoveAt(i);
                        i--;
                    }
                    else if (filterBy == FilterType.IncludeEnemies && fac.IsAlignedWith(source))
                    {
                        entsIn.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        
    }
}


namespace Toolbox.Game.Messages
{
    /// <summary>
    /// Message for demanding an entity's Faction component using the GlobalMessagePump.
    /// </summary>
    public class DemandFactionComponent : Demand<Faction>
    {
        public DemandFactionComponent(Action<Faction> callback) : base(callback) { }
    }
}
