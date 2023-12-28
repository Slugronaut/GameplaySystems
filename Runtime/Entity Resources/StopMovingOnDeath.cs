using Peg.AutonomousEntities;
using Peg.GCCI;
using Peg.Messaging;
using UnityEngine;

namespace Peg.Game.ConsumableResource
{
    /// <summary>
    /// Cancels the last movement request made upon recieving a death event.
    /// Unfreezes pathing upon Revival event.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StopMovingOnDeath : LocalListenerMonoBehaviour
    {
        IPathAgentAdaptor Agent;


        void Awake()
        {
            Agent = gameObject.FindComponentInEntity<IPathAgentAdaptor>();
            DispatchRoot.AddLocalListener<EntityDiedEvent>(HandleDeath);
            DispatchRoot.AddLocalListener<EntityRevivedEvent>(HandleRevive);
        }

        protected override void OnDestroy()
        {
            DispatchRoot.RemoveLocalListener<EntityDiedEvent>(HandleDeath);
            DispatchRoot.RemoveLocalListener<EntityRevivedEvent>(HandleRevive);
            base.OnDestroy();
        }

        void HandleDeath(EntityDiedEvent msg)
        {
            if (Agent != null)
            {
                Agent.FreezeMotion = true;
                Agent.CancelDest();
            }
        }

        void HandleRevive(EntityRevivedEvent msg)
        {
            if(Agent != null)
            {
                Agent.FreezeMotion = false;
            }
        }
    }
}
