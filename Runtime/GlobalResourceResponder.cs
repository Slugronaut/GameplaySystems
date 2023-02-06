using System;
using UnityEngine;


namespace Toolbox.Game
{
    /// <summary>
    /// Responds to globally posted Demands for an <see cref="IEntityResource"/>.
    /// </summary>
    public class GlobalResourceResponder : MonoBehaviour
    {
        /// <summary>
        /// Message used to query globally for a resource responder.
        /// </summary>
        public class DemandEntityResource : Demand<IEntityResource>
        {
            public static DemandEntityResource Shared = new DemandEntityResource(null, -1);
            public int IdHash { get; private set; }
            public DemandEntityResource(Action<IEntityResource> callback, int idHash) : base(callback)
            {
                IdHash = idHash;
            }

            public static DemandEntityResource PrepareDemand(int idHash)
            {
                Shared.Reset();
                Shared.IdHash = idHash;
                return Shared;
            }

            public static int QueryResponse => Shared.Responded ? Shared.IdHash : throw new UnityException("No response for GlobalResponseResponder");
        }


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
