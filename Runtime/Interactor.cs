using System;
using System.Collections.Generic;
using Peg.Game;
using Peg.GCCI;
using Peg.Messaging;
using UnityEngine;
using UnityEngine.Events;


namespace Peg.Game
{
    /// <summary>
    /// Base class for all users of interactables. This should be paired appropriately for each sub-class of interactable.
    /// i.e Attacker for Attackables, Grabber for Pickups, etc. This class requires a character motor of some kind to allow movement.
    /// </summary>
    public abstract class Interactor : LocalListenerMonoBehaviour
    {

        public enum Use
        {
            Success = 0, //The only success flag. All others are failure
            NotReady,
            OutOfRange,
            Incompatible,
            InUseByAnother,
            NoLoS,
        }

        [Header("Interactor")]
        [Tooltip("If true, this interactor can use targets derived from its valid Target Type.")]
        [SerializeField]
        bool _CanUseSubclasses = false;
        public virtual bool CanUseSubclasses { get { return _CanUseSubclasses; } protected set { _CanUseSubclasses = value; } }

        [Tooltip("If set, this interactor will ignore the Reuse Delay setting of its target interactable.")]
        public bool IgnoreTargetDelay = false;

        [Space(10)]
        [Tooltip("The maximum range of effect for this interactor. It must overlap with the target interactable's radius.")]
        public float MaxRange = 1.0f;

        [Tooltip("The minimum range of effect for this interactor. It must not overlap with the target interactable's radius.")]
        public float MinRange = 0.0f;

        [Space(10)]
        [Tooltip("If set, the interaction requires line-of-sight between the center of the agent and target in order to work.")]
        public bool RequiresLos = false;

        [Tooltip("The collision mask to use when testing LoS.")]
        public LayerMask LosMask;

        [Tooltip("If checking for LoS, set this to use 2D physics raycasting instead of 3D.")]
        public bool Raycast2d = false;

        /// <summary>
        /// The entity root of this interactor.
        /// </summary>
        public EntityRoot Entity { get; protected set; }

        public Vector3 Position
        {
            get { return gameObject.transform.position; }
        }

        public Type UseableType { get; protected set; }

#if UNITY_EDITOR
        public Color GizmoColor = Color.yellow;

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = GizmoColor;
            Gizmos.DrawWireSphere(transform.position, MaxRange);
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        protected virtual void Awake()
        {
            UseableType = DeclaredUseable();
            DispatchRoot.AddLocalListener<InteractCmd>(HandleInteractCommand);
            
            Entity = gameObject.GetEntityRoot();
            if(Entity == null) Debug.LogWarning("Many features may break if " + this.GetType().Name + " is not part of an Autonomous Entity Hierarchy.");
        }

