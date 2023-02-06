using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System;

namespace Toolbox.Game
{
    /// <summary>
    /// Detects when a NavMeshAgent has stopped either due to reaching it's destination or to having an incomplete path.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class DetectAgentStop : MonoBehaviour
    {
        public float StallTime = 3;
        
        [FoldoutGroup("Events", 1)]
        public NavAgentEvent OnStopped;
        [FoldoutGroup("Events", 1)]
        public NavAgentEvent OnDestReached;
        [FoldoutGroup("Events", 1)]
        public NavAgentEvent OnIncompleteDestReached;
        [FoldoutGroup("Events", 1)]
        public NavAgentEvent OnDestInvalid;
        [FoldoutGroup("Events", 1)]
        public NavAgentEvent OnPathIncomplete;
        [FoldoutGroup("Events", 1)]
        public NavAgentEvent OnStalled;

        [Serializable]
        public class NavAgentEvent : UnityEvent<NavMeshAgent> { }

        /// <summary>
        /// Set this when setting the agent destination.
        /// </summary>
        public bool StartMoving { get; set; }

        NavMeshAgent Agent;
        float StartTimer;
        bool MovementDetected;


        #region Flags
        [HideInInspector]
        [SerializeField]
        byte Flags;

        public enum DetectionFlags
        {
            DestReached             = 1 << 0,
            IncompleteDestReached   = 1 << 1,
            DestInvalid             = 1 << 2,
            PathIncomplete          = 1 << 3,
            Stall                   = 1 << 4,
        }

        [BoxGroup("Flags", Order = 0)]
        [ShowInInspector]
        public bool DestReached
        {
            get { return (Flags & (byte)DetectionFlags.DestReached) != 0; }
            set
            {
                if (value) Flags |= (byte)DetectionFlags.DestReached;
                else Flags &= ((byte)DetectionFlags.DestReached ^ 0xff);
            }
        }

        [BoxGroup("Flags", Order = 0)]
        [ShowInInspector]
        public bool IncompleteDestReached
        {
            get { return (Flags & (byte)DetectionFlags.IncompleteDestReached) != 0; }
            set
            {
                if (value) Flags |= (byte)DetectionFlags.IncompleteDestReached;
                else Flags &= ((byte)DetectionFlags.IncompleteDestReached ^ 0xff);
            }
        }

        [BoxGroup("Flags", Order = 0)]
        [ShowInInspector]
        public bool DestInvalid
        {
            get { return (Flags & (byte)DetectionFlags.DestInvalid) != 0; }
            set
            {
                if (value) Flags |= (byte)DetectionFlags.DestInvalid;
                else Flags &= ((byte)DetectionFlags.DestInvalid ^ 0xff);
            }
        }

        [BoxGroup("Flags", Order = 0)]
        [ShowInInspector]
        public bool PathIncomplete
        {
            get { return (Flags & (byte)DetectionFlags.PathIncomplete) != 0; }
            set
            {
                if (value) Flags |= (byte)DetectionFlags.PathIncomplete;
                else Flags &= ((byte)DetectionFlags.PathIncomplete ^ 0xff);
            }
        }

        [BoxGroup("Flags", Order = 0)]
        [ShowInInspector]
        public bool Stall
        {
            get { return (Flags & (byte)DetectionFlags.Stall) != 0; }
            set
            {
                if (value) Flags |= (byte)DetectionFlags.Stall;
                else Flags &= ((byte)DetectionFlags.Stall ^ 0xff);
            }
        }
        #endregion


        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (!StartMoving)
                return;

            //first, try to detect when we have actually started following a path
            if (!MovementDetected)
            {
                //still calculating
                if (Agent.pathPending)
                    return;
                else
                {
                    //invalid paths
                    if (Agent.pathStatus == NavMeshPathStatus.PathInvalid && DestInvalid)
                    {
                        FullStop();
                        OnDestInvalid.Invoke(Agent);
                        return;
                    }
                    if (Agent.pathStatus == NavMeshPathStatus.PathPartial && PathIncomplete)
                    {
                        FullStop();
                        OnPathIncomplete.Invoke(Agent);
                        return;
                    }
                }

                //we know we have a path now
                MovementDetected = true;
                StartTimer = 0;
                return;
            }
            else
            {
                //trying to move are are disabled or no on a navmesh, so stop
                if(!Agent.isActiveAndEnabled || !Agent.isOnNavMesh)
                {
                    FullStop();
                    return;
                }

                //we probably reached our dest
                if (DestReached && Agent.remainingDistance < Agent.stoppingDistance && !float.IsInfinity(Agent.remainingDistance))
                {
                    FullStop();
                    OnDestReached.Invoke(Agent);
                    return;
                }

                //are we trying to move but can't because something is stopping us
                if(Stall && !Agent.isStopped && Agent.velocity.sqrMagnitude < float.Epsilon)
                {
                    if(StartTimer == 0)
                    {
                        //try again in a bit
                        StartTimer = Time.time;
                        return;
                    }
                    else if(Time.time - StartTimer > StallTime)
                    {
                        FullStop();
                        OnStalled.Invoke(Agent);
                        return;
                    }
                }

                //TODO: how to detect a partial path that we've traveled as far as we can?
                //if (IncompleteDestReached)
                //{

                //}
            }
        }

        /// <summary>
        /// Resets all internal motion flags and triggers the OnStop event.
        /// </summary>
        public void FullStop()
        {
            StartTimer = 0;
            StartMoving = false;
            MovementDetected = false;
            //this little fucker causes issues with off-mesh links
            //Agent.ResetPath();
            OnStopped.Invoke(Agent);
        }
    }
}
