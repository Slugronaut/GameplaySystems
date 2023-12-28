using Peg.AutonomousEntities;
using Peg.MessageDispatcher;
using System;
using UnityEngine;

namespace Peg.Game.ConsumableResource
{
    /// <summary>
    /// Posted by Health Component when its internal current health value changes.
    /// </summary>
    public class HealthChangedEvent : IMessageEvent
    {
        public GameObject Agent { get; protected set; }
        public Health Health { get; protected set; }
        public int Difference { get; protected set; }

        public HealthChangedEvent(GameObject agent, Health health, int diff)
        {
            Agent = agent;
            Health = health;
            Difference = diff;
        }

        public HealthChangedEvent ChangeValues(GameObject agent, Health health, int diff)
        {
            Agent = agent;
            Health = health;
            Difference = diff;
            return this;
        }
    }


    /// <summary>
    /// Posted by a Health component when it would have taken damage had it not been marked as being in Godmode.
    /// </summary>
    public class HealthDamageAbsorbedEvent : IMessageEvent
    {
        public static HealthDamageAbsorbedEvent Shared = new HealthDamageAbsorbedEvent(null, null, 0);

        public GameObject Agent { get; protected set; }
        public Health Health { get; protected set; }
        public int Difference { get; protected set; }

        public HealthDamageAbsorbedEvent(GameObject agent, Health health, int diff)
        {
            Agent = agent;
            Health = health;
            Difference = diff;
        }

        public HealthDamageAbsorbedEvent ChangeValues(GameObject agent, Health health, int diff)
        {
            Agent = agent;
            Health = health;
            Difference = diff;
            return this;
        }
    }


    /// <summary>
    /// Posted by a Health component to the attacker when the target would have taken damage had it not been marked as being in Godmode.
    /// </summary>
    public class HealthDamageAbsorbedByOtherEvent : IMessageEvent
    {
        public static HealthDamageAbsorbedByOtherEvent Shared = new HealthDamageAbsorbedByOtherEvent(null, null, 0);

        public GameObject Agent { get; protected set; }
        public Health Health { get; protected set; }
        public int Difference { get; protected set; }

        public HealthDamageAbsorbedByOtherEvent(GameObject agent, Health health, int diff)
        {
            Agent = agent;
            Health = health;
            Difference = diff;
        }

        public HealthDamageAbsorbedByOtherEvent ChangeValues(GameObject agent, Health health, int diff)
        {
            Agent = agent;
            Health = health;
            Difference = diff;
            return this;
        }
    }


    /// <summary>
    /// Posted by Health Component when its internal current health goes down.
    /// </summary>
    public class HealthLostEvent : IMessageEvent
    {
        public GameObject Agent { get; protected set; }
        public Health Health { get; protected set; }

        /// <summary>
        /// This will always be negative.
        /// </summary>
        public int Difference { get; protected set; }

        public HealthLostEvent(GameObject agent, Health health, int diff)
        {
            Agent = agent;
            Health = health;
            Difference = diff;
        }

        public HealthLostEvent ChangeValues(GameObject agent, Health health, int diff)
        {
            Agent = agent;
            Health = health;
            Difference = diff;
            return this;
        }
    }


    /// <summary>
    /// Posted by Health Component when its internal current health goes up.
    /// </summary>
    public class HealthGainedEvent : IMessageEvent
    {
        public GameObject Agent { get; protected set; }
        public Health Health { get; protected set; }

        /// <summary>
        /// This will always be positive.
        /// </summary>
        public int Difference { get; protected set; }

        public HealthGainedEvent(GameObject agent, Health health, int diff)
        {
            Agent = agent;
            Health = health;
            Difference = diff;
        }

        public HealthGainedEvent ChangeValues(GameObject agent, Health health, int diff)
        {
            Agent = agent;
            Health = health;
            Difference = diff;
            return this;
        }
    }


    /// <summary>
    /// Forwarded to the agent by the Health Component when its internal current health value changes.
    /// </summary>
    public class CausedHealthChangedEvent : IMessageEvent
    {
        public GameObject Agent { get; protected set; }
        public Health Health { get; protected set; }
        public int Difference { get; protected set; }

        public CausedHealthChangedEvent(GameObject agent, Health health, int diff)
        {
            Agent = agent;
            Health = health;
            Difference = diff;
        }

        public CausedHealthChangedEvent ChangeValues(GameObject agent, Health health, int diff)
        {
            Agent = agent;
            Health = health;
            Difference = diff;
            return this;
        }
    }


