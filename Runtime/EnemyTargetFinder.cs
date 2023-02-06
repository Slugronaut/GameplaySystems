using UnityEngine;
using System.Collections.Generic;

namespace Toolbox.Game
{
    /// <summary>
    /// Device for scanning the area for enemy targets. This is a dedicated script that will replace
    /// the NodeCanvas Find
    /// </summary>
    public class EnemyTargetFinder : MonoBehaviour
    {
        [Tooltip("The layermask to use when searching for a target.")]
        public LayerMask TargetLayer;

        bool HasTrans;
        Transform _Trans;
        public Transform Trans
        {
            get
            {
                if(!HasTrans)
                {
                    HasTrans = true;
                    _Trans = transform;
                }
                return _Trans;
            }
        }

        public bool HasTarget { get { return CurrentTarget != null; } }

        public GameObject CurrentTarget;



        void OnEnable()
        {
            EnemyTargetFinderSystem.Instance.Register(this);
        }

        private void OnDisable()
        {
            EnemyTargetFinderSystem.Instance.UnRegister(this);
        }
    }
    

}
