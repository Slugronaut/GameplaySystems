
using UnityEngine;
/// <summary>
/// A collection of interfaces that represent very common component achetypes that will be found in most games.
/// This allows us to search for components in a GameObject based on useage and archetype rather than by 
/// a specificly implemented class.
/// </summary>
namespace Peg.Game
{
    public interface IToolboxComponentInterface
    {
        bool enabled { get; set; }
    }

    public interface IInputSourceComponent : IToolboxComponentInterface
    {
        byte Id { get; set; }
        bool AllInputEnabled { get; set; }
        bool JumpEnabled { get; set; }
        bool MotionEnabled { get; set; }
        bool AimEnabled { get; set; }
        bool AttackEnabled { get; set; }
        bool InteractEnabled { get; set; }
        bool AvatarInputEnabled { get; set; }

        /// <summary>
        /// Can be used to make the entity look in a specifc direction as though the player had input it.
        /// </summary>
        /// <param name="input"></param>
        void SimulateLookDirection(Vector2 input);
    }
    
}