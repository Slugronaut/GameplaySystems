using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using Toolbox.AutoCreate;

namespace Toolbox.Game
{
    /// <summary>
    /// This singleton can be used to track spawn positions throughout the scene.
    /// </summary>
    [AutoCreate(CreationActions.DeserializeSingletonData)]
    public sealed class SpawnPointPlacement
    {
        [Tooltip("A manual override for the Camera to use for culling. NOTE: The camera must not be rendering to a texture!")]
        public Camera CullingOverride;
        [Tooltip("A tag that names a manual override for the Camera to use for culling. This will not be used if CullingOverride is set.")]
        public string CullingOverrideTag;


        public Transform[] ManualPlacement;
        public float ScreenSize = 15;

        public bool UseDistanceBands = false;
        [Tooltip("Somtimes Unity is a shithead and can't figure out how to cull BoundingSpeheres properly. If that's the case we'll just have to check this box and do it manually using frustum culling.")]
        public bool ManuallyCullPoints = false;

        HashSet<CulledSpawnPoint> CurrentPoints = new HashSet<CulledSpawnPoint>();
        BoundingSphere[] SpawnBounds = new BoundingSphere[0];
        CullingGroup SpawnGroup;
        public static SpawnPointPlacement Instance { get; private set; }
        
        
        void AutoAwake()
        {
            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
            GlobalMessagePump.Instance.AddListener<SceneReadyForPlay>(HandleSceneReady);
        }

        void AutoDestroy()
        {
            if (SpawnGroup != null)
            {
                SpawnGroup.Dispose();
                SpawnGroup = null;
            }
            SceneManager.sceneLoaded -= OnSceneLoaded;
            GlobalMessagePump.Instance.RemoveListener<SceneReadyForPlay>(HandleSceneReady);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CollectSpawnPoints();
        }
        
        void HandleSceneReady(SceneReadyForPlay msg)
        {
            //this.InvarientDelay(CollectSpawnPoints);
            CollectSpawnPoints();
        }

        public void RemovePoint(CulledSpawnPoint point)
        {
            CurrentPoints.Remove(point);
            BuildSpawnList(CurrentPoints);
        }

        public void RestorePoint(CulledSpawnPoint point)
        {
            CurrentPoints.Add(point);
            BuildSpawnList(CurrentPoints);
        }

        void BuildSpawnList(HashSet<CulledSpawnPoint> points)
        {
            SpawnBounds = new BoundingSphere[points.Count];
            int i = 0;
            foreach(var p in points)
            {
                SpawnBounds[i] = new BoundingSphere(p.transform.position, p.Radius);
               //GameObject.Destroy(list[i], 0.1f);
                i++;
            }
            SpawnGroup.SetBoundingSpheres(SpawnBounds);
            SpawnGroup.SetBoundingSphereCount(SpawnBounds.Length);
        }

        static readonly string SpawnPointTag = "CulledSpawnPoint";
        void CollectSpawnPoints()
        {
            Debug.Log("<color=green>Collecting spawn points...</color>");
            if (SpawnGroup != null)
            {
                SpawnGroup.Dispose();
                SpawnGroup = null;
            }
            CurrentPoints = new HashSet<CulledSpawnPoint>(GameObject.FindObjectsOfType<CulledSpawnPoint>());
            if (CurrentPoints == null || CurrentPoints.Count < 1)
            {
                CurrentPoints = new HashSet<CulledSpawnPoint>(GameObject.FindGameObjectsWithTag(SpawnPointTag).Where((x) => x != null).Select((go) => go.GetComponent<CulledSpawnPoint>()));
                if(CurrentPoints == null || CurrentPoints.Count < 1)
                    return;
            }

            SpawnGroup = new CullingGroup();
            if (CullingOverride == null)
                CullingOverride = GameObject.FindGameObjectWithTag(CullingOverrideTag).GetComponent<Camera>();
            SpawnGroup.targetCamera = CullingOverride ?? Camera.main;

            BuildSpawnList(CurrentPoints);

            if (ScreenSize > 0)
                SpawnGroup.SetBoundingDistances(new float[] { ScreenSize, ScreenSize * 2, float.PositiveInfinity });
            Debug.Log("<color=green>... spawn points collected.</color>");
        }

        static int CullVisiblePoints(int[] results)
        {
            //var planes = UnityEngine.GeometryUtility.CalculateFrustumPlanes(Instance.CullingOverride);
            //GeometryUtility.TestPlanesAABB
            //return 0;
            throw new UnityException("Not yet implemented.");
        }

        /// <summary>
        /// Because the culling API is broken when using render targets, I had to make a custrom workaraound
        /// that utilizes distance bands that are the size of the camera. This only works with top-down views
        /// and will require a tree structure will camera frustum culling tests to work in a general case.
        /// </summary>
        /// <param name="visible"></param>
        /// <param name="distanceIndex"></param>
        /// <param name="result"></param>
        /// <param name="firstIndex"></param>
        static int QueryIndices(bool visible, int distanceIndex, int[] result, int firstIndex, Vector3 point)
        {
            if (Instance.ScreenSize == 0) distanceIndex = 0;
            var group = Instance.SpawnGroup;
            if(group == null)
            {
                Debug.Log("Something was fucked with the spawn points. Fixing the fucking problem now, or whatever.");
                Instance.CollectSpawnPoints();
                group = Instance.SpawnGroup;
                if (group == null)
                    throw new UnityException("Nope. Still fucked. Just like Unity.");
            }
            if (Instance.UseDistanceBands)
            {
                group.SetDistanceReferencePoint(point);
                if (Instance.ManuallyCullPoints) return CullVisiblePoints(result);
                else return group.QueryIndices(visible, distanceIndex, result, firstIndex);
            }
            else
            {
                if (Instance.ManuallyCullPoints) return CullVisiblePoints(result);
                else return group.QueryIndices(visible, result, firstIndex);
            }
        }

