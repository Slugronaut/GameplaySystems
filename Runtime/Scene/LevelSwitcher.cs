using Peg.MessageDispatcher;
using Peg.Systems;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
//using LazarusPool = Peg.Lazarus.Lazarus;

namespace Peg.Game.Scene
{
    /// <summary>
    /// Used to transition to new scenes with smooth effects like screen fade and BGM crossfade.
    /// </summary>
    public class LevelSwitcher : AbstractOperationOnEvent
    {
        public static string NextScene;

        public string SceneToLoad;
        public float DelayUntilSceneLoad = 1;
        public Color FadeColor = Color.black;
        public float FadeTime = 0.75f;
        public AudioClip Jingle;
        public float CrossFadeTime = 0.75f;
        //public bool RelenquishPools = false;

        System.Action CachedCallback;
        
        
        public override void PerformOp()
        {
            foreach (var col in GetComponents<Collider>())
                col.enabled = false;

            Fadeout(FadeComplete);
        }

        /// <summary>
        /// For linking to UIs.
        /// </summary>
        public void Fadeout()
        {
            GlobalMessagePump.Instance.PostMessage(new ChangeBGMCmd(Jingle, false, CrossFadeTime));
            ScreenFadeUtility.Instance.FadeTo(FadeColor, FadeTime, true, FadeComplete, FadeUpdate);
        }

        /// <summary>
        /// For linking to UIs.
        /// </summary>
        public void Fadeout(System.Action callback)
        {
            GlobalMessagePump.Instance.PostMessage(new ChangeBGMCmd(Jingle, false, CrossFadeTime));
            CachedCallback = callback;
            ScreenFadeUtility.Instance.FadeTo(FadeColor, FadeTime, true, FadeComplete, FadeUpdate);
        }

        /// <summary>
        /// For linking to UIs.
        /// </summary>
        public void Fadein()
        {
            GlobalMessagePump.Instance.PostMessage(new ChangeBGMCmd(Jingle, false, CrossFadeTime));
            ScreenFadeUtility.Instance.FadeFrom(FadeColor, FadeTime, FadeComplete, FadeUpdate);
        }

        /// <summary>
        /// For linking to UIs.
        /// </summary>
        public void Fadein(System.Action callback)
        {
            GlobalMessagePump.Instance.PostMessage(new ChangeBGMCmd(Jingle, false, CrossFadeTime));
            CachedCallback = callback;
            ScreenFadeUtility.Instance.FadeFrom(FadeColor, FadeTime, FadeComplete, FadeUpdate);
        }

        void FadeUpdate()
        {

        }

        void FadeComplete()
        {
            StartCoroutine(DelayedEndActions(DelayUntilSceneLoad, CachedCallback));
        }

        IEnumerator DelayedEndActions(float time, System.Action callback)
        {
            yield return new WaitForSecondsRealtime(time);
            GlobalMessagePump.Instance.PostMessage(new LevelSwitchCleanup());
            yield return null;
            callback?.Invoke();
            callback = null;
            LoadSelected();
        }

        public void LoadSelected()
        {
            //REMOVED: We should instead handle the LevelSwitchCleanup message and use that to clear lazarus pools.

            //***********************************************************************************************************************************
            //TODO: This is a huge problem! We are not recovering our pooled objects before swtiching scenes!
            //This might lead to leaks, null-refference errors, or at the very least, a loss of pooled
            //objects that must be re-created - thus defeating the use of the pool.

            //I think this line is fucking everything. Unity is just too stupid about how it handles
            //GameObjects between scene loads... :(
            //if(RelenquishPools)
            //    LazarusPool.Instance.RelenquishAll();
            //***********************************************************************************************************************************
            
            SwitchScenes(SceneToLoad);
        }

        /// <summary>
        /// Because of the fucky nature of Unity and all its goddamn race conditions we need to do some shit-hackery
        /// to make scene loading actually fucking work - hence the reason this motherfucker exists. Call it when you
        /// need a dirty bastard scene to load without it completely fucking everything to pieces. On the otherhand, if you
        /// *like* small disasters, then by all means just manually call Unity's SceneManager.LoadScene() yourself and
        /// see what happens.
        /// </summary>
        public static void SwitchScenes(string sceneToLoad)
        {
            NextScene = sceneToLoad;
            Time.timeScale = 1; //need to do this now or shit will get fucked
            SceneManager.LoadScene("Level Transition");
        }
        
    }


    /// <summary>
    /// Posted by LevelSwitcher after the fade has ended and before the new scene is loaded.
    /// </summary>
    [System.Serializable]
    public class LevelSwitchCleanup : IMessage { }


}
