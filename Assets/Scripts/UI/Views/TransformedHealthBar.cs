using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class TransformedHealthBar : HealthBar
    {
        [SerializeField] Vector2 _localPosition = Vector2.zero;
        private Camera _camera;


        protected override void Awake()
        {
            base.Awake();
            _camera = Camera.main;
        }

        protected override void Update()
        {
            if (HeanthRateVM == null)
                return;

            base.Update();

            if (HeanthRateVM is IPositionedVM pvm)
                Transform.position
                    = _camera.WorldToScreenPoint(pvm.WorldBottomPosition)
                    + (Vector3)_localPosition;
        }
    }
}
