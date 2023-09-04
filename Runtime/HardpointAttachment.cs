using UnityEngine;


namespace Peg.Game
{
    /// <summary>
    /// Attachable to a Hardpoint component.
    /// </summary>
    public class HardpointAttachment : MonoBehaviour
    {
        [Tooltip("User-friendly, descriptive name of this hardpoint.")]
        public HashedString Id;

        public Hardpoint AttachedTo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hardpoint"></param>
        /// <returns></returns>
        public bool AttachTo(Hardpoint hardpoint)
        {
            var trans = this.transform;

            if(hardpoint == null)
            {
                //detatch from previous
                if (AttachedTo != null)
                {
                    AttachedTo = null;
                    trans.SetParent(null, false);
                    return true;
                }
                //wasn't attached anyway, just fail
                return false;
            }
            else
            {
                //same as before, early out with success
                if (AttachedTo == hardpoint)
                    return true;

                //not valid, fail and leave old attachment
                if (hardpoint.Id.Hash != this.Id.Hash)
                    return false;

                //change to new hardpoint
                AttachedTo = hardpoint;
                trans.SetParent(hardpoint.transform, false);
                trans.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                return true;

            }


            
        }
    }
}