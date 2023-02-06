using UnityEngine;
using UnityEngine.AI;


namespace Toolbox.Game
{
    /// <summary>
    /// Sets the priority for an attached NavMeshAgent to a random value within a range.
    /// </summary>
    public class RandomNavPriority : MonoBehaviour
    {
        public int Min = 0;
        public int Max = 99;

        // Use this for initialization
        void OnEnable()
        {
            var agent = GetComponent<NavMeshAgent>();
            if (agent != null)
                agent.avoidancePriority = Random.Range(Min, Max + 1);

            Destroy(this);
        }


    }
}
