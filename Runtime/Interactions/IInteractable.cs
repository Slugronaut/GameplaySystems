using Peg.AutonomousEntities;
using System;
using UnityEngine;

namespace Peg.Game.Interactions
{
    /// <summary>
    /// Interface exposed by all interactables. In most cases you want to derive from the abstract class <see cref="AbstractInteractable"/>.
    /// </summary>
    public interface IInteractable
    {
        int Priority { get; }
        bool CanUseSelf { get; }
        Type UserType { get; }
        bool UseableBySubclasses { get; }
        Vector3 ActivationPosition { get; }
        Vector3 ActivationPointOffset { get; }
        float MaxRange { get; }
        float MinRange { get; }
        float ReuseDelay { get; set; }
        bool ReadyForUse { get; }
        LocalMessageDispatch DispatchRoot { get; }
        GameObject gameObject { get; }
        bool enabled { get; }
        bool isActiveAndEnabled { get; }
        void OnInteract(AbstractInteractor user);

        void Callback_ConsumeUse();
    }

}
