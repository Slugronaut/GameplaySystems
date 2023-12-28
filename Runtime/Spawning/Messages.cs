
using Peg.AutonomousEntities;
using Peg.MessageDispatcher;

namespace Peg.Game.Spawning
{
    /// <summary>
    /// 
    /// </summary>
    public class EntitySpawnedEvent : AgentTargetMessage<ISpawner, EntityRoot, EntitySpawnedEvent>
    {
        public static EntitySpawnedEvent Shared = new(null, null);
        public EntitySpawnedEvent() : base() { }
        public EntitySpawnedEvent(ISpawner agent, EntityRoot target) : base(agent, target) { }
    }


    /// <summary>
    /// 
    /// </summary>
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
