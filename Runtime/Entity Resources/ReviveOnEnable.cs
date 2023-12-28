using Peg.AutonomousEntities;
using Peg.GCCI;
using UnityEngine;

namespace Peg.Game.ConsumableResource
{
    public class ReviveOnEnable : MonoBehaviour
    {

        void OnEnable()
        {
            var agent = gameObject.FindComponentInEntity<IPathAgentAdaptor>(true);
            if (agent != null) agent.FreezeMotion = false;

            var health = gameObject.FindComponentInEntity<Health>(true);
            if (health != null)
            {
                //we are assuming that this entity is probably being
                //recycled from the pool and needs to be revived
                //at the time they are respawned

                //we need to force health to be 0 so that we can be sure the appropriate
                //'Revive' event is posted when we reset the health to the proper value
                health.SetHealth(null, 0, true); //suppress events so we don't 'die' again!
                health.SetHealth(null, health.MaxHealth);

            }
        }

    }
}
