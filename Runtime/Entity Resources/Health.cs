using UnityEngine;
using Peg.Game;
using System;
using UnityEngine.Events;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;
using Peg.Game.ConsumableResource;
using Peg.AutonomousEntities;
using Peg.MessageDispatcher;

namespace Peg.Game.ConsumableResource
{
    /// <summary>
    /// General-purpose health script.
    /// </summary>
    /// <remarks>
    /// It is important to note that this class caches many of the health messages.
    /// You should not hang on to the reference of any HealthChanged (or similar)
    /// messages after handling it as it may be volatile.
    /// 
    /// UPDATED 8/25/2017: This component now relies on FullInspector in order to render its editor correctly!
    ///                     Without it, the UnityEvents will not be available through the inspector.
    /// </remarks>
    [AddComponentMenu("Toolbox/Game/Building Blocks/Health")]
    [DisallowMultipleComponent]
    public class Health : MonoBehaviour, IEntityResource, IHealthProxy
    {
        //cached message for re-use
        //UPDATE 7/15: THESE WERE REMOVED DUE TO ERRORS
        //UPDATE 10/16: Added them back. The performance is nothing to scoff at when dealing with thousands of mobs!
        //          Just be sure to avoid hanging on to references of these messages.
        static HealthChangedEvent HealthMsg = new HealthChangedEvent(null, null, 0);
        static CausedHealthChangedEvent CausedHealthMsg = new CausedHealthChangedEvent(null, null, 0);
        static EntityDiedEvent DiedEvent = new EntityDiedEvent(null, null);
        static EntityRevivedEvent RevivedEvent = new EntityRevivedEvent(null);
        static KilledEntityEvent CausedKillEvent = new KilledEntityEvent(null, null, 0);
        static RevivedEntityEvent CausedReviveEvent = new RevivedEntityEvent(null, null);
        static HealthGainedEvent GainedEvent = new HealthGainedEvent(null, null, 0);
        static HealthLostEvent LostEvent = new HealthLostEvent(null, null, 0);

        public static readonly HashedString HealthResourceName = new HashedString("Health");
        public HashedString Name { get { return HealthResourceName; } }

        [HideInInspector]
        [SerializeField]
        bool _Godmode;
        [ShowInInspector]
        public bool Godmode
        {
            get => _Godmode;

            set
            {
                if (value != _Godmode)
                {
                    _Godmode = value;
                    if (value) OnInvincibilityStart.Invoke(this, CurrentHealth);
                    else OnInvincibilityEnd.Invoke(this, CurrentHealth);
                }
            } 
        }

        public Health HealthSource { get { return this; } }

        LocalMessageDispatch _DispatchRoot;
        public LocalMessageDispatch DispatchRoot
        {
            get
            {
                if (_DispatchRoot == null)
                {
                    _DispatchRoot = gameObject.FindComponentInEntity<LocalMessageDispatch>();
                    if (_DispatchRoot == null) throw new UnityException("The component '" + this.GetType().Name + "' attached to '" + gameObject.name + "' requires there to be a LocalMessageDispatch attached to its autonomous entity hierarchy.");
                }
                return _DispatchRoot;
            }
        }


        /// <summary>
        /// Gets or sets the current health.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        protected int _CurrentHealth;
        [PropertyTooltip("The current health. If it becomes zero or less, this component is flagged as dead.")]
        [ShowInInspector]
        public int CurrentHealth
        {
            get { return _CurrentHealth; }
            set 
            {
                //We must be sure we don't try to use the SetHealth() when not in play-mode in the editor.
                //It will try to create an instance of the message pump and badness will ensue.
                //UPDATE: This shouldn't be a problem any more but it needs testing
                #if UNITY_EDITOR
                if (Application.isPlaying) SetHealth(null, value);
                else _CurrentHealth = value;
                #else
                SetHealth(null, value);
                #endif
            }
        }

        /// <summary>
        /// Gets or sets the current health as a percentage expressed between 0.0 and 1.0
        /// </summary>
        /// BUG: Exposing this field has caused severe bugs when running in the editor with this inspector open
        //[Inspectable("Current health expressed as a percentage. If it becomes zero or less, this component is flagged as dead.")]
        public float CurrentPercent
        {
            get { return (float)_CurrentHealth / (float)_MaxHealth; }
            set { CurrentHealth = Mathf.CeilToInt(value * (float)_MaxHealth); }
        }

