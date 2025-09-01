using UniEngine.StateMachines.FSM;
using UnityEngine;

namespace MonsterActions
{
    internal class HitFlash : MonsteActionState
    {
        private float? mainAnimationLength;

        public HitFlash() : base(MonsterAction.Hit.ToString())
        {

        }

        protected override void Initialize()
        {
            AddChild(new FlashAction(), true);
            AddChild(new AfterAction());
        }

        protected override void OnEnter(params object[] inputs)
        {
            SetInputs(inputs);

            mainAnimationLength = Playtime.HasValue
                ? (Playtime.Value >= 0 ? Playtime.Value : null)
                : Owner.InvincibleDuration;

            MainAnimationRemainingTime = mainAnimationLength;
            isCompleted = false;
        }

        protected override void OnExit()
        {
            Parent.SetNextToNone();
            base.OnExit();
        }



        internal class FlashAction : Work<HitFlash>
        {
            private Monster Owner => Parent.Owner;

            private Material originalMaterial;
            private bool materialRestored;


            protected override void OnEnter(params object[] _)
            {
                originalMaterial = Owner.SpriteRenderer.material;
                Owner.SpriteRenderer.material = Owner.sceneAssetsLibrary.SolidColor;
                Owner.SpriteRenderer.material.color = Color.white;

                materialRestored = false;
            }

            protected override void OnUpdate()
            {
                if (!Parent.MainAnimationRemainingTime.HasValue)
                    return;

                if (!Owner.SpriteRenderer)
                    Parent.Exit();


                Parent.MainAnimationRemainingTime -= Time.deltaTime;

                if (!materialRestored && Parent.MainAnimationRemainingTime <= Parent.mainAnimationLength - Owner.damageFlashDuration)
                {
                    Owner.SpriteRenderer.material = originalMaterial;
                    materialRestored = true;
                }

                if (Parent.MainAnimationRemainingTime <= 0)
                {
                    if (Parent.StayTimeAfterFinished > 0)
                        Parent.SetNext<AfterAction>();
                    else
                        Parent.Complete();
                }
            }

            protected override void OnExit()
            {
                if (!materialRestored && Owner.SpriteRenderer)
                    Owner.SpriteRenderer.material = originalMaterial;
            }
        }
    }
}
