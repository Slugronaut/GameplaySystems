using UnityEngine;
using System;
using Peg.Game;
using Peg;
using Peg.Messaging;
using Peg.Util;

namespace Peg.Game
{
    /// <summary>
    /// General-purpose entity tags.
    /// </summary>
    [AddComponentMenu("Toolbox/Game/Building Blocks/Tags (Name-to-Name)")]
    [Serializable]
    public sealed class Tags : LocalListenerBehaviour
    {
        [Tooltip("Name used to identify named features of this entity (character name, class name, rank, favorite food, etc...)")]
        public string Name;

        /// <summary>
        /// Specialized concrete, non-generic dictionary that maps strings to floats.
        /// Used for storing game-specific entity statistics.
        /// </summary>
        public StringStringHashMap Mapping;


        void Awake()
        {
            DispatchRoot.AddLocalListener<DemandTagsComponent>(OnDemandedMe);
        }

        protected override void OnDestroy()
        {
            DispatchRoot.RemoveLocalListener<DemandTagsComponent>(OnDemandedMe);
            base.OnDestroy();
        }

        void Reset()
        {
            Name = "Tags";
            Mapping = new StringStringHashMap();

        }

        void OnDemandedMe(DemandTagsComponent msg)
        {
            msg.Respond(this);
        }
        
    }
}



namespace UnityEngine
{
    /// <summary>
    /// Toolbox helper extension methods for various components.
    /// </summary>
    public static class TagsExtensionMethods
    {
        /// <summary>
        /// Extension method for finding the first Stats component on an object with the given name.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public static Tags FindNamedTags(this GameObject go, string name)
        {
            var comps = go.FindComponentsInEntity<Tags>();
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
        /// Extension method for finding the first Stats component on an object with the given name.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public static Tags FindNamedTags(this EntityRoot entity, string name, bool cacheComponents)
        {
            var comps = entity.FindComponentsInEntity<Tags>(cacheComponents);
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


    /// <summary>
    /// Used to demand a Tags componet using the message dispatch system.
    /// </summary>
    public class DemandTagsComponent : Demand<Tags>
    {
        public DemandTagsComponent(Action<Tags> callback) : base(callback) { }
    }
}



