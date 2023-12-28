using Peg.AutonomousEntities;
using System;
using UnityEngine;

namespace Peg.Game.ConsumableResource
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
}
