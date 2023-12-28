using Peg.MessageDispatcher;
using UnityEngine.Events;

namespace Peg.Game.Interactions
{
    /// <summary>
    /// Event used internally by Interactor to trigger use event in the interactable.
    /// </summary>
    public class InteractableUseEvent : IMessage
    {
        public AbstractInteractor User { get; private set; }
        public UnityAction<AbstractInteractor.Use> Callback;

        public InteractableUseEvent(AbstractInteractor user)
        {
            User = user;
        }
    }
}