        protected override void OnDestroy()
        {
            DispatchRoot.RemoveLocalListener<InteractCmd>(HandleInteractCommand);
            base.OnDestroy();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        protected virtual void HandleInteractCommand(InteractCmd cmd)
        {
            if(cmd.Agent != this) return; //this one's not for us

            if (cmd.Validate) InteractWith(cmd.Target, null);
            else InteractWithNonValidate(cmd.Target);
        }

        /// <summary>
        /// Attempts to make this interactor use the supplied interactable.
        /// </summary>
        /// <param name="thing"></param>
        public Use InteractWith(IInteractable target, UnityAction<Use, IInteractable> callback)
        {
            Use use = CanInteract(target);
            if (use != Use.Success) return use;
            //We forward a message to the interactable base class to let it know it has been used.
            //This is prefered over calling public methods that really should stay hidden.
            //BUG ALERT:message doesn't work when we use two-way subclassing since the handler doesn't check for it properly
            //GlobalMessagePump.ForwardDispatch<InteractableUseEvent>(target.gameObject, new InteractableUseEvent(this));
            target.Callback_ConsumeUse();
            target.OnInteract(this); //just call it directly
            Interaction(target);
            if(callback != null) callback(use, target);
            return use;
        }

        /// <summary>
        /// Similar to <see cref="InteractWith"/> but without validating distances and readiness.
        /// This simply performs the interaction regardless of validity.
        /// </summary>
        /// <param name="target"></param>
        public void InteractWithNonValidate(IInteractable target)
        {
            Interaction(target);
            //We forward a message to the interactable base class to let it know it has been used.
            //This is prefered over calling public methods that really should stay hidden.
            GlobalMessagePump.Instance.ForwardDispatch<InteractableUseEvent>(target.gameObject, new InteractableUseEvent(this));
        }

        /// <summary>
        /// Returns true if this interactor is within using range of the given interactable.
        /// This will check both the max allowed range and the min required range.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool WithinInteractionRange(IInteractable target, float fudge = 0.1f)
        {
            if (Peg.TypeHelper.IsReferenceNull(target)) return false;

            float d = Vector3.Distance(transform.position, target.ActivationPosition);
            float maxAllowed = MaxRange + target.MaxRange + fudge;
            float minRequired = Mathf.Max(MinRange, target.MinRange);
            return (d <= maxAllowed && (d >= minRequired || minRequired <= 0));
        }
       
        /// <summary>
        /// Returns true if this interactor can use the target interactable. Does not validate range or LoS.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsValidInteraction(IInteractable target)
        {
            //early-outs
            if (TypeHelper.IsReferenceNull(target)) return false;
            if (!isActiveAndEnabled || !target.isActiveAndEnabled) return false;
            if (target.gameObject == this.gameObject && !target.CanUseSelf) return false;

            Type userType = GetType();
            Type useableType = target.GetType();

            
            int flag = Convert.ToInt32(target.UseableBySubclasses) | (Convert.ToInt32(CanUseSubclasses) << 1);
            if(flag == 0)
            {
                //sub-classes not supported, just check types
                if (UseableType == useableType && target.UserType == userType) return true;
            }
            else if(flag == 1)
            {
                //target can accept co-varient 'userType'
                if (UseableType == useableType && TypeHelper.IsSameOrSubclass(target.UserType, userType)) return true;
            }
            else if(flag == 2)
            {
                //user can accept co-varient 'targetType'
                if (target.UserType == userType && TypeHelper.IsSameOrSubclass(UseableType, useableType)) return true;
            }
            else if(flag == 3)
            {
                //both can accept co-varient types
                if (TypeHelper.IsSameOrSubclass(UseableType, useableType) && TypeHelper.IsSameOrSubclass(target.UserType, userType)) return true;
            }

            return false;
        }

        /// <summary>
        /// Tests if a given interaction would be feasable without actually performing the interaction.
        /// </summary>
        /// <param name="target"></param>
        /// <returns>Returns Use.Success if feasable. Other return values reflect the reason why it isn't.</returns>
        public virtual Use CanInteract(IInteractable target)
        {
            if (!IsValidInteraction(target)) return Use.Incompatible;

            if (MaxRange + target.MaxRange > Vector3.Distance(transform.position, target.ActivationPosition))
            {
                if (IgnoreTargetDelay || target.ReadyForUse)
                {
                    if (!RequiresLos || (RequiresLos && HasLoS(target, LosMask, Raycast2d)))
                        return Use.Success;
                    else return Use.NoLoS;
                }
                else return Use.NotReady;
            }
            else return Use.OutOfRange;
        }
        
        /// <summary>
        /// Checks for Line-of-Sight between interactor and interactable.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="mask"></param>
        /// <param name="raycast2D"></param>
        /// <returns><c>true</c> if there is LoS between this interactor and the interactable, <c>false</c> otherwise.</returns>
        public bool HasLoS(IInteractable target, LayerMask mask, bool raycast2D)
        {
            if (raycast2D)
                return (Physics2D.Linecast(this.Position, target.ActivationPosition, mask.value).collider == null);
            else
                return !Physics.Linecast(this.Position, target.ActivationPosition, mask.value);
        }


        //TODO: This stuff is all quite bothersome and large.
        //We should wrap it in a PoD so that we can simply hang on to
        //a single pointer which will be much smaller when not in use.
        bool Pending = false;
        IInteractable CachedUseable;
        UnityAction<IPathAgentAdaptor, bool> StoppedMovingCallback;
        bool ConfirmActivationRange;
        bool RequireLos;
        LayerMask LosBlockers;
        bool UseRaycast2d;
        bool Repath;
        IPathAgentAdaptor Agent;

        public enum SequenceStartStatus
        {
            Failed,
            Finished,
            Pending,
        }

        /// <summary>
        /// Attempts to interact with a target interactable. If the range is too far it will make use of any avilable
        /// path agent adaptors on its AEH to move within range before attempting.
        /// </summary>
        /// <param name="target"></param>
        public SequenceStartStatus BeginInteractionSequence(IInteractable target, bool confirmActivationRange, bool requireLos, LayerMask losBlockers, bool useRaycast2d,bool repath, UnityAction<IPathAgentAdaptor, bool> stoppedCallback, IPathAgentAdaptor optionalAgent = null)
        {
            optionalAgent ??= Entity.FindComponentInEntity<IPathAgentAdaptor>();
            if (optionalAgent == null) return SequenceStartStatus.Failed;

            CancelInteractionSequence();

            //need to shrink interaction range a little to ensure enough time when interacting with moving targets
            float withinRange = (target.MaxRange + MaxRange) * 0.9f;

            if (!IsValidInteraction(target))
                return SequenceStartStatus.Failed;
            if (WithinInteractionRange(target))
            {
                //short circuit the whole thing
                if (!target.ReadyForUse) return SequenceStartStatus.Failed;
                if (requireLos && !HasLoS(target, losBlockers, useRaycast2d)) return SequenceStartStatus.Failed;
                InteractWithNonValidate(target);
                return SequenceStartStatus.Finished;
            }

            //cache all info we'll need for the callback
            Pending = true;
            CachedUseable = target;
            ConfirmActivationRange = confirmActivationRange;
            RequireLos = requireLos;
            LosBlockers = losBlockers;
            UseRaycast2d = useRaycast2d;
            Repath = repath;
            Agent = optionalAgent;

            //begin running
            optionalAgent.AddOnStoppedCallback(EndInteractionSequence);
            StoppedMovingCallback = stoppedCallback;
            if (!optionalAgent.FollowTarget(target.gameObject.transform, withinRange)) //add some fudge to ensure it meets the range requirements
            {
                EndInteractionSequence(optionalAgent);
                return SequenceStartStatus.Finished;
            }
            
            
            return SequenceStartStatus.Pending;
        }

        /// <summary>
        /// Cancels a previously started interaction sequence.
        /// </summary>
        public void CancelInteractionSequence()
        {
            if (Pending)
            {
                Pending = false;
                StoppedMovingCallback = null;
                Agent.RemoveOnStoppedCallback(EndInteractionSequence);
            }
        }

        /// <summary>
        /// Internal handler for stopping an perviously called 'BeginInteractionSequence' call.
        /// </summary>
        /// <param name="agent"></param>
        void EndInteractionSequence(IPathAgentAdaptor agent)
        {
            if (StoppedMovingCallback == null) Debug.LogError("StoppedMovingCallback is null.");
            Pending = false;
            Agent.RemoveOnStoppedCallback(EndInteractionSequence);

            //we need to check for null because the target may have been destroyed since we started this sequence.
            if (TypeHelper.IsReferenceNull(CachedUseable))
            {
                if (StoppedMovingCallback != null) StoppedMovingCallback.Invoke(Agent, true);
                return;
            }

            //check range, readiness, and LoS
            if (!CachedUseable.ReadyForUse || CachedUseable == null)
            {
                if (StoppedMovingCallback != null) StoppedMovingCallback.Invoke(agent, false);
                return;
            }

            if (ConfirmActivationRange && !WithinInteractionRange(CachedUseable))
            {
                if (Repath)
                {
                    //we are out of range but were flagged as having reached our goal. If we have LoS, try pathing again
                    if (RequireLos && !HasLoS(CachedUseable, LosBlockers, UseRaycast2d))
                    {
                        if (StoppedMovingCallback != null) StoppedMovingCallback.Invoke(Agent, false);
                        return;
                    }
                    float withinRange = (CachedUseable.MaxRange + MaxRange) * 0.9f;
                    Agent.FollowTarget(CachedUseable.gameObject.transform, withinRange);
                    Agent.AddOnStoppedCallback(EndInteractionSequence);
                    //still running
                    return;
                }
                else
                {
                    if (StoppedMovingCallback != null) StoppedMovingCallback.Invoke(Agent, false);
                    return;
                }
            }
            //can't interact if we don't have LoS and we require it
            if (RequireLos && !HasLoS(CachedUseable, LosBlockers, UseRaycast2d))
            {
                if (StoppedMovingCallback != null) StoppedMovingCallback.Invoke(Agent, false);
                return;
            }

            //reached our dest
            InteractWithNonValidate(CachedUseable);
            if(StoppedMovingCallback != null) StoppedMovingCallback.Invoke(Agent, true);
        }

        /// <summary>
        /// Implemented in derived classes so that this system knows what type it can use.
        /// </summary>
        /// <returns></returns>
        protected abstract Type DeclaredUseable();

        /// <summary>
        /// Override in base class for specific functionality.
        /// </summary>
        /// <param name="interactable"></param>
        protected abstract void Interaction(IInteractable interactable);
        
        /// <summary>
        /// Compares a list of interactors and interactables and attempts to find the highest-priority
        /// interaction amongst all of them. Note that this does not perform validation beyond compatiblilty
        /// checking, so things like Line-of-Sight, cooldown periods, range, etc. will not be taken into consideration.
        /// </summary>
        /// <param name="users">A list of potential interactors to be considered.</param>
        /// <param name="targets">A list of potential interactables to be considered.</param>
        /// <param name="priorityUser">The resulting interactor found of null if none.</param>
        /// <param name="priorityUseable">The resulting interactable found or null if none.</param>
        /// <returns></returns>
        public static bool FindCompatibleInteraction(List<Interactor> users, List<IInteractable> useables, out Interactor priorityUser, out IInteractable priorityUseable)
        {
            Interactor user;
            IInteractable useable;
            priorityUser = null;
            priorityUseable = null;

            //yuckers, gonna have to try every possible combo to find one that is a)valid and b)has the highest priority
            for (int i = 0; i < useables.Count; i++)
            {
                for (int j = 0; j < users.Count; j++)
                {
                    user = users[j];
                    useable = useables[i];

                    //early-outs
                    if (user == null || useable == null) continue;
                    if (user.CanInteract(useable) == Interactor.Use.Incompatible) continue;
                    if (user.gameObject == useable.gameObject && !useable.CanUseSelf) continue;

                    //remember, lower value means higher priority!
                    //TODO: consider: should self-use be highest priority? Lowest priority? Processed as normal (currently what it is)?
                    if (priorityUseable == null || useable.Priority < priorityUseable.Priority)
                    {
                        priorityUser = user; 
                        priorityUseable = useable;
                    }
                }
            }

            if (priorityUseable == null || priorityUser == null) return false;
            return true;
        }
        
    }
}


namespace Peg
{
    /// <summary>
    /// Command to invoke an interaction.
    /// </summary>
    public class InteractCmd : AgentTargetMessage<Interactor, IInteractable, InteractCmd>, IMessageCommand
    {
        public bool Validate { get; protected set; }

        public InteractCmd() : base() { }
        public InteractCmd(Interactor agent, IInteractable target, bool validate)
            : base(agent, target)
        {
            Validate = validate;
        }
    }
}