using Toolbox.Messaging;
using UnityEngine;


namespace Toolbox.Game
{
    /// <summary>
    /// Attach to an entity that is detectable by a Leader
    /// and can be set to follow it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Follower : LocalListenerMonoBehaviour
    {
        [Tooltip("A list of tags that this follower will accept as a leader. If this list is empty, any leader will be accepted.")]
        public HashedString[] ValidLeaders;


        //Blackboard Memory;
        //static readonly string LeaderVarName = "Leader";
        //static Leader.LostFollower LostFollowerMsg = new Leader.LostFollower(null);
        static LostLeaderEvent LostLeaderMsg = new LostLeaderEvent(null);
        static GainedLeaderEvent GainedLeaderMsg = new GainedLeaderEvent(null);

        /// <summary>
        /// Message posted locally when a follower has lost their leader.
        /// </summary>
        public class LostLeaderEvent : TargetMessage<Leader, LostLeaderEvent>, IMessageEvent
        {
            public LostLeaderEvent(Leader target) : base(target) {}
        }

        /// <summary>
        /// Message posted localled when a follower has started following a leader.
        /// </summary>
        public class GainedLeaderEvent : TargetMessage<Leader, GainedLeaderEvent>, IMessageEvent
        {
            public GainedLeaderEvent(Leader target) : base(target) { }
        }

        /// <summary>
        /// Command to post when a leader should be assigned a follower.
        /// </summary>
        public class AssignLeaderCmd : TargetMessage<Leader, AssignLeaderCmd>, IMessageCommand
        {
            public AssignLeaderCmd(Leader target) : base(target) {}
        }

        public bool HasLeader { get { return _Leader != null; } }


        public Leader _Leader;
        public Leader Leader
        {
            get { return _Leader; }
            set
            {
                if (!isActiveAndEnabled) return;
                if (value == null)
                {
                    if (_Leader != null)
                    {
                        _Leader.RemoveFollower(this);
                        GlobalMessagePump.Instance.ForwardDispatch(gameObject, LostLeaderMsg.Change(_Leader) as LostLeaderEvent);
                    }
                }
                else
                {
                    if (_Leader != null) return; //can't accept new leader if old one still exists
                    if (value.gameObject == gameObject) return; //can't be our own leader
                    else if ((ValidLeaders.Length == 0 || HashedString.Contains(ValidLeaders, value.LeaderTag.Hash)) && _Leader != value)
                    {
                        value.AddFollower(this);
                        GlobalMessagePump.Instance.ForwardDispatch(gameObject, GainedLeaderMsg.Change(value) as GainedLeaderEvent);
                    }
                }

                _Leader = value;
            }
        }        
        
        void OnEnable()
        {
            Leader = null;
            DispatchRoot.AddLocalListener<AssignLeaderCmd>(HandleAssignLeader);
        }

        void OnDisable()
        {
            Leader = null;
            DispatchRoot.RemoveLocalListener<AssignLeaderCmd>(HandleAssignLeader);
        }

        void HandleAssignLeader(AssignLeaderCmd msg)
        {
            Leader = msg.Target;
        }
    }
}