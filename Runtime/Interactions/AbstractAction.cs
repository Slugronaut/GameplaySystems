using System;
using UnityEngine;

namespace Peg.Game.Interactions
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractAction : AbstractInteractable
    {
        public Sprite UseIcon;


        private void Reset()
        {
            UseableBySubclasses = true;
        }

        protected override void Awake()
        {
            base.Awake();
            UseableBySubclasses = true;
        }

        protected override Type DeclaredUser()
        {
            UseableBySubclasses = true;
            return typeof(Actor);
        }
    }
}
