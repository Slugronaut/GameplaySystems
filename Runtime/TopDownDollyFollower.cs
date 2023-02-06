using UnityEngine;
using System.Collections;

namespace Toolbox.Game
{
    /// <summary>
    /// Follows a target in a dollied fashion. This allows for things like
    /// 3D cameras to work like top-down views but still follow on the vertical axis.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Toolbox/Game/Top-Down Dolly Follower")]
    public class TopDownDollyFollower : MonoBehaviour
    {
        public Transform Target;
        public float Offset = 0.0f;
        //public float Smoothness = 0.15f;
#pragma warning disable CS0169 // The field 'TopDownDollyFollower.Vel' is never used
        float Vel;
#pragma warning restore CS0169 // The field 'TopDownDollyFollower.Vel' is never used
        public float DeadZone = 3.0f;

        public Orientation BoomAxis;

        public enum Orientation
        {
            YUp,
            ZUp,
        }

        // Update is called once per frame
        void LateUpdate()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, Target.position.z + Offset);
            /*
            if (Target != null && Mathf.Abs((Target.position.z + Offset + (DeadZone / 2)) - transform.position.z) > DeadZone)
            {
                float smoothZ = transform.position.z;
                float t = 0.0f;
                while (t < 0.5f)
                {
                    t += Time.deltaTime * (Time.timeScale / 1.0f); // set the duration of the camera lerp in seconds
                    smoothZ = Mathf.Lerp(transform.position.z, Target.position.z + Offset, Smoothness * Time.deltaTime);
                }

                transform.position = new Vector3(transform.position.x, transform.position.y, smoothZ);
            }
            */
        }
    }
}
