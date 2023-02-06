using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace Toolbox.Game
{
    /// <summary>
    /// Allows this object to display an interaction option when a 'User' touches it with a trigger collider.
    /// </summary>
    public class Useable : TouchInteractable
    {
        [TabGroup("Useable")]
        [Tooltip("Should this object call SendMessage on all components with 'OnUse'?")]
        public bool PostMessageOnUse = true;

        [TabGroup("Useable")]
        [Tooltip("What icon is to be displayed when this interaction becomes available?")]
        public Sprite CueIcon;

        [TabGroup("Useable")]
        [Tooltip("What text is displayed when this interaction becomes available?")]
        public string CueText;

        [TabGroup("Useable")]
        [Tooltip("An id that can be used by handlers to determine what course of action to take. (eg the PlayerUserAction component uses this to determine what button performs the action.)")]
        public HashedString InteractionId;

        public enum UseReRegistry
        {
            None = 0,
            Unregister = 1,
            Reregister = 2,
        }
        [TabGroup("Useable")]
        [Tooltip("This resets registration on use so that if multiple objects of the same priority are overlapping (wepaon next to a door) we can 'switch' between them without getting stuck with access to only one of them.")]
        public UseReRegistry Reregister = UseReRegistry.Reregister;

        [TabGroup("Useable")]
        public User.UseableEvent OnUse;

        HashSet<User> PotentialUsers = new HashSet<User>();


        public override bool UseableBySubclasses { get { return true; } set { } }


        /// <summary>
        /// Posted to inform a Useable that it has been used.
        /// </summary>
        public class UsedByOtherEvent : IMessageEvent
        {
            public static UsedByOtherEvent Shared = new UsedByOtherEvent();

            public User User { get; protected set; }

            public UsedByOtherEvent(User user)
            {
                User = user;
            }

            public UsedByOtherEvent() { }

            public UsedByOtherEvent Change(User user)
            {
                User = user;
                return this;
            }
        }



        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            int count = PotentialUsers.Count;
            while(PotentialUsers.Count > 0)
            {
                var user = PotentialUsers.First();
                user.Unregister(this);
                PotentialUsers.Remove(user);
            }
        }

        /// <summary>
        /// Handles the final interaction.
        /// </summary>
        /// <param name="user"></param>
        public override void OnInteract(Interactor interactor)
        {
            var user = interactor as User;
            if (PostMessageOnUse)
                GlobalMessagePump.Instance.ForwardDispatch(gameObject, UsedByOtherEvent.Shared.Change(user));
            OnUse.Invoke(this, user);
            
            if(Reregister == UseReRegistry.Unregister)
                ForceUnregister(user);
            if (Reregister == UseReRegistry.Reregister)
            {
                ForceUnregister(user);
                ForceRegister(user);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Type DeclaredUser()
        {
            return typeof(User);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="who"></param>
        protected override void ProcessTouch(GameObject who)
        {
            //NOTE: We should have to worry about duplicate entries due to the nature
            //of physics but if we get a lot of exceptions here (because sometimes physics breaks)
            //it might be worth checking for duplicates before attempting to add.
            ForceRegister(who.FindComponentInEntity<User>(true));
        }

        /// <summary>
        /// BUG: This never gets called if the collider is simply disabled!
        /// </summary>
        /// <param name="other"></param>
        public void OnTriggerExit(Collider other)
        {
            ForceUnregister(other.gameObject.FindComponentInEntity<User>(true));
        }

        /// <summary>
        /// Used to manually register this useable interaction with a user.
        /// This method assumes that the GameObject that 'user' is attached to
        /// is also the object that has the collider for 'user'. This is important
        /// because when the user's trigger exist this useable it makes that assumption
        /// for Unregistration!
        /// </summary>
        /// <param name="user"></param>
        public void ForceRegister(User user)
        {
            if (user == null) return;
            if (PotentialUsers.Add(user))
                user.Register(this);
        }

        /// <summary>
        /// This is used to manually disengaged an items use registration when the
        /// situation calls for it (like items being grabbed that should disable
        /// their interaction collider).
        /// </summary>
        /// <param name="user"></param>
        public void ForceUnregister(User user)
        {
            if (user == null) return;
            if (PotentialUsers.Remove(user))
                user.Unregister(this);
        }
    }
}
