using UnityEngine;
using System;
using Toolbox.Game;
using Toolbox;
using Toolbox.Messaging;
using Toolbox.Collections;

namespace Toolbox.Game
{
    /// <summary>
    /// General-purpose entity flags.
    /// </summary>
    [AddComponentMenu("Toolbox/Game/Building Blocks/Flags (Name-to-Bool)")]
    [Serializable]
    public sealed class Flags : LocalListenerMonoBehaviour
    {
        [Tooltip("Name used to identify the purpose of this object's flags (actions ready, quests completed, combat statuses, etc...)")]
        public string Name = "Flags";

        /// <summary>
        /// Specialized concrete, non-generic dictionary that maps strings to bool.
        /// Used for storing game-specific entity statistics.
        /// </summary>
        public StringBoolHashMap Mapping = new();


        void Awake()
        {
            DispatchRoot.AddLocalListener<DemandFlagsComponent>(OnDemandedMe);
        }

        protected override void OnDestroy()
        {
            DispatchRoot.RemoveLocalListener<DemandFlagsComponent>(OnDemandedMe);
            base.OnDestroy();
        }

        void OnDemandedMe(DemandFlagsComponent msg)
        {
            msg.Respond(this);
        }


    }
}


namespace UnityEngine
{
    /// <summary>
    /// Helper extension methods for various components.
    /// </summary>
    public static partial class GameObjectExtension
    {
        /// <summary>
        /// Extension method for finding the first Flags component on an object with the given name.
        /// </summary>
        /// <returns></returns>
        public static Flags FindNamedFlags(this GameObject go, string name)
        {
            var comps = go.FindComponentsInEntity<Flags>();
            if (comps != null)
            {
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i].Name.Equals(name)) return comps[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Extension method for finding the first Flags component on an object with the given name.
        /// </summary>
        /// <returns></returns>
        public static Flags FindNamedFlags(this EntityRoot entity, string name, bool cacheComponents)
        {
            var comps = entity.FindComponentsInEntity<Flags>(cacheComponents);
            if (comps != null)
            {
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i].Name.Equals(name)) return comps[i];
                }
            }

            return null;
        }
    }
}


namespace Toolbox
{
    public class DemandFlagsComponent : Demand<Flags>
    {
        public DemandFlagsComponent(Action<Flags> callback) : base(callback) { }
    }
}

