using System;
using System.Collections.Generic;
using Peg.Util;
using UnityEngine.Events;

namespace Peg.Game
{
    /// <summary>
    /// Allows an object to receive notification when touching a trigger of a 'Useable'.
    /// </summary>
    public class User : Interactor
    {
        [Serializable]
        public class UseableEvent: UnityEvent<Useable, User> { }

        [Serializable]
        public class SetCurrentUseable : UnityEvent<Useable, User> { }

        public UseableEvent OnSetCurrent = new UseableEvent();
        public UseableEvent OnRemoveCurrent = new UseableEvent();



        public Useable Current { get; protected set; }
        Useable DisabledRef;
        public override bool CanUseSubclasses { get { return true; } protected set { } }


        void OnDisable()
        {
            DisabledRef = Current;
            if(DisabledRef != null)
                Unregister(Current);
        }

        void OnEnable()
        {
            //need to make sure interaction is still feasable otherwise we might
            //get stuck permanently in an invalid state where this interactio won't go away
            if (DisabledRef != null && this.CanInteract(DisabledRef) == Use.Success)
                Register(DisabledRef);
            DisabledRef = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DisabledRef = null;
        }

        protected override Type DeclaredUseable()
        {
            return typeof(Useable);
        }

        protected override void Interaction(IInteractable interactable)
        {
        }

        Dictionary<int, List<Useable>> Registered = new();
        /// <summary>
        /// Called by 'useable' when it detects this user has entered its trigger collider.
        /// </summary>
        /// <param name="used"></param>
        public virtual void Register(Useable used)
        {
            if (Current == null || (used.Priority > Current.Priority))
            {
                Current = used;
                OnSetCurrent.Invoke(used, this);
            }
            Registered.AddListed(used.Priority, used);
        }

        /// <summary>
        /// Called by 'useable' if it previously registered an interaction and now it want to revoke it.
        /// Usually called because triggers have exited each other and an interaction is no longer possible.
        /// </summary>
        /// <param name="used"></param>
        public virtual void Unregister(Useable used)
        {
            Registered.RemoveListed(used.Priority, used);

            //if the useable that was just unregistered is also out current useable,
            //then we need to find the next available useable that was previous registered.
            if(Current == used)
            {
                OnRemoveCurrent.Invoke(used, this);

                if (Registered.Count < 1)
                {
                    Current = null;
                    return;
                }
                else
                {
                    int nextHighest = int.MinValue;
                    foreach(var key in Registered.Keys)
                    {
                        if (key > nextHighest)
                            nextHighest = key;
                    }

                    //due to the way 'RemovedListed()' works, this should always have at least
                    //one value in the list if there is a key for it so we shouldn't have to check
                    //for an empty list here.
                    Current = Registered[nextHighest][0];
                    OnSetCurrent.Invoke(Current, this);
                }
            }
        }


    }
}

