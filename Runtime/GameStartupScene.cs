using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Peg.AutoCreate;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Peg.Game
{
    /// <summary>
    /// Utility singleton that aids in quickly getting scenes started.
    /// It provide a means of supplying both a default startup scene,
    /// a followup startup scene, and the ability to return the scene that
    /// was most recently active in the editor before pressing play.
    /// </summary>
    [AutoCreate(CreationActions.DeserializeSingletonData)]
    public sealed class GameStartupScene
    {
        #if UNITY_EDITOR
        //[HideInInspector]
        //[SerializeField]
        //SceneAsset _StartupScene;

        //[HideInInspector]
        //[SerializeField]
        //SceneAsset _FollowupScene;
        #endif

        [Tooltip("Can be used to easily disable auto-scene switching on startup while leaving configuration alone.")]
        public bool DisableStartupScene;

        [Tooltip("If set, will load the provided startup scene upon starting playmode.")]
        //[SceneName]
        public string StartupScene;

        #if UNITY_EDITOR
        [Tooltip("If set, when play is pressed in the editor, any startup scene provided will be loaded and then the last scene open in the editor before playing will be loaded.")]
        public bool UseCurrentEditorScene;
        #endif

        [Tooltip("If set, and if EditorScene is not set, will load this scene after loading the startup scene.")]
        //[SceneName]
        public string FollowupScene;


        void AutoStart()
        {
            if (Application.isPlaying && !DisableStartupScene)
            {
                #if UNITY_EDITOR
                var scenes = AddSceneToQueue(
                    StartupScene,
                    (UseCurrentEditorScene ? StartGameRegistry.EditorActiveScene : null),
                    ((StartGameRegistry.EditorActiveScene == null || !UseCurrentEditorScene) ? FollowupScene : null)
                    );
                #else
                var scenes = AddSceneToQueue(StartupScene, FollowupScene);
                #endif
                foreach (var scene in scenes)
                {
                    if (scene != null)
                        SceneManager.LoadScene(scene, LoadSceneMode.Single);
                }
            }

            System.GC.Collect();
        }

        List<string> AddSceneToQueue(params string[] scenes)
        {
            List<string> queue = new List<string>(5);
            foreach (var scene in scenes)
            {
                if (!string.IsNullOrEmpty(scene))
                    queue.Add(scene);
            }

            return queue;
        }
    }


#if UNITY_EDITOR
    [InitializeOnLoad]
    public static class StartGameRegistry
    {
        public static string EditorActiveScene;

        static StartGameRegistry()
        {
            if (!Application.isPlaying)
            {
                EditorActiveScene = EditorSceneManager.GetActiveScene().name;
            }
        }
    }
#endif
}

