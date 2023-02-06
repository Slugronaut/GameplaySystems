using UnityEngine;
using System;
using Toolbox.Messaging;

namespace Toolbox.Behaviours
{
    /// <summary>
    /// Posts a message to the global BGM to play a new song
    /// upon a triggering unity event.
    /// </summary>
    public class PlayBGMOnMessage : AbstractMessageReciever
    {
        public AudioClip Clip;
        public float Delay;
        public float FadeTime;
        public float Start = 0;
        public float StartLoop = -1;
        public float EndLoop = -1;
        public bool Loop = true;
        

        protected override void HandleMessage(Type msgType, object msg)
        {
            if (Delay > 0)
                Invoke(nameof(DelayPlay), Delay);
            else GlobalMessagePump.Instance.PostMessage(new ChangeBGMCmd(Clip, Loop, FadeTime, Start, StartLoop, EndLoop));
        }

        void DelayPlay()
        {
            GlobalMessagePump.Instance.PostMessage(new ChangeBGMCmd(Clip, Loop, FadeTime, Start, StartLoop, EndLoop));
        }
    }
}
