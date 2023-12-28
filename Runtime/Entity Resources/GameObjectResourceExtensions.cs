using Peg.AutonomousEntities;
using UnityEngine;

namespace Peg.Game.ConsumableResource
{

    /// <summary>
    /// Helper extension methods for various components.
    /// </summary>
    public static partial class GameObjectResourceExtension
    {
        /// <summary>
        /// Extension method for finding the first named EntityResource component on an object.
        /// </summary>
        /// <returns></returns>
        public static EntityResource FindConcreteEntityResource(this GameObject go, string name)
        {
            var comps = go.FindComponentsInEntity<EntityResource>();
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
        /// Extension method for finding the first named component on an object that exposes the IEntityResource interface.
        /// </summary>
        /// <returns></returns>
        public static IEntityResource FindEntityResourceInterface(this GameObject go, string name)
        {
            var comps = go.FindComponentsInEntity<IEntityResource>();
            if (comps != null)
            {
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i].Name.Value.Equals(name)) return comps[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Extension method for finding the first named component on an object that exposes the IEntityResource interface.
        /// </summary>
        /// <returns></returns>
        public static IEntityResource FindHierarchyResourceInterface(this GameObject go, string name)
        {
            var comps = go.FindComponentsInHierarchy<IEntityResource>();
            if (comps != null)
            {
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i].Name.Value.Equals(name)) return comps[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Extension method for finding the first named component on an object that exposes the IEntityResource interface.
        /// </summary>
        /// <returns></returns>
        public static IEntityResource FindEntityResourceInterface(this GameObject go, int nameHash)
        {
            var comps = go.FindComponentsInEntity<IEntityResource>();
            if (comps != null)
            {
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i].Name.Hash == nameHash) return comps[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Extension method for finding the first named component on an object that exposes the IEntityResource interface.
        /// </summary>
        /// <returns></returns>
        public static IEntityResource FindHierarchyResourceInterface(this GameObject go, int nameHash)
        {
            var comps = go.FindComponentsInHierarchy<IEntityResource>();
            if (comps != null)
            {
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i].Name.Hash == nameHash) return comps[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Extension method for finding the first named component on an object that exposes the IEntityResource interface.
        /// </summary>
        /// <returns></returns>
        public static IEntityResource FindEntityResourceInterface(this EntityRoot root, string name, bool useLookup)
        {
            var comps = root.FindComponentsInEntity<IEntityResource>(useLookup);
            if (comps != null)
            {
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i].Name.Value.Equals(name)) return comps[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Extension method for finding the first named component on an object that exposes the IEntityResource interface.
        /// </summary>
        /// <returns></returns>
        public static IEntityResource FindEntityResourceInterface(this EntityRoot root, int nameHash, bool useLookup)
        {
            var comps = root.FindComponentsInEntity<IEntityResource>(useLookup);
            if (comps != null)
            {
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i].Name.Hash == nameHash) return comps[i];
                }
            }

            return null;
        }

    }
}
