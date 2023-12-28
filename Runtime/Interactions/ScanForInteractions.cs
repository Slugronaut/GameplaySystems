using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Peg.Collections;

namespace Peg.Game.Interactions
{
    /// <summary>
    /// Used to determine what interactables are nearby and
    /// which one will take priority. It is expected that all
    /// Actor components are attached to this GameObjects.
    /// </summary>
    public class ScanForInteractions : MonoBehaviour
    {
        [Tooltip("The sprite that is used to render the current interaction icon.")]
        public SpriteRenderer IconRenderer;
        [Tooltip("The layer mask to scan against.")]
        public LayerMask ScanMask;
        [Tooltip("Radius of scan area.")]
        public float Radius;
        [Tooltip("Time in seconds between each scan.")]
        public float Freq = 0.1f;
        [Tooltip("Do any interactions detected within range also require Line-of-Sight to be valid?")]
        public bool RequireLos = true;

        Transform Trans;
        float LastTime;


        List<AbstractInteractor> Actors;
#pragma warning disable CS0169 // The field 'ScanForInteractions.Actions' is never used
        List<IInteractable> Actions;
#pragma warning restore CS0169 // The field 'ScanForInteractions.Actions' is never used


        void Awake()
        {
            Trans = transform;
            Actors = new List<AbstractInteractor>(GetComponentsInChildren<Actor>());
        }

        public void Update()
        {
            float t = Time.time;
            if (t - LastTime < Freq) return;
            LastTime = t;

            var cols = SharedArrayFactory.RequestTempArray<Collider>(5);
            if (Physics.OverlapSphereNonAlloc(Trans.position, Radius, cols, ScanMask, QueryTriggerInteraction.Collide) > 0)
            {
                //TODO: Profile this. It might be producing a lot of garbage!
                var gos = cols.Where(x => x != null).Select(x => x.gameObject);
                var actions = AbstractInteractable.CondenseAllInteractables(gos);

                AbstractInteractor actor = null;
                IInteractable action = null;
                if (AbstractInteractor.FindCompatibleInteraction(Actors, actions, out actor, out action))
                {
                    //TODO: get the appropriate icon to display from the action and flag the input that this is a valid option
                    Debug.Log("Interaction found between " + actor.GetType().Name + " and " + action.GetType().Name + "!");
                }

            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }

    }
    
}
