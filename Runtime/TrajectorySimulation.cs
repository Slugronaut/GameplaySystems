using System.Collections.Generic;
using UnityEngine;
using Dweiss;
using Peg.Util;
using Peg.Lib;

namespace Peg.Game
{
    /// <summary>
    /// Used to simulate a ballistic trajectory using raycasts.
    /// </summary>
    public static class TrajectorySimulationUtil
    {
        // Reference to the LineRenderer we will use to display the simulated path
        //public LineRenderer sightLine;

        // Reference to a Component that holds information about fire strength, location of cannon, etc.
        //public PlayerFire playerFire;
        //public float FireStrength

        // Number of segments to calculate - more gives a smoother line
        //public int segmentCount = 20;

        // Length scale for each segment
        //public float segmentScale = 1;

        // gameobject we're actually pointing at (may be useful for highlighting a target, etc.)
        //private Collider _hitObject;
        //public Collider hitObject { get { return _hitObject; } }
        public class Contact
        {
            public Collider Collider;
            public Vector3 Point;
            public Vector3 Normal;

            public Contact()
            {
                Collider = null;
                Point = Vector3.zero;
                Normal = Vector3.zero;
            }

            public Contact(Collider col, Vector3 p, Vector3 n)
            {
                Collider = col;
                Point = p;
                Normal = n;
            }

            public void Set(Collider col, Vector3 p, Vector3 n)
            {
                Collider = col;
                Point = p;
                Normal = n;
            }
        }
        
        static List<Vector3> segments = new List<Vector3>(25);
        
        /// <summary>
        /// Simulate the path of a ballistic arc. Slight errors are inherent in the numerical method used.
        /// </summary>
        /// <returns>A Collision object describing the final point of the path if anything was struck. 
        /// This is a shared and volitile object and should not be cached.</returns>
        public static bool RaycastBallistic(Contact contact, Vector3 startPos, Vector3 forward, LayerMask layers, float fireStrength, float maxLength, float segmentLength = 1)
        {
            //initial values
            float currLen = 0;
            Vector3 segVelocity = forward * fireStrength;// * Time.deltaTime; //initial velocity. The integrator below is going to fuck this so bad...
            int segmentCount = Mathf.Max(Mathf.CeilToInt(maxLength / Mathf.Max(segmentLength, 0.01f)), 2);
            
            //make sure we have enough pre-allocated segments
            //segments.Clear();
            while (segments.Count < segmentCount)
                segments.Add(new Vector3());

            segments[0] = startPos;
            
            for (int i = 1; i < segmentCount; i++)
            {
                // Time it takes to traverse one segment of length segScale (careful if velocity is zero)
                float segTime = (segVelocity.sqrMagnitude != 0) ? segmentLength / segVelocity.magnitude : 0;
                // Add velocity from gravity for this segment's timestep
                segVelocity = segVelocity + Physics.gravity * segTime;
                
                var results = SharedArrayFactory.Hit1;
                //Debug.DrawRay(segments[i - 1], segVelocity, Color.blue);
                if(Physics.Raycast(segments[i - 1], segVelocity, out RaycastHit h, segmentLength, layers, QueryTriggerInteraction.Ignore))
                {
                    contact.Set(h.collider, h.point, h.normal);
                    return true;

                    //TODO - implement a bounce calculator that fills out a list of additional contact points

                    /*
                    // set next position to the position where we hit the physics object
                    segments[i] = segments[i - 1] + segVelocity.normalized * hit.distance;
                    // correct ending velocity, since we didn't actually travel an entire segment
                    segVelocity = segVelocity - Physics.gravity * (segmentScale - hit.distance) / segVelocity.magnitude;
                    // flip the velocity to simulate a bounce
                    segVelocity = Vector3.Reflect(segVelocity, hit.normal);
                    */
                    /*
                     * Here you could check if the object hit by the Raycast had some property - was 
                     * sticky, would cause the ball to explode, or was another ball in the air for 
                     * instance. You could then end the simulation by setting all further points to 
                     * this last point and then breaking this for loop.
                     */
                }
                // If our raycast hit no objects, then set the next position to the last one plus v*t
                else
                {
                    var currPos = segments[i - 1];
                    //This is where the errors will be introduced. Needs a better integration method!!!
                    segments[i] = segments[i - 1] + (segVelocity * segTime);
                }

                //this is also not a very accurate method of tracking distances, but it shouldn't be too bad as long as distances are reasonably small
                currLen += segmentLength;
                if (currLen > maxLength)
                    break;
            }

            return false;
        }


