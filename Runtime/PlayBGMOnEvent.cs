using UnityEngine;

namespace Peg.Behaviours
{
    /// <summary>
    /// Posts a message to the global BGM to play a new song
    /// upon a triggering unity event.
    /// </summary>
    public class PlayBGMOnEvent : AbstractOperationOnEvent
    {
        public AudioClip Clip;
        public float Volume = 1.0f;
        public float FadeTime;
        public float Start = 0;
        public float StartLoop = -1;
        public float EndLoop = -1;
        public bool Loop = true;

        public override void PerformOp()
        {
            GlobalMessagePump.Instance.PostMessage(new ChangeBGMCmd(Clip, Volume, Loop, FadeTime, Start, StartLoop, EndLoop));
        }
    }
}
