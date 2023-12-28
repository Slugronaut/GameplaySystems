using Peg.MessageDispatcher;
using UnityEngine;


namespace Peg.Game.ConsumableResource
{
    /// <summary>
    /// Responds to globally posted Demands for an <see cref="IEntityResource"/>.
    /// </summary>
    public class GlobalResourceResponder : MonoBehaviour
    {
        [Tooltip("An id used to match which global resource responder is being queried.")]
        public HashedString Id;
        /// <summary>
        /// I'd rather use IEntiryResource but I don't want to fall back on Odin's serialization
        /// so we're sticking with this for now.
        /// </summary>
        [Tooltip("The resource to respond with when demanded.")]
        public EntityResource Resource;



        private void Awake()
        {
            GlobalMessagePump.Instance.AddListener<DemandEntityResource>(HandleDemand);
        }

        private void OnDestroy()
        {
            GlobalMessagePump.Instance.RemoveListener<DemandEntityResource>(HandleDemand);
        }

        void HandleDemand(DemandEntityResource msg)
        {
            if(msg.IdHash == Id.Hash)
                msg.Respond(Resource);
        }
    }
}
