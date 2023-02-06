using UnityEngine;

namespace Toolbox.Behaviours
{
    /// <summary>
    /// Posts an EntityDespawned message when a Unity event occurs.
    /// </summary>
    public class PostDespawnOnEvent : AbstractOperationOnEvent
    {
        EntityRoot Root;

        protected override void Awake()
        {
            Root = gameObject.GetEntityRoot();
            if (Root == null) throw new UnityException("Must have EntityRoot in order to post a valid despawn message.");

            base.Awake();
        }

        /// <summary>
        /// Posts the despawn message for this entity.
        /// </summary>
        public override void PerformOp()
        {
            GlobalMessagePump.Instance.PostMessage(EntityDespawnedEvent.Shared.Change(null, Root) as EntityDespawnedEvent);
        }
    }
}
