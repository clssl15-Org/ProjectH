using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infrastructure
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSizeHandler : MonoBehaviour
    {
        [SerializeField, Min(0)] private float _ratio = 1f;
        [SerializeField] UpdateTypes _updateTypes = UpdateTypes.Never;

        [Flags]
        private enum UpdateTypes
        {
            Never = 0,
            Start = 1 << 0,
            EveryFrame = 1 << 1,
        }

        private SpriteRenderer _sr;

        public void ApplyRatio(float? ratio = null)
        {
            if (ratio != null)
                _ratio = ratio.Value;

            if (!_sr)
                _sr = GetComponent<SpriteRenderer>();

            if (!_sr || !_sr.sprite)
                return;

            _sr.drawMode = SpriteDrawMode.Sliced;
            _sr.size = new Vector2(
                _ratio * _sr.sprite.rect.size.x,
                _ratio * _sr.sprite.rect.size.y);
        }

        private void Start()
        {
            if (_updateTypes.HasFlag(UpdateTypes.Start))
                ApplyRatio();
        }

        private void LateUpdate()
        {
            if (_updateTypes.HasFlag(UpdateTypes.EveryFrame))
                ApplyRatio();
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(SpriteSizeHandler))]
        private class SpriteSizeHandlerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Apply"))
                    ((SpriteSizeHandler)target).ApplyRatio();
            }
        }
#endif
    }
}
