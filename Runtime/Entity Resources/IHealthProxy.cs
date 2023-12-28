
namespace Peg.Game.ConsumableResource
{
    /// <summary>
    /// used by multiple components so that proxies can point to the location of the health script.
    /// </summary>
    public interface IHealthProxy
    {
        Health HealthSource { get; }
    }
}
