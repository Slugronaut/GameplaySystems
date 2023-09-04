using UnityEngine;
using System.Collections.Generic;
using Peg.Messaging;
using Peg.Util;

namespace Peg.Game
{
    /// <summary>
    /// Periodically searched fro nearby followers
    /// and sets their brain's follower field.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Leader : LocalListenerMonoBehaviour
    {
        [Tooltip("A tag that is used by followers to see if they can follow this leader.")]
        public HashedString LeaderTag = new HashedString("Leader");
        [Tooltip("How frequently we should attract new nearby followers.")]
        public float Period = 1;
        [Tooltip("How far away we can attract followers.")]
        public float Range = 8;
        [Tooltip("The mask that to check for followers on.")]
        public LayerMask FollowerMask;
        [Tooltip("The maximum allowed followers for this leader.")]
        public int MaxFollowers = 20;
        
        /// <summary>
        /// Message posted localled when a follower has started following a leader.
        /// </summary>
        public class GainedFollowerEvent : TargetMessage<Follower, GainedFollowerEvent>, IMessageEvent
        {
            public GainedFollowerEvent(Follower target) : base(target) { }
        }

        /// <summary>
        /// Message posted when a Leader has lost a follower.
        /// </summary>
        public class LostFollowerEvent : TargetMessage<Follower, LostFollowerEvent>, IMessageEvent
        {
            public LostFollowerEvent(Follower target) : base(target) { }
        }

        static LostFollowerEvent LostFollowerMsg = new LostFollowerEvent(null);
        static GainedFollowerEvent GainedFollowerMsg = new GainedFollowerEvent(null);
        List<Follower> Followers;
        float Last;
        

        void Awake()
        {
            Followers = new List<Follower>(MaxFollowers);
        }

        void OnEnable()
        {
            DispatchRoot.AddLocalListener<LostFollowerEvent>(HandleLostFollower);
        }

        void OnDisable()
        {
            DispatchRoot.RemoveLocalListener<LostFollowerEvent>(HandleLostFollower);
            foreach (var follower in Followers.ToArray())
            {
                if (!TypeHelper.IsReferenceNull(follower))
                    follower.Leader = null;
            }
            Followers.Clear();
        }

        public void RemoveFollower(Follower follower)
        {
            if (Followers.Remove(follower))
                GlobalMessagePump.Instance.ForwardDispatch(gameObject, LostFollowerMsg.Change(follower) as LostFollowerEvent);
        }

        public void AddFollower(Follower follower)
        {
            if(!Followers.Contains(follower))
            {
                Followers.Add(follower);
                GlobalMessagePump.Instance.ForwardDispatch(gameObject, GainedFollowerMsg.Change(follower) as GainedFollowerEvent);
            }
        }

        void HandleLostFollower(LostFollowerEvent msg)
        {
            Followers.Remove(msg.Target);
        }

        void Update()
        {
            if(Followers.Count < MaxFollowers && Time.time - Last >= Period)
            {
                AttractNewFollowers();
                Last = Time.time;
            }
        }

        public void AttractNewFollowers()
        {
            var list = SharedArrayFactory.RequestTempArray<Collider>(MaxFollowers); //NOTE: we can't get more than MaxFollowers followers at a time - most likely this will be a hard limit too.
            var hits = Physics.OverlapSphereNonAlloc(transform.position, Range, list, FollowerMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < hits - Followers.Count; i++)
            {
                var follower = list[i].gameObject.FindComponentInEntity<Follower>(true);
                if (follower != null && follower.isActiveAndEnabled)
                    follower.Leader = this;
            }
        }
    }
}
