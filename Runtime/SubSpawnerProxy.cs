using UnityEngine;


namespace Toolbox.Game
{
    /// <summary>
    /// Forwards another SpawnedEntity's Spawner to this entity's spawner
    /// </summary>
    [RequireComponent(typeof(SpawnedEntity))]
    public class SubSpawnerProxy : MonoBehaviour
    {

        public void RegisterSubSpawn(SpawnedEntity subEnt)
        {
            GetComponent<SpawnedEntity>().Spawner.RegisterSpawn(subEnt);
        }

        public void InjectRegisteredSubSpawn(EntityRoot subEnt)
        {
            GetComponent<SpawnedEntity>().Spawner.InjectRegisteredSpawn(subEnt);
        }
    }
}
