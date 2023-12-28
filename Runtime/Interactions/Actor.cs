using System;

namespace Peg.Game.Interactions
{
    /// <summary>
    /// Derived interactors from this that should be part of the interaction-scanning process on this entity.
    /// </summary>
    public class Actor : AbstractInteractor
    {
        private void Reset()
        {
            CanUseSubclasses = true;
        }

        protected override void Awake()
        {
            base.Awake();
            CanUseSubclasses = true;
        }

        protected override Type DeclaredUseable()
        {
            CanUseSubclasses = true;
            return typeof(AbstractAction);
        }

        protected override void Interaction(IInteractable interactable)
        {
            AbstractAction a = interactable as AbstractAction;
            a.OnInteract(this);
        }
    }

}
