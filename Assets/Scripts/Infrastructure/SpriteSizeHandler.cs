using UnityEditor;
using UnityEngine;

namespace Infrastructure
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSizeHandler : MonoBehaviour
    {
        [SerializeField, Min(0)] private float _ratio = 1f;
        [SerializeField] bool _applyOnStart = true;

        private SpriteRenderer _sr;


        private void Start()
        {
            if (_applyOnStart)
                ApplyRatio();
        }

        public void ApplyRatio(float? ratio = null)
        {
            if (ratio != null)
                _ratio = ratio.Value;

            if (!_sr)
                _sr = GetComponent<SpriteRenderer>();

            if (!_sr || !_sr.sprite)
                return;

            _sr.size = new Vector2(
                _ratio * _sr.sprite.rect.size.x,
                _ratio * _sr.sprite.rect.size.y);
        }


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
    }
}
