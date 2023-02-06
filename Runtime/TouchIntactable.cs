using UnityEngine;
using Sirenix.OdinInspector;

namespace Toolbox.Game
{
    /// <summary>
    /// Base class for interactables that will
    /// operate when a trigger collision occurs.
    /// </summary>
    public abstract class TouchInteractable : Interactable
    {
        [TabGroup("Touchable")]
        [PropertyTooltip("A list of tags that can be used to filter out who is allowed to interact with this object.")]
        [Tooltip("A list of tags that can be used to filter out who is allowed to interact with this object.")]
        public string[] AllowedInteractionTags;


        protected override void OnEnable()
        {
            base.OnEnable();
        }

        #if TOOLBOX_2DCOLLIDER
        void OnTriggerEnter2D(Collider2D col)
        {
            var who = col.gameObject;
            if (ReadyForUse)
            {
                if (AllowedInteractionTags == null || AllowedInteractionTags.Length < 1)
                    ProcessTouch(who);
                else
                {
                    for (int i = 0; i < AllowedInteractionTags.Length; i++)
                    {
                        if (col.CompareTag(AllowedInteractionTags[i]))
                        {
                            ProcessTouch(who);
                            return;
                        }
                    }
                }
            }
        }

        #else

        void OnTriggerEnter(Collider col)
        {
            var who = col.gameObject;
            if (ReadyForUse)
            {
                if (AllowedInteractionTags == null || AllowedInteractionTags.Length < 1)
                    ProcessTouch(who);
                else
                {
                    for (int i = 0; i < AllowedInteractionTags.Length; i++)
                    {
                        if (col.CompareTag(AllowedInteractionTags[i]))
                        {
                            ProcessTouch(who);
                            return;
                        }
                    }
                }
            }
        }
        #endif

        protected abstract void ProcessTouch(GameObject who);
    }
}
