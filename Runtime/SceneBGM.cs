using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Toolbox.Behaviours
{
    /// <summary>
    /// used to tag the BGM player for the scene.
    /// 
    /// TODO: Add ability to cross-fade
    /// TODO: Add support for loop count
    /// TODO: Add support for queued songs
    /// 
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SceneBGM : MonoBehaviour
    {
        /// <summary>
        /// How frequently we update the volume when cross fading.
        /// Default is 0.05f.
        /// </summary>
        public static float Freq = 0.05f;


        void Awake()
        {
            GlobalMessagePump.Instance.AddListener<ChangeBGMCmd>(HandleChangeSong);
        }
        
        void OnDestroy()
        {
            GlobalMessagePump.Instance.RemoveListener<ChangeBGMCmd>(HandleChangeSong);
        }

        void HandleChangeSong(ChangeBGMCmd msg)
        {
            //TODO: add cross-fading options
            var source = GetComponent<AudioSource>();
            source.loop = msg.Loop;

            var loopPoints = GetComponent<AudioSourceLoopPoints>();
            if (loopPoints == null)
                loopPoints = gameObject.AddComponent<AudioSourceLoopPoints>();

            loopPoints.StartPoint = msg.Start;
            float startLoop = msg.StartLoop;
            float endLoop = msg.EndLoop;
            if (startLoop >= 0 || endLoop >= 0)
            {
                loopPoints.LoopStart = startLoop;
                loopPoints.LoopEnd = endLoop;
            }
            else
            {
                loopPoints.LoopStart = 0;
                loopPoints.LoopEnd = float.MaxValue;
            }
            

            if (msg.FadeTime <= 0)
            {
                source.clip = msg.Clip;
                if (msg.Clip == null) source.Stop();
                else
                {
                    SetPlayCursor(msg.Start, source);
                    source.volume = msg.Volume;
                    source.Play();
                }
            }
            else CrossFade(msg.FadeTime, msg.Volume, source, msg.Clip, msg.Start);
        }

        /// <summary>
        /// Sets the play cursor on the audio source. A clip must be provided to the source
        /// beforehand or this method will thrown and exception.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="source"></param>
        public static void SetPlayCursor(float time, AudioSource source)
        {
            Assert.IsNotNull(source);
            Assert.IsNotNull(source.clip);

            source.timeSamples = (int)(time * source.clip.frequency);
        }

        /// <summary>
        /// Currently this does not cross fade, but instead simply fades
        /// out the original bgm and then starts the new one at full volume.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public void CrossFade(float time, float targetVolume, AudioSource source, AudioClip dest, float startTime)
        {
            StartCoroutine(Fade(time, targetVolume, source, dest, startTime));
        }
        
        /// <summary>
        /// This doesn't actually support cross-fading yet. It simply fades out the first song
        /// and then fades in the next. This is due to the support for only a single audio source.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        IEnumerator Fade(float time, float targetVolume, AudioSource source, AudioClip dest, float startTime)
        {
            float start = Time.unscaledTime;
            float inc = Freq / time * source.volume;
            
            if (source.isPlaying)
            {
                time *= 0.5f; //we'll need to fade out first so let's half our time spent fading in and out
                inc = Freq / time;
                while (Time.unscaledTime - start < time)
                {
                    source.volume -= inc;

                    //needed to adapt this to work even when Time.scale is 0
                    yield return Toolbox.CoroutineUtilities.WaitForRealTime(Freq);// CoroutineWaitFactory.RequestWait(Freq);
                }
            }


            source.Stop();
            source.clip = dest;
            inc = (Freq / time) * targetVolume;
            if (dest != null)
            {
                source.volume = 0;
                SetPlayCursor(startTime, source);
                source.Play();

                if (source.isPlaying)
                {
                    while (Time.unscaledTime - start < time)
                    {
                        source.volume += inc;
                        //needed to adapt this to work even when Time.scale is 0
                        yield return Toolbox.CoroutineUtilities.WaitForRealTime(Freq);// CoroutineWaitFactory.RequestWait(Freq);
                    }
                }
                source.volume = targetVolume;
            }
        }
    }


    /// <summary>
    /// Issue this command to make the SceneBGM change its current song.
    /// </summary>
    public class ChangeBGMCmd : IMessageCommand
    {
        public AudioClip Clip { get; private set; }
        public float Volume { get; private set; }
        public float Start { get; private set; }
        public float StartLoop { get; private set; }
        public float EndLoop { get; private set; }
        public bool Loop { get;  private set;}
        public float FadeTime { get; private set; }

        public ChangeBGMCmd(AudioClip clip, bool loop = true, float fadeTime = 0, float start = 0, float startLoop = -1, float endLoop = -1)
        {
            Clip = clip;
            Loop = loop;
            FadeTime = fadeTime;
            Start = start;
            StartLoop = startLoop;
            EndLoop = endLoop;
        }

        public ChangeBGMCmd(AudioClip clip, float volume, bool loop = true, float fadeTime = 0, float start = 0, float startLoop = -1, float endLoop = -1)
        {
            Clip = clip;
            Volume = volume;
            Loop = loop;
            FadeTime = fadeTime;
            Start = start;
            StartLoop = startLoop;
            EndLoop = endLoop;
        }
    }
}
