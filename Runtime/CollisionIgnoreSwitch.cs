using Peg.AutonomousEntities;
using UnityEngine;


namespace Peg.Game
{
    /// <summary>
    /// When a gameobject with this component is triggered, the object that collided is
    /// made to ignore another set of colliders.
    /// </summary>
    public class CollisionIgnoreSwitch : MonoBehaviour
    {
        public enum IgnoreModes
        {
            #if !TOOLBOX_2DCOLLIDER
            CharacterController,
            #endif
            SingleCollider,
            AllColliders,
        }

        public IgnoreModes Mode;
        public Collider[] ToIgnore;


        #if TOOLBOX_2DCOLLIDER
        private void OnTriggerEnter2D(Collider2D other)
        {
            switch (Mode)
            {
                case IgnoreModes.SingleCollider:
                    {
                        throw new UnityException("Not implemented.");
                    }
                case IgnoreModes.AllColliders:
                    {
                        throw new UnityException("Not implemented.");
                    }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            switch (Mode)
            {
                case IgnoreModes.SingleCollider:
                    {
                        throw new UnityException("Not implemented.");
                    }
                case IgnoreModes.AllColliders:
                    {
                        throw new UnityException("Not implemented.");
                    }
            }
        }
        #else

        private void OnTriggerEnter(Collider other)
        {
            if (Mode == IgnoreModes.CharacterController)
            {
                for (int i = 0; i < ToIgnore.Length; i++)
                    Physics.IgnoreCollision(other.gameObject.FindComponentInEntity<CharacterController>(), ToIgnore[i], true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            switch(Mode)
            {
                case IgnoreModes.CharacterController:
                    {
                        //this should very rarely ever be trigger so I'm not worried about speed here
                        for (int i = 0; i < ToIgnore.Length; i++)
                            Physics.IgnoreCollision(other.gameObject.FindComponentInEntity<CharacterController>(), ToIgnore[i], false);

                        break;
                    }
                case IgnoreModes.SingleCollider:
                    {
                        throw new UnityException("Not implemented.");
                    }
                case IgnoreModes.AllColliders:
                    {
                        throw new UnityException("Not implemented.");
                    }
            }
        }
        #endif

    }
}