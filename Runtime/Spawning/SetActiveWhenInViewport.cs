using Peg.Lib;
using UnityEngine;

namespace Peg.Game.Spawning
{
    /// <summary>
    /// Sets the active state of a 
    /// </summary>
    public sealed class SetActiveWhenInViewport : MonoBehaviour
    {
        public bool State;
        public GameObject[] GameObjects;
        public Behaviour[] Behaviours;

        Transform Trans;
        static Camera Cam;



        void Awake()
        {
            Trans = transform;
        }

        private void OnEnable()
        {
            if (Cam == null)
                Cam = Camera.main;
        }

        private void Update()
        {
            bool inView = MathUtils.IsInViewport(Cam, Trans.position);

            if (inView) PerformOp(State);
            else PerformOp(!State);
        }

        public void PerformOp(bool state)
        {
            for (int i = 0; i < GameObjects.Length; i++)
                if(GameObjects[i].activeSelf != state) GameObjects[i].SetActive(state);

            for (int i = 0; i < Behaviours.Length; i++)
                if(Behaviours[i].enabled != state) Behaviours[i].enabled = state;
        }
    }
}
