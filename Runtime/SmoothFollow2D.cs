using UnityEngine;

namespace Toolbox.Game
{
    [AddComponentMenu("Toolbox/Game/Smooth Follow 2D")]
    public class SmoothFollow2D : MonoBehaviour
    {
        public Transform Following;
        public float Speed = 5.0f;
        public Vector2 DeadZone;
        public Orientation Axis;
        [Compact]
        public Vector3 Offset;
        public bool FollowBoomAxis = false;
       
        public enum Orientation
        {
            XYAxis,
            XZAxis,
        }


        Vector2 LastFollowedPos;
        //Vector2 LastPos;

        void Update()
        {
            if (Axis == Orientation.XYAxis)
            {
                Vector2 target = Following.position;
                Vector2 temp = (Vector2)Toolbox.Math.MathUtils.SmoothApproach(
                    (Vector2)transform.position, 
                    (Vector2)LastFollowedPos, 
                    target,
                    Speed);
                LastFollowedPos = target;
                transform.position = new Vector3(temp.x + Offset.x, temp.y + Offset.y, (FollowBoomAxis) ? Following.position.z + Offset.z : Offset.z);
                
            }
            else
            {
                Vector2 target = new Vector2(Following.position.x, Following.position.z);
                Vector2 temp = (Vector2)Toolbox.Math.MathUtils.SmoothApproach(
                    new Vector2(transform.position.x, transform.position.z), 
                    LastFollowedPos, 
                    target, 
                    Speed);
                LastFollowedPos = target;
                transform.position = new Vector3(temp.x + Offset.x, (FollowBoomAxis) ? Following.position.y + Offset.y : Offset.z, temp.y + Offset.z);
                
            }
            //LastPos = transform.position;
        }
    }
}
