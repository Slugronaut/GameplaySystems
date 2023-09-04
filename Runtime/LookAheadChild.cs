using UnityEngine;

namespace Peg.Game
{
    /// <summary>
    /// Positions a child object ahead of it's parent based on the parent's motion.
    /// </summary>
    [DefaultExecutionOrder(25)]
    public class LookAheadChild : MonoBehaviour
    {
        public UpdateModes Mode = UpdateModes.LateUpdate;
        [Tooltip("Max distance ahead of the parent this child can move.")]
        public float Distance;
        public Transform Parent;
        public float SmoothTime = 0.5f;
        public float MotionThreshold = 0.001f;
        public bool X, Y, Z;
        [Tooltip("When the parent isn't moving, will this oject slowly return to the parten's origin or stay where it is.")]
        public bool DriftToOrigin;


        Transform MyTrans;
        Vector3 LastParentPos;

        Vector3 Current;
        Vector3 CurrentVel;

        private void Awake()
        {
            MyTrans = transform;
        }

        public void Update()
        {
            if (Mode == UpdateModes.Update)
                Process();
        }

        public void LateUpdate()
        {
            if (Mode == UpdateModes.LateUpdate)
                Process();
        }

        public void FixedUpdate()
        {
            if (Mode == UpdateModes.FixedUpdate)
                Process();
        }

        public void Teleport(Vector3 position)
        {
            MyTrans.position = position;
            Current = position;
            //LastParentPos = position;
            CurrentVel = Vector3.zero;
        }

        void Process()
        {
            if (Parent == null)
                return;

            var curPos = MyTrans.position;
            var parentPos = Parent.position;
            var dir = parentPos - LastParentPos;

            Vector3 finalPos = parentPos;

            //parent isn't moving, don't worry about this child object
            if (DriftToOrigin ||
                (X && Mathf.Abs(dir.x) > MotionThreshold) ||
                (Y && Mathf.Abs(dir.y) > MotionThreshold) ||
                (Z && Mathf.Abs(dir.z) > MotionThreshold))
            {
                LastParentPos = parentPos;
                dir.Normalize();
                Vector3 target = dir * Distance;
                Current = Vector3.SmoothDamp(Current, target, ref CurrentVel, SmoothTime);

                finalPos = new Vector3(X ? parentPos.x : curPos.x, Y ? parentPos.y : curPos.y, Z ? parentPos.z : curPos.z)
                                 + new Vector3(X ? Current.x : 0, Y ? Current.y : 0, Z ? Current.z : 0);
                if (!X) finalPos.x = parentPos.x;
                if (!Y) finalPos.y = parentPos.y;
                if (!Z) finalPos.z = parentPos.z;
                MyTrans.position = finalPos;
            }

        }
    }
}
