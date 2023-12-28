
namespace Peg.Game.ConsumableResource
{
    /// <summary>
    /// Used by entity resources and health as a unified way of querying their state.
    /// </summary>
    public interface IEntityResource
    {
        HashedString Name { get; }
        float CurrentPercent { get; set; }
        float Current { get; set; }
        float Max { get; }
    }
}
