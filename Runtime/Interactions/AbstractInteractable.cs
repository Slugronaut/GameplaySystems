using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Peg.Messaging;
using Peg.AutonomousEntities;

namespace Peg.Game.Interactions
{
    /// <summary>
    /// Base class from which all interactable objects can derive. Used for 
    /// things like pickups, attackables, items, doors, switches, buttons, etc.
    /// </summary>
    public abstract class AbstractInteractable : LocalListenerMonoBehaviour, IInteractable
    {
        [SerializeField]
        bool _UseableBySubclasses = false;
        [TabGroup("Interactable")]
        [PropertyTooltip("If true, this interactable can be targeted by a type derived from its valid Target Type.")]
        [ShowInInspector]
        public virtual bool UseableBySubclasses { get { return _UseableBySubclasses; } set { _UseableBySubclasses = value; } }

        [SerializeField]
        bool _CanUseSelf = false;
        [TabGroup("Interactable")]
        [PropertyTooltip("If true, this interactable can be targeted by the same GameObject that is interacting with it.")]
        [ShowInInspector]
        public bool CanUseSelf { get { return _CanUseSelf; } set { _CanUseSelf = value; } }

        [SerializeField]
        float _MaxRange = 5.0f;
        [TabGroup("Interactable")]
        [PropertySpace(10)]
        [PropertyTooltip("The max radius of this interactable's use range.")]
        [ShowInInspector]
        public float MaxRange
        {
            get { return _MaxRange; }
            set { _MaxRange = value; }
        }

        [SerializeField]
        float _MinRange = 0.0f;
        [TabGroup("Interactable")]
        [PropertyTooltip("The min radius of this interactable's use range.")]
        [ShowInInspector]
        public float MinRange
        {
            get { return _MinRange; }
            set { _MinRange = value; }
        }

        [SerializeField]
        float _ReuseDelay = 0.5f;
        [TabGroup("Interactable")]
        [PropertyTooltip("Can be treated as a delay between uses or as the time it takes to use.")]
        [ShowInInspector]
        public float ReuseDelay
        {
            get { return _ReuseDelay; }
            set { _ReuseDelay = value; }
        }

        [HideInInspector]
        [SerializeField]
        int _Priority = 0;
        [TabGroup("Interactable")]
        [PropertyTooltip("Priority of the interaction. Lower values have higher priority.")]
        [ShowInInspector]
        public int Priority { get { return _Priority; } set { _Priority = value; } }

        [SerializeField]
        Vector3 _ActivationPointOffset;
        [TabGroup("Interactable")]
        [PropertyTooltip("The local-space offset of this interactable's point of useage.")]
        [ShowInInspector]
        public Vector3 ActivationPointOffset
        {
            get { return _ActivationPointOffset; }
            set { _ActivationPointOffset = value; }
        }


        /// <summary>
        /// This interactable's entity root.
        /// </summary>
        public EntityRoot Entity { get; private set; }

        private double LastInteractionTime;
        [HideInInspector]
        public bool ReadyForUse { get; private set; }
        public Vector3 ActivationPosition { get { return transform.position + ActivationPointOffset; } }
        [HideInInspector]
        public Type UserType { get; protected set; }



#if UNITY_EDITOR
        [TabGroup("Interactable")]
        public Color GizmoColor = Color.magenta;

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = GizmoColor;
            Gizmos.DrawWireSphere(transform.position + ActivationPointOffset, MaxRange);
        }
#endif

        protected virtual void Awake()
        {
            UserType = DeclaredUser();
            Entity = gameObject.GetEntityRoot();
            if (Entity == null) Debug.LogWarning("Many features may break if " + GetType().Name + " is not part of an Autonomous Entity Hierarchy.");
        }

        protected virtual void OnEnable()
        {
            DispatchRoot.AddLocalListener<InteractableUseEvent>(HandleUse);
        }

        protected virtual void OnDisable()
        {
            DispatchRoot.RemoveLocalListener<InteractableUseEvent>(HandleUse);
        }

        void HandleUse(InteractableUseEvent msg)
        {
            //we need to ensure the interactor that triggered this message is a valid user of this interactable
            //BUG ALERT: TODO: This needs to check for the ability to subclass as well!
            if (msg.User.GetType() == UserType)
            {
                Callback_ConsumeUse();
                OnInteract(msg.User);
            }
        }

        /// <summary>
        /// Callback that is invoked internally by Interactor when an interaction has taken place.
        /// This helps to internally track things like last use time, etc...
        /// </summary>
        public void Callback_ConsumeUse()
        {
            //due to the fact that we can't make this private it is vital that
            //this not break if called multple times during a single interaction.
            LastInteractionTime = Time.timeAsDouble;
        }

        /// <summary>
        /// TODO: Remove this Update method and make this check inside the ReadyForUse property getter.
        /// </summary>
        protected virtual void Update()
        {
            //ok to compare float to 0.0 if it was specifically set
            if (ReuseDelay == 0.0f || Time.timeAsDouble - LastInteractionTime >= ReuseDelay)
                ReadyForUse = true;
            else ReadyForUse = false;
        }

        /// <summary>
        /// Used to manually activate the reuse delay, thus disallowing interaction.
        /// </summary>
        protected void SetUseDelay()
        {
            LastInteractionTime = Time.timeAsDouble;
            ReadyForUse = false;
        }

        /// <summary>
        /// Used to manually remove the reuse delay, thus allowing interaction.
        /// </summary>
        protected void ResetUseDelay()
        {
            LastInteractionTime = Time.timeAsDouble - (ReuseDelay + 0.00001f);
            ReadyForUse = true;
        }

        /// <summary>
        /// Implemented in derived classes so that this system knows what type it can be used by.
        /// </summary>
        /// <returns></returns>
        protected abstract Type DeclaredUser();

        /// <summary>
        /// Override this to provide specific functionality. This will be called by the interactor upon use.
        /// </summary>
        /// <param name="user"></param>
        public abstract void OnInteract(AbstractInteractor user);

        /// <summary>
        /// Condenses all interactables found on all supplied GameObjects' AEHes into a single list.
        /// </summary>
        /// <param name="gameObjects"></param>
        /// <returns></returns>
        public static List<IInteractable> CondenseAllInteractables(List<GameObject> gameObjects)
        {
            var list = new List<IInteractable>(25);
            if (gameObjects == null) return list;

            for (int i = 0; i < gameObjects.Count; i++)
                list.AddRange(gameObjects[i].FindComponentsInEntity<IInteractable>());
            return list;
        }

        /// <summary>
        /// Condenses all interactables found on all supplied GameObjects' AEHes into a single list.
        /// </summary>
        /// <param name="gameObjects"></param>
        /// <returns></returns>
        public static List<IInteractable> CondenseAllInteractables(IEnumerable<GameObject> gameObjects)
        {
            var list = new List<IInteractable>(25);
            if (gameObjects == null) return list;

            foreach (GameObject go in gameObjects)
                list.AddRange(go.FindComponentsInEntity<IInteractable>());
            return list;
        }

        /// <summary>
        /// Condenses all interactables found on all supplied GameObjects' AEHes into a single list.
        /// </summary>
        /// <param name="gameObjects"></param>
        /// <returns></returns>
        public static List<IInteractable> CondenseAllInteractables(List<EntityRoot> entities, bool cacheComponents = false)
        {
            var list = new List<IInteractable>(25);
            if (entities == null) return list;

            for (int i = 0; i < entities.Count; i++)
                list.AddRange(entities[i].FindComponentsInEntity<IInteractable>(cacheComponents));
            return list;
        }
    }

}
