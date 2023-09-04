using Peg.AutoCreate;
using Peg.Lazarus;
using Peg.Messaging;
using UnityEngine;

namespace Peg.Game
{
    /// <summary>
    /// Relenquishes this mob when it is dies.
    /// </summary>
    public class MobDeathRelenquish : LocalListenerMonoBehaviour
    {
        [Tooltip("Relenquish this object or destroy it?")]
        public DestructEffect Action;

        [Tooltip("Delay after death before mob is relenquished.")]
        public float Delay = 5.0f;

        IPoolSystem Lazarus;

        void Start()
        {
            Lazarus = AutoCreator.AsSingleton<IPoolSystem>();
            DispatchRoot.AddLocalListener<EntityDiedEvent>(HandleDeath);
            DispatchRoot.AddLocalListener<EntityRevivedEvent>(HandleRevive);
        }

        protected override void OnDestroy()
        {
            DispatchRoot.RemoveLocalListener<EntityDiedEvent>(HandleDeath);
            DispatchRoot.RemoveLocalListener<EntityRevivedEvent>(HandleRevive);
            base.OnDestroy();
        }

        void HandleDeath(EntityDiedEvent msg)
        {
            Invoke(nameof(ReturnToPool), Delay);
        }

        void HandleRevive(EntityRevivedEvent msg)
        {
            CancelInvoke(nameof(ReturnToPool));
        }

        public void ReturnToPool()
        {
            if (Action == DestructEffect.Relenquish)
                Lazarus.RelenquishToPool(gameObject);
            else Destroy(gameObject);
        }
    }
}