    /// <summary>
    /// Forwarded directly to an entity to let them
    /// know they should change their health by a certain value.
    /// </summary>
    public class ChangeHealthCmd : IMessageCommand
    {
        public GameObject Agent { get; protected set; }
        public int Change { get; protected set; }
        public bool HonorInvincibility { get; protected set; }

        public ChangeHealthCmd(GameObject agent, int change, bool honorInvincibility = true)
        {
            Agent = agent;
            Change = change;
            HonorInvincibility = honorInvincibility;
        }

        public ChangeHealthCmd ChangeValues(GameObject agent, int change, bool honorInvincibility = true)
        {
            Agent = agent;
            Change = change;
            HonorInvincibility = honorInvincibility;
            return this;
        }
    }


    /// <summary>
    /// Forwarded directly to an entity to let
    /// them know they should reduce their health to zero and trigger
    /// an EntityDiedEvent.
    /// </summary>
    public class KillEntityCmd : AgentTargetMessage<GameObject, GameObject, KillEntityCmd>
    {
        public KillEntityCmd() : base() { }
        public KillEntityCmd(GameObject agent, GameObject target) : base(agent, target) { }
    }


    /// <summary>
    /// Forwarded directly to an entity to let
    /// them know they should reduce their health to zero and trigger
    /// an EntityDiedEvent. This version will always be handled even if the health
    /// component is disabled.
    /// </summary>
    public class KillEntityForcedCmd : KillEntityCmd
    {
        public static KillEntityForcedCmd Shared = new KillEntityForcedCmd(null, null);
        public KillEntityForcedCmd(GameObject agent, GameObject target) : base(agent, target) { }

        public new KillEntityForcedCmd Change(GameObject agent, GameObject target)
        {
            Agent = agent;
            Target = target;
            return this;
        }
    }


    /// <summary>
    /// Forwarded directly to an entity to let
    /// them know they should revive if dead.
    /// </summary>
    public class ReviveEntityCmd : TargetMessage<GameObject, ReviveEntityCmd>
    {
        public ReviveEntityCmd(GameObject target) : base(target) { }
    }


    /// <summary>
    /// Message to inform concerned parties that a given entity
    /// died as the result of another entity. If this is processed locally
    /// on an entity it can be presumed that the target is itself.
    /// </summary>
    public class EntityDiedEvent : AgentTargetMessage<GameObject, Health, EntityDiedEvent>
    {
        public EntityDiedEvent() : base() { }
        public EntityDiedEvent(GameObject agent, Health target) : base(agent, target) { }

        public EntityDiedEvent ChangeValues(GameObject agent, Health target)
        {
            Agent = agent;
            Target = target;
            return this;
        }
    }


    /// <summary>
    /// Message to inform concerned parties that an entity that
    /// was previously dead has been revived. If processed locally
    /// it can be presumed that the target is itself.
    /// </summary>
    public class EntityRevivedEvent : TargetMessage<Health, EntityRevivedEvent>
    {
        public EntityRevivedEvent(Health target) : base(target) { }

        public EntityRevivedEvent ChangeValues(GameObject agent, Health target)
        {
            Target = target;
            return this;
        }
    }


    /// <summary>
    /// Message sent to the agent that caused another entity to die.
    /// If processed locally it can be presumed that the agent is itself.
    /// </summary>
    public class KilledEntityEvent : AgentTargetMessage<GameObject, Health, KilledEntityEvent>
    {
        public int Damage { get; protected set; }
        public KilledEntityEvent() : base() { }
        public KilledEntityEvent(GameObject agent, Health target, int damage) : base(agent, target)
        {
            Damage = damage;
        }

        public KilledEntityEvent ChangeValues(GameObject agent, Health target, int damage)
        {
            Damage = damage;
            Agent = agent;
            Target = target;
            return this;
        }
    }


    /// <summary>
    /// Message sent to the agent that caused another entity to revive.
    /// If processed locally it can be presumed that the agent is itself.
    /// </summary>
    public class RevivedEntityEvent : AgentTargetMessage<GameObject, Health, RevivedEntityEvent>
    {
        public RevivedEntityEvent() : base() { }
        public RevivedEntityEvent(GameObject agent, Health target) : base(agent, target) { }

        public RevivedEntityEvent ChangeValues(GameObject agent, Health target)
        {
            Agent = agent;
            Target = target;
            return this;
        }
    }


    /// <summary>
    /// Message for demanding a health component.
    /// </summary>
    public class DemandHealthComponent : Demand<Health>
    {
        public static DemandHealthComponent Shared = new(null);
        public DemandHealthComponent(Action<Health> callback) : base(callback) { }
    }
}
