
using Peg.AutonomousEntities;

namespace Peg.Game.Spawning
{

    /// <summary>
    /// Interface used by all spawners
    /// </summary>
    public interface ISpawner
    {
        //GameObject Spawn(Vector3 position, Transform parent);
        void RegisterSpawn(SpawnedEntity ent);
        void Despawned(SpawnedEntity ent);
        void Killed(SpawnedEntity ent);
        void InjectRegisteredSpawn(EntityRoot entity);
    }
}
