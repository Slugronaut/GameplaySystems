using Peg.MessageDispatcher;

namespace Peg.Game.Interactions
{
    /// <summary>
    /// Command to invoke an interaction.
    /// </summary>
    public class InteractCmd : AgentTargetMessage<AbstractInteractor, IInteractable, InteractCmd>, IMessageCommand
    {
        public bool Validate { get; protected set; }

        public InteractCmd() : base() { }
        public InteractCmd(AbstractInteractor agent, IInteractable target, bool validate)
            : base(agent, target)
        {
            Validate = validate;
        }
    }
}
