using UnityEngine;
using System.Collections;

namespace Peg.Game.Nav
{
    /// <summary>
    /// A low-power option for navigation that uses Unity's navmesh raycasting
    /// and teleportation rather than full path finding in order to move an entity around.
    /// 
    /// It is meant to be a tool used in-tandem with NavMeshAgent when entities are
    /// off-camera rather than a full-blown replacement for navmeshagents. Some properties
    /// of the NavMeshAgent (such as agent speed) will be used by this component.
    /// 
    /// TODO: use navmesh sampling to move rather than navagent raycasting. This way we can
    /// be sure we end up in a proper location AND we can use this while the agent is disabled.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
    public class TeleportNavigator : MonoBehaviour
    {
        public float MinWander = 5;
        public float MaxWander = 10;
        public static float Frequency = 2;

        public static readonly float Size = 3;

        Vector3 Destination;
        bool PendingMove;
        UnityEngine.AI.NavMeshAgent Agent;
        

        void Awake()
        {
            Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        }

        void OnEnable()
        {
            PendingMove = false;
            Destination = transform.position;
            StartCoroutine(RandomWander());
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator RandomWander()
        {
            while(true)
            {
                yield return CoroutineWaitFactory.RequestWait(Frequency);
                if (!PendingMove)
                {
                    Vector3 pos;
                    if (NavMeshUtilities.RandomPoint(transform.position, MinWander, MaxWander, 10, 2.5f, out pos))
                    {
                        UnityEngine.AI.NavMeshHit hit;
                        if (Agent.enabled && Agent.isOnNavMesh && Agent.Raycast(pos, out hit)) SetDestination(hit.position);
                        else SetDestination(pos);
                    }
                }
            }
        }

        bool IsVisible(Vector3 start, Vector3 end, Vector3 size)
        {
            var cam = Camera.main;
            var planes = GeometryUtility.CalculateFrustumPlanes(cam);

            return (GeometryUtility.TestPlanesAABB(planes, new Bounds(start, size)) ||
                    GeometryUtility.TestPlanesAABB(planes, new Bounds(end, size))) ? true : false;
        }

        void Teleport()
        {
            PendingMove = false;

            //never teleport if it would be visible to the main camera
            if (IsVisible(transform.position, Destination, new Vector3(Size, Size, Size)))
            {
                Destination = transform.position;
                return;
            }

            if(Agent.enabled) Agent.Warp(Destination);
            else transform.position = Destination;
        }

        public void SetDestination(Vector3 dest)
        {
            Destination = dest;
            PendingMove = true;
            Invoke("Teleport", Vector3.Distance(transform.position, dest) / Agent.speed);
        }
        
    }
}
