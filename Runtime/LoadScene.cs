using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Toolbox.Behaviours
{
    /// <summary>
    /// Simple component that can be used to load a scene when activated.
    /// </summary>
    public class LoadScene : MonoBehaviour
    {
        #if UNITY_EDITOR
        [SerializeField]
        [HideInInspector]
#pragma warning disable CS0169 // The field 'LoadScene._Scene' is never used
        UnityEditor.SceneAsset _Scene;
#pragma warning restore CS0169 // The field 'LoadScene._Scene' is never used
        #endif

        [SceneName]
        public string Scene;

        public bool UnscaledTime = true;
        public float Delay = 2;
        public bool FadeMusic = false;
        public float MusicFadeTime = 2;


        public void LoadSelected()
        {
            if (FadeMusic)
                GlobalMessagePump.Instance.PostMessage(new ChangeBGMCmd(null, false, MusicFadeTime));

            InvokeRealTime(InnerLoad, Delay);
        }

        void InnerLoad()
        {
            //make sure timescale is back to normal
            Time.timeScale = 1.0f;
            SceneManager.LoadScene(Scene);
        }

        public void InvokeRealTime(System.Action callback, float delay)
        {
            StartCoroutine(InvokeRealTimeHelper(callback, delay));
        }

        private IEnumerator InvokeRealTimeHelper(System.Action callback, float delay)
        {
            yield return StartCoroutine(Toolbox.CoroutineUtilities.WaitForRealTime(delay));
            callback?.Invoke();
        }
    }
}
