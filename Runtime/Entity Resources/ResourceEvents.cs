using Peg.MessageDispatcher;
using UnityEngine;

namespace Peg.Game.ConsumableResource
{

    /// <summary>
    /// Posted by a CharacterStat Component when its internal current value changes.
    /// </summary>
    public class EntityResourceChangedEvent : IMessageEvent
    {
        public GameObject Agent { get; protected set; }
        public IEntityResource CharacterStat { get; protected set; }
        public float Difference { get; protected set; }

        public EntityResourceChangedEvent(GameObject agent, EntityResource charStat, float diff)
        {
            Agent = agent;
            CharacterStat = charStat;
            Difference = diff;
        }

        public void ChangeValues(GameObject agent, IEntityResource charStat, float diff)
        {
            Agent = agent;
            CharacterStat = charStat;
            Difference = diff;
        }
    }


    /// <summary>
    /// Posted by a CharacterStat Component when its internal current value goes up.
    /// </summary>
    public class EntityResourceGainedEvent : IMessageEvent
    {
        public GameObject Agent { get; protected set; }
        public IEntityResource CharacterStat { get; protected set; }

        /// <summary>
        /// Note that this value will always be positive.
        /// </summary>
        public float Difference { get; protected set; }

        public EntityResourceGainedEvent(GameObject agent, EntityResource charStat, float diff)
        {
            Agent = agent;
            CharacterStat = charStat;
            Difference = diff;
        }

        public EntityResourceGainedEvent ChangeValues(GameObject agent, IEntityResource charStat, float diff)
        {
            Agent = agent;
            CharacterStat = charStat;
            Difference = diff;
            return this;
        }
    }


    /// <summary>
    /// Posted by a CharacterStat Component when its internal current value goes down.
    /// </summary>
    public class EntityResourceLostEvent : IMessageEvent
    {
        public GameObject Agent { get; protected set; }
        public IEntityResource CharacterStat { get; protected set; }

        /// <summary>
        /// Note that this value will always be negative.
        /// </summary>
        public float Difference { get; protected set; }

        public EntityResourceLostEvent(GameObject agent, EntityResource charStat, float diff)
        {
            Agent = agent;
            CharacterStat = charStat;
            Difference = diff;
        }

        public EntityResourceLostEvent ChangeValues(GameObject agent, IEntityResource charStat, float diff)
        {
            Agent = agent;
            CharacterStat = charStat;
            Difference = diff;
            return this;
        }
    }


    /// <summary>
    /// Message that can be dispatched directly to an entity to let them
    /// know they should change their current stat by a certain value.
    /// </summary>
    public class ChangeEntityResourceCmd : IMessageCommand
    {
        public GameObject Agent { get; protected set; }
        public float Change { get; protected set; }

        public ChangeEntityResourceCmd(GameObject agent, float change)
        {
            Agent = agent;
            Change = change;
        }

        public ChangeEntityResourceCmd ChangeValues(GameObject agent, float change)
        {
            Agent = agent;
            Change = change;
            return this;
        }
    }
}