        /// <summary>
        /// Returns the worldspace position of the closest CulledSpawn
        /// point within the scene that is off-camera.
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetClosestToPoint(Vector3 point)
        {
            if (Instance == null || Instance.SpawnBounds.Length < 1) return Vector3.zero;


            int[] indices = new int[Instance.SpawnBounds.Length];
            int count = QueryIndices(false, 1, indices, 0, point);
            Vector3 closestPos = Vector3.zero;

            float closest = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                int j = indices[i];
                float dist = Vector3.Distance(Instance.SpawnBounds[j].position, point);
                if (dist < closest)
                {
                    closestPos = Instance.SpawnBounds[j].position;
                    closest = dist;
                }

            }

            return closestPos;
        }

        /// <summary>
        /// Gets the second closest spawnpoint to a point in worldpsace.
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetSecondClosestToPoint(Vector3 point)
        {
            if (Instance == null || Instance.SpawnBounds == null || Instance.SpawnBounds.Length < 1) return Vector3.zero;

            int[] indices = new int[Instance.SpawnBounds.Length];
            int count = QueryIndices(false, 1, indices, 0, point);
            Vector3 secondClosestPos = Vector3.zero;

            float closestDist = float.MaxValue;
            float secondClosestDist = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                int j = indices[i];
                float dist = Vector3.Distance(Instance.SpawnBounds[j].position, point);
                if (dist <= closestDist)
                    closestDist = dist;
                else if (dist < secondClosestDist)
                {
                    secondClosestPos = Instance.SpawnBounds[j].position;
                    secondClosestDist = dist;
                }

            }

            return secondClosestPos;
        }

        /// <summary>
        /// Gets the furthest spawnpoint from a point in worldspace.
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetFurthestFromPoint(Vector3 point)
        {
            if (Instance == null || Instance.SpawnBounds.Length < 1) return Vector3.zero;

            int[] indices = new int[Instance.SpawnBounds.Length];
            int count = QueryIndices(false, 1, indices, 0, point);
            Vector3 furthestPos = Vector3.zero;

            float furthest = 0;
            for (int i = 0; i < count; i++)
            {
                int j = indices[i];
                float dist = Vector3.Distance(Instance.SpawnBounds[j].position, point);
                if (dist > furthest)
                {
                    furthestPos = Instance.SpawnBounds[j].position;
                    furthest = dist;
                }

            }

            return furthestPos;
        }

        /// <summary>
        /// Gets the second furthest spawnpoint from a point in worldspace.
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetSecondFurthestFromPoint(Vector3 point)
        {
            if (Instance == null || Instance.SpawnBounds.Length < 1) return Vector3.zero;

            int[] indices = new int[Instance.SpawnBounds.Length];
            int count = QueryIndices(false, 1, indices, 0, point);
            Vector3 secondFurthestPos = Vector3.zero;

            float furthestDist = 0;
            float secondFurthestDist = 0;
            for (int i = 0; i < count; i++)
            {
                int j = indices[i];
                float dist = Vector3.Distance(Instance.SpawnBounds[j].position, point);
                if (dist >= furthestDist)
                    furthestDist = dist;
                else if (dist > secondFurthestDist)
                {
                    secondFurthestDist = dist;
                    secondFurthestPos = Instance.SpawnBounds[j].position;
                }

            }

            return secondFurthestPos;
        }

        /// <summary>
        /// Picks a spawnpoint at random.
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetRandomOffCamera()
        {
            if (Instance == null || Instance.SpawnBounds.Length < 1) return Vector3.zero;

            int[] indices = new int[Instance.SpawnBounds.Length];
            int count = Instance.SpawnGroup.QueryIndices(false, 0, indices, 0);

            //TODO: loop through to until we find one that passes camera frustum culling test
            if (Instance.ManuallyCullPoints) throw new UnityException("Not yet implemented.");
            return Instance.SpawnBounds[indices[Random.Range(0, count)]].position;
        }

        /// <summary>
        /// Gets a valid spawnpoint just right of the player's view on camera.
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetSpawnStageRight()
        {
            return Vector3.zero;
        }

        /// <summary>
        /// Gets a valid spawnpoint just left of the player's view on camera.
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetSpawnStageLeft()
        {
            return Vector3.zero;
        }

        /// <summary>
        /// Gets a valid spawnpoint just above of the player's view on camera.
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetSpawnStageBack()
        {
            return Vector3.zero;
        }

        /// <summary>
        /// Gets a valid spawnpoint just below of the player's view on camera.
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetSpawnStageFront()
        {
            return Vector3.zero;
        }

        /// <summary>
        /// Helper for getting a spawn point based on a given enumeration method.
        /// </summary>
        /// <param name="method"></param>
        public static Vector3 LocationMethodChooser(SpawnLocationMethod method, Vector3 pos, int placementIndex = 0)
        {
            switch (method)
            {
                case SpawnLocationMethod.Closest: return GetClosestToPoint(pos);
                case SpawnLocationMethod.Furthest: return GetFurthestFromPoint(pos);
                case SpawnLocationMethod.SecondClosest: return GetSecondClosestToPoint(pos);
                case SpawnLocationMethod.SecondFurthest: return GetSecondFurthestFromPoint(pos);
                case SpawnLocationMethod.Random: return GetRandomOffCamera();
                case SpawnLocationMethod.Placed: return Instance.ManualPlacement[placementIndex].position;
                default: return Vector3.zero;
            }
        }
    }


    public enum SpawnLocationMethod
    {
        Local,
        Closest,
        SecondClosest,
        Furthest,
        SecondFurthest,
        Random,
        Placed,
    }
}