        [SerializeField]
        [HideInInspector]
        protected int _MaxHealth;
        /// <summary>
        /// Maximum Health. It will not allow itself to be set to a value smaller than 1.
        /// </summary>
        [PropertyTooltip("The maximum health that can be obtained.")]
        [ShowInInspector]
        public int MaxHealth
        {
            get { return _MaxHealth; }
            set
            {
                _MaxHealth = Mathf.Max(value, 1);
                if (EnforceMaxHealth) _CurrentHealth = Mathf.Min(_CurrentHealth, _MaxHealth);
            }
        }

        /// <summary>
        /// Casts the current health value to a float.
        /// </summary>
        public float Current
        {
            get { return CurrentHealth; }
            set { CurrentHealth = (int)value; }
        }

        /// <summary>
        /// Casts the max health value to a float.
        /// </summary>
        public float Max
        {
            get { return MaxHealth; }
            set
            {
                MaxHealth = Mathf.Max((int)value, 1);
                if(EnforceMaxHealth) _CurrentHealth = Mathf.Min(_CurrentHealth, _MaxHealth);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if this entity is dead due to current health being at or below zero, <c>false</c> otherwise.
        /// It also returns <c>true</c> if the GameObject it is attached to is not active. Note that this behaviour's 'enabled'
        /// state affects nothing.
        /// </summary>
        [Sirenix.OdinInspector.ShowInInspector]
        [Sirenix.OdinInspector.ReadOnly]
        public bool IsDead
        {
            get
            { 
                return (_CurrentHealth <= 0 || !gameObject.activeInHierarchy || !gameObject.activeSelf);
            }
        }

        [Tooltip("If true, health will always be capped at maximum value. Otherwise, current health can go over max health.")]
        public bool EnforceMaxHealth = true;

        [Tooltip("Can be used to supress messages posted locally about health events.")]
        public bool SuppressLocalMessages = false;

        [Tooltip("Can be used to supress messages posted globally about health events.")]
        public bool SuppressGlobalMessages = false;

        [Tooltip("If set, the UnityEvents seen in this inspector will be triggered. This is meant to be a performance-critical option when posting to the GMP is prohibitive (e.g. projectiles with health scripts).")]
        public bool PostUnityEvents = false;

        [Tooltip("If set, disabling this component will also disable its message handlers for receiving damage.")]
        public bool CanDisableDamageHandlers = true;

        bool HandlersEnabled;


        [Sirenix.OdinInspector.ShowIf("PostUnityEvents")]
        public HealthEvent OnHealthChanged;

        [Sirenix.OdinInspector.ShowIf("PostUnityEvents")]
        public HealthEvent OnDied;

        [Sirenix.OdinInspector.ShowIf("PostUnityEvents")]
        public HealthEvent OnRevived;

        [Sirenix.OdinInspector.ShowIf("PostUnityEvents")]
        public HealthEvent OnInvincibilityStart;

        [Sirenix.OdinInspector.ShowIf("PostUnityEvents")]
        public HealthEvent OnInvincibilityEnd;


        [Serializable]
        public class HealthEvent : UnityEvent<Health, int> { }

        protected virtual void Awake()
        {
            DispatchRoot.AddLocalListener<DemandHealthComponent>(OnDemandedMe);
            DispatchRoot.AddLocalListener<ReviveEntityCmd>(HandleReviveCmd);
            DispatchRoot.AddLocalListener<KillEntityForcedCmd>(HandleForcedKillCmd);

            if (!CanDisableDamageHandlers)
            {
                DispatchRoot.AddLocalListener<ChangeHealthCmd>(HandleAffectHealth);
                DispatchRoot.AddLocalListener<KillEntityCmd>(HandleKillCmd);
                HandlersEnabled = true;
            }
        }

        protected virtual void OnDestroy()
        {
            if(HandlersEnabled)
            {
                DispatchRoot.RemoveLocalListener<ChangeHealthCmd>(HandleAffectHealth);
                DispatchRoot.RemoveLocalListener<KillEntityCmd>(HandleKillCmd);
            }
            DispatchRoot.RemoveLocalListener<ReviveEntityCmd>(HandleReviveCmd);
            DispatchRoot.RemoveLocalListener<DemandHealthComponent>(OnDemandedMe);
            DispatchRoot.RemoveLocalListener<KillEntityForcedCmd>(HandleForcedKillCmd);
            _DispatchRoot = null;
        }

        void OnDemandedMe(DemandHealthComponent msg)
        {
            msg.Respond(this);
        }

        protected virtual void OnEnable()
        {
            CancelInvincibility();

            if (CanDisableDamageHandlers && !HandlersEnabled)
            {
                DispatchRoot.AddLocalListener<ChangeHealthCmd>(HandleAffectHealth);
                DispatchRoot.AddLocalListener<KillEntityCmd>(HandleKillCmd);
                HandlersEnabled = true;
            }

            if(EnforceMaxHealth && CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
        }

        protected virtual void OnDisable()
        {
            if (CanDisableDamageHandlers && HandlersEnabled)
            {
                DispatchRoot.RemoveLocalListener<ChangeHealthCmd>(HandleAffectHealth);
                DispatchRoot.RemoveLocalListener<KillEntityCmd>(HandleKillCmd);
                HandlersEnabled = false;
            }
            StopAllCoroutines();
            InvincibleTimer = 0;
            InvincibilityRoutine = null;
            Godmode = false;
        }
        
        
        Coroutine InvincibilityRoutine;
        double InvincibleTimer;
        /// <summary>
        /// Makes this health object un-able to receive damage for a given amount of time.
        /// Multiple overlapping calls to this method will cause it to stack.
        /// </summary>
        /// <param name="time"></param>
        public void ProcInvincibility(float time)
        {
            if (InvincibilityRoutine == null)
            {
                InvincibleTimer = time;
                InvincibilityRoutine = StartCoroutine(InvincibleCountdown());
            }
            else InvincibleTimer += time;
        }

        /// <summary>
        /// Makes this health object un-able to receive damage for a given amount of time.
        /// Multiple overlapping calls to this method will overwite the previous time.
        /// </summary>
        /// <param name="time"></param>
        public void OverwriteInvincibility(float time)
        {
            if(InvincibilityRoutine != null)
                StopCoroutine(InvincibilityRoutine);

            InvincibleTimer = time;
            InvincibilityRoutine = StartCoroutine(InvincibleCountdown());
        }

        double LastInvincibleStart;
        /// <summary>
        /// Makes this health object un-able to receive damage for a given amount of time.
        /// Multiple overlapping calls to this method will overwite the previous time.
        /// </summary>
        /// <param name="time"></param>
        public void UnionInvincibility(float time)
        {
            double currTime = 0;

            if (InvincibilityRoutine != null)
            {
                StopCoroutine(InvincibilityRoutine);
                currTime = (Time.timeAsDouble - LastInvincibleStart) - InvincibleTimer;
            }

            InvincibleTimer = Math.Max(time, currTime);
            InvincibilityRoutine = StartCoroutine(InvincibleCountdown());
        }

        /// <summary>
        /// Cancels a previousl proced invincible state.
        /// </summary>
        public void CancelInvincibility()
        {
            if (InvincibilityRoutine != null)
            {
                StopCoroutine(InvincibilityRoutine);
                InvincibleTimer = 0;
                Godmode = false;// OldGodmode;
                InvincibilityRoutine = null;
            }
        }

        bool OldGodmode;
        IEnumerator InvincibleCountdown()
        {
            OldGodmode = Godmode;
            Godmode = true;

            var invincibleStart = Time.timeAsDouble;
            LastInvincibleStart = invincibleStart;
            while (Time.timeAsDouble - invincibleStart < InvincibleTimer)
                yield return null;

            InvincibleTimer = 0;
            Godmode = false;// OldGodmode;
            InvincibilityRoutine = null;
        }

        /// <summary>
        /// Reduces this health component to zero and ignores god status
        /// </summary>
        public void ForceKill()
        {
            CancelInvincibility();
            bool oldGod = Godmode;
            Godmode = false;
            Current = 0;
            Godmode = oldGod;
        }

        /// <summary>
        /// Handles incoming requests to change our health.
        /// </summary>
        /// <param name="message"></param>
        void HandleAffectHealth(ChangeHealthCmd message)
        {
            SetHealth(message.Agent, _CurrentHealth - message.Change, false, !message.HonorInvincibility);
        }

        /// <summary>
        /// Handles incoming requests to become alive.
        /// </summary>
        /// <param name="msg"></param>
        void HandleReviveCmd(ReviveEntityCmd msg)
        {
            var myRoot = gameObject.GetEntityRoot();
            if (msg.Target == gameObject || msg.Target == myRoot || msg.Target.GetEntityRoot() == myRoot)
            {
                if (_CurrentHealth <= 0) _CurrentHealth = 1;
                //TODO: probably want to post a 'restore health' message here
                var rev = Health.RevivedEvent.ChangeValues(null, this);
                GlobalMessagePump.Instance.PostMessage(rev); //tell the world
                GlobalMessagePump.Instance.ForwardDispatch(gameObject, rev);//tell me
            }
        }

        /// <summary>
        /// Handle incoming requests to insta-die.
        /// </summary>
        /// <param name="msg"></param>
        void HandleKillCmd(KillEntityCmd msg)
        {
            CurrentHealth = 0;
        }

        /// <summary>
        /// Handle incoming requests to insta-die.
        /// </summary>
        /// <param name="msg"></param>
        void HandleForcedKillCmd(KillEntityForcedCmd msg)
        {
            #if UNITY_EDITOR
            if (Application.isPlaying) SetHealth(msg.Agent, 0, false, true);
            else _CurrentHealth = 0;
            #else
            SetHealth(msg.Agent, 0, false, true);
            #endif
        }

        /// <summary>
        /// Helper for posting revive events and messages.
        /// </summary>
        /// <param name="msg"></param>
        protected void PostRevive(GameObject agent, int diff)
        {
            var revivedMsg = Health.RevivedEvent.ChangeValues(agent, this);

            if (!SuppressGlobalMessages) GlobalMessagePump.Instance.PostMessage(revivedMsg); //tell the world
            if (!SuppressLocalMessages)
            {
                GlobalMessagePump.Instance.ForwardDispatch(gameObject, revivedMsg);//tell me
                if (agent != null) GlobalMessagePump.Instance.ForwardDispatch(agent, Health.CausedReviveEvent.ChangeValues(agent, this)); //tell the reviver
            }

            if (PostUnityEvents)
                OnRevived.Invoke(this, diff);
        }

        /// <summary>
        /// Helper for posting died events and messages.
        /// </summary>
        /// <param name="diedMsg"></param>
        /// <param name="agent"></param>
        protected void PostDied(GameObject agent, int diff)
        {
            var diedMsg = Health.DiedEvent.ChangeValues(agent, this);

            if (!SuppressGlobalMessages) GlobalMessagePump.Instance.PostMessage(diedMsg); //tell the world
            if (!SuppressLocalMessages)
            {
                GlobalMessagePump.Instance.ForwardDispatch(gameObject, diedMsg);//tell me
                if (agent != null) GlobalMessagePump.Instance.ForwardDispatch(agent, Health.CausedKillEvent.ChangeValues(agent, this, diff)); //tell the killer
            }

            if (PostUnityEvents)
                OnDied.Invoke(this, diff);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="changedMsg"></param>
        /// <param name="agent"></param>
        /// <param name="diff"></param>
        protected void PostChanged(GameObject agent, int diff)
        {
            var changedMsg = Health.HealthMsg.ChangeValues(agent, this, diff);

            if (!SuppressLocalMessages)
            {
                if (diff > 0) GlobalMessagePump.Instance.ForwardDispatch(gameObject, LostEvent.ChangeValues(agent, this, diff));
                else if (diff < 0) GlobalMessagePump.Instance.ForwardDispatch(gameObject, GainedEvent.ChangeValues(agent, this, diff));
                GlobalMessagePump.Instance.ForwardDispatch(gameObject, changedMsg);//tell me
                if (agent != null) GlobalMessagePump.Instance.ForwardDispatch(agent, Health.CausedHealthMsg.ChangeValues(agent, this, diff));//tell the one that caused this to happen
            }
            if (!SuppressGlobalMessages) GlobalMessagePump.Instance.PostMessage(changedMsg);//tell the world

            if (PostUnityEvents)
                OnHealthChanged.Invoke(this, diff);
        }

        /// <summary>
        /// Sets this object's current health while marking 'agent' as responsible for the change.
        /// <br />
        /// Calling this function will post a HealthChangedEvent/HealthDamageAbsorbedEvent both globally as well as locally to both
        /// the agent and the target of the health change unless suppressEvents is set to <c>true</c>.
        /// </summary>
        /// <param name="agent">The entity that caused this change in health to occur.</param>
        /// <param name="hp">The new value of current health.</param>
        public virtual void SetHealth(GameObject agent, int hp, bool suppressEvents = false, bool ignoreGodmode = false)
        {
            if (!ignoreGodmode && Godmode && !IsDead)
            {
                HealthDamageAbsorbedEvent.Shared.ChangeValues(agent, this, _CurrentHealth - hp);
                if (!SuppressLocalMessages)
                {
                    GlobalMessagePump.Instance.ForwardDispatch(gameObject, HealthDamageAbsorbedEvent.Shared); //tell me
                    if (agent != null) GlobalMessagePump.Instance.ForwardDispatch(agent, HealthDamageAbsorbedByOtherEvent.Shared); //tell the one that did this
                }
                if (!SuppressGlobalMessages) GlobalMessagePump.Instance.PostMessage(HealthDamageAbsorbedEvent.Shared); //tell the world

                return;
            }


            bool wasDead = _CurrentHealth <= 0;

            if (EnforceMaxHealth && hp > MaxHealth)
                hp = MaxHealth;

            int diff = _CurrentHealth - hp;
            _CurrentHealth = hp;

            if (suppressEvents) return;

#if UNITY_EDITOR
            if(Application.isEditor && !Application.isPlaying) return;
#endif
            

            if (!wasDead && _CurrentHealth <= 0)
                PostDied(agent, diff);
            if(wasDead && _CurrentHealth > 0)
                PostRevive(agent, diff);

            //post this *after* revive/death events that way we can catch and cancel this if needed by disabling any listeners
            PostChanged(agent, diff);

        }

        /// <summary>
        /// Returns true if this entity has a health component attached to it.
        /// Note: The entity does not have to be currently 'alive' for this to return true,
        /// only that is is capabile of stracking damage using a Health component.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static bool IsLivingEntity(EntityRoot root)
        {
            Assert.IsNotNull(root);
            return root.FindComponentInEntity<Health>(true) != null;
        }

        /// <summary>
        /// Returns true if this entity has a health component attached to it.
        /// Note: The entity does not have to be currently 'alive' for this to return true,
        /// only that is is capabile of stracking damage using a Health component.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static bool IsLivingEntity(GameObject gameObject)
        {
            Assert.IsNotNull(gameObject);
            return gameObject.FindComponentInEntity<Health>(true) != null;
        }

        /// <summary>
        /// Returns true if this entity has a health component and is currently not dead.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static bool IsAlive(EntityRoot root)
        {
            Assert.IsNotNull(root);
            var hp = root.FindComponentInEntity<Health>(true);
            if (hp != null)
                return !hp.IsDead;
            return false;
        }

        /// <summary>
        /// Returns the attached health component if this entity has a health component and is currently not dead and
        /// does not have godmode set to true.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static Health IsKillable(EntityRoot root)
        {
            Assert.IsNotNull(root);
            var hp = root.FindComponentInEntity<Health>(true);
            if (hp != null && !hp.Godmode)
                return hp;
            return null;
        }

        /// <summary>
        /// Returns true if this entity has a health component and is currently not dead.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static bool IsAlive(GameObject gameObject)
        {
            Assert.IsNotNull(gameObject);
            var hp = gameObject.FindComponentInEntity<Health>(true);
            if (hp != null)
                return !hp.IsDead;
            return false;
        }

    }
}