        /// <summary>
        /// Simulate the path of a ballistic arc. Slight errors are inherent in the numerical method used.
        /// </summary>
        /// <returns>A Collision object describing the final point of the path if anything was struck. 
        /// This is a shared and volitile object and should not be cached.</returns>
        public static bool RaycastBallistic_Heun(Contact contact, Vector3 startPos, Vector3 forward, LayerMask layers, float fireStrength, float maxLength, float segmentLength = 1)
        {
            //initial values
            float currLen = 0;
            Vector3 currentVel = forward * fireStrength;
            Vector3 currentPos = startPos;
            int segments = Mathf.CeilToInt(maxLength / Mathf.Max(segmentLength, 0.01f));


            while (currLen < maxLength)
            {
                //this is also not a very accurate method of tracking distances,
                //but it shouldn't be too bad as long as distances are reasonably small
                currLen += currentVel.magnitude;

                float segTime = (currentVel.sqrMagnitude != 0) ? segmentLength / currentVel.magnitude : 0;
                if (segTime <= 0) return false;


                var results = SharedArrayFactory.Hit1;
#if UNITY_EDITOR
                //Debug.DrawRay(currentPos, currentVel.normalized * segTime, Color.green);
                Debug.DrawRay(currentPos, currentVel.normalized * segmentLength, Color.green);
#endif
                if (Physics.Raycast(currentPos, currentVel.normalized * segmentLength, out RaycastHit h, segTime, layers, QueryTriggerInteraction.Ignore))
                {
                    contact.Set(h.collider, h.point, h.normal);
                    return true;
                }
                // If our raycast hit no objects, then set the next position to the last one plus v*t
                else
                {
                    MathUtils.Verlet(Time.fixedDeltaTime, Physics.gravity, ref currentPos, ref currentVel);
                    //Vector3 newPos;
                    //Vector3 newVel;
                    //Heuns(Time.fixedDeltaTime, currentPos, currentVel, out newPos, out newVel);
                    //Heuns(Time.fixedDeltaTime, currentPos, currentVel, out newPos, out newVel);
                    //currentPos = newPos;
                    //currentVel = newVel;
                }


            }

            return false;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="h"></param>
        /// <param name="currentPosition"></param>
        /// <param name="currentVelocity"></param>
        /// <param name="newPosition"></param>
        /// <param name="newVelocity"></param>
        public static void HeunsIntegrator(float h, Vector3 currentPosition, Vector3 currentVelocity, out Vector3 newPosition, out Vector3 newVelocity)
        {
            Vector3 accelerationFactorEuler = Physics.gravity;
            Vector3 accelerationFactorHeun = Physics.gravity;

            Vector3 velocityFactor = currentVelocity;

            Vector3 pos_E = currentPosition + h * velocityFactor;
            Vector3 vel_E = currentVelocity + h * accelerationFactorEuler;

            Vector3 pos_H = currentPosition + h * 0.5f * (velocityFactor + vel_E);
            Vector3 vel_H = currentVelocity + h * 0.5f * (accelerationFactorEuler + accelerationFactorHeun);

            newPosition = pos_H;
            newVelocity = vel_H;
        }


#if UNITY_EDITOR
        static public float DebugLength = 1.0f;
#endif

        //static Vector3[] points;
        /// <summary>
        /// 
        /// </summary>
        public static bool RaycastBallisticArc(Contact contact, Rigidbody body, LayerMask layers, float maxLength, int skip, float deltaTimeMultiplier)
        {
            float dt = Time.fixedDeltaTime * deltaTimeMultiplier;
            int segments = Mathf.CeilToInt(maxLength /dt ); //Mathf.CeilToInt(maxLength / Mathf.Max(segmentLength, 0.01f));
            var points = body.CalculateMovement(segments, dt);
            //if (points == null || points.Length < segments)
            //    points = new Vector3[segments];
            //body.CalculateMovementNonAlloc(ref points, Time.fixedTime, Vector3.zero, Vector3.zero);
            float currDist = 0;
            Vector3 lastPoint = body.position;

            //int skip = Mathf.FloorToInt((float)((float)segments / (float)subDivisions));
            int skipCount = 1;
            for (int i = 1; i < points.Length-1; i++)
            {
                var p1 = points[i];
                var p0 = points[i - 1];
                var dir = p1 - p0;
                var dist = dir.magnitude;

                if (skipCount < skip)
                {
                    skipCount++;
                    currDist += dist;
                    if (currDist > maxLength)
                        break;
                    continue;
                }
                else skipCount = 1;

                dir = p1 - lastPoint;
                dist = dir.magnitude;
                

                #if UNITY_EDITOR
                Debug.DrawRay(p0, dir.normalized * dist * DebugLength, Color.magenta, 3.0f);
                #endif

                var info = SharedArrayFactory.Hit1;
                if(Physics.Raycast(p0, dir.normalized, out RaycastHit h, dist, layers, QueryTriggerInteraction.Ignore))
                {
                    contact.Set(h.collider, h.point, h.normal);
                    return true;
                }

                lastPoint = p0;
                currDist += dist;
                if (currDist > maxLength)
                    break;
            }

            return false;
        }

        public static Vector3[] CalculateArcArray(int resolution, float angle, Vector3 velocity, Vector3 gravity)
        {
            Vector3[] arcArray = new Vector3[resolution + 1];
            float vel = velocity.magnitude;
            float g = Mathf.Abs(gravity.y);

            float radianAngle = Mathf.Deg2Rad * angle;
            float maxDistance = (vel * vel * Mathf.Sin(2 * radianAngle)) / g;

            for(int i = 0; i <= resolution; i++)
            {
                float t = (float)i / (float)resolution;
                arcArray[i] = CalculateArcPoint(t, maxDistance, g, vel, radianAngle);
            }

            return arcArray;
        }

        public static Vector3 CalculateArcPoint(float t, float maxDistance, float g, float vel, float radianAngle)
        {
            float x = t * maxDistance;
            float y = x * Mathf.Tan(radianAngle) - ((g * x * x) / (2 * vel * vel * Mathf.Cos(radianAngle) * Mathf.Cos(radianAngle)));
            return new Vector3(x, y);
        }


        public static bool UpdateTrajectory(Vector3 startPos, Vector3 direction, float speed, float timePerSegmentInSeconds, float maxTravelDistance)
        {
            var positions = new List<Vector3>();
            var lastPos = startPos;
            var currentPos = startPos;
            positions.Add(startPos);
            bool hasHitSomething = false;

            var traveledDistance = 0.0f;
            while (traveledDistance < maxTravelDistance)
            {
                traveledDistance += speed * timePerSegmentInSeconds;
               hasHitSomething = TravelTrajectorySegment(currentPos, direction, speed, timePerSegmentInSeconds, out currentPos);
                if (hasHitSomething)
                {
                    break;
                }
                lastPos = currentPos;
                //currentPos = positions[positions.Count - 1];
                direction = currentPos - lastPos;
                direction.Normalize();
            }

            return hasHitSomething;
        }

        public static bool TravelTrajectorySegment(Vector3 startPos, Vector3 direction, float speed, float timePerSegmentInSeconds, out Vector3 newPos)
        {
            newPos = startPos + direction * speed * timePerSegmentInSeconds + Physics.gravity * timePerSegmentInSeconds;

            RaycastHit hitInfo;
            var hasHitSomething = Physics.Linecast(startPos, newPos, out hitInfo);
            if (hasHitSomething)
                newPos = hitInfo.point;

            return hasHitSomething;
        }
    }
}