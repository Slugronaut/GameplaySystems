using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Toolbox.Collections;

namespace Toolbox.Game
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
        

        List<Interactor> Actors;
#pragma warning disable CS0169 // The field 'ScanForInteractions.Actions' is never used
        List<IInteractable> Actions;
#pragma warning restore CS0169 // The field 'ScanForInteractions.Actions' is never used


        void Awake()
        {
            Trans = transform;
            Actors = new List<Interactor>(GetComponentsInChildren<Actor>());
        }

        public void Update()
        {
            float t = Time.time;
            if (t - LastTime < Freq) return;
            LastTime = t;

            var cols = SharedArrayFactory.RequestTempArray<Collider>(5);
            if(Physics.OverlapSphereNonAlloc(Trans.position, Radius, cols, ScanMask, QueryTriggerInteraction.Collide) > 0)
            {
                //TODO: Profile this. It might be producing a lot of garbage!
                var gos = cols.Where(x => x != null).Select(x => x.gameObject);
                var actions = Interactable.CondenseAllInteractables(gos);

                Interactor actor = null;
                IInteractable action = null;
                if(Interactor.FindCompatibleInteraction(Actors, actions, out actor, out action))
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


    /// <summary>
    /// Derived interactors from this that should be part of the interaction-scanning process on this entity.
    /// </summary>
    public class Actor : Interactor
    {
        private void Reset()
        {
            CanUseSubclasses = true;
        }

        protected override void Awake()
        {
            base.Awake();
            CanUseSubclasses = true;
        }

        protected override Type DeclaredUseable()
        {
            CanUseSubclasses = true;
            return typeof(Action);
        }

        protected override void Interaction(IInteractable interactable)
        {
            Action a = interactable as Action;
            a.OnInteract(this);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public abstract class Action : Interactable
    {
        public Sprite UseIcon;


        private void Reset()
        {
            UseableBySubclasses = true;
        }

        protected override void Awake()
        {
            base.Awake();
            UseableBySubclasses = true;
        }

        protected override Type DeclaredUser()
        {
            UseableBySubclasses = true;
            return typeof(Actor);
        }
    }
}
