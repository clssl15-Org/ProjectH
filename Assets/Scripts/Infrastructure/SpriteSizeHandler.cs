using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infrastructure
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSizeHandler : MonoBehaviour
    {
        [SerializeField] private bool _useManualScaleFactor = false;
        [SerializeField, Min(0)] private float _scaleFactor = 0.01f;
        [SerializeField] private Configuration _configuration;

        private SpriteRenderer _sr;
        private int _applyScaleFactorCount = 0;


        public SpriteSizeHandler Initialize(Configuration configuration, bool apply = false)
        {
            _useManualScaleFactor = false;
            _configuration = configuration;

            if (apply)
                ApplyScaleFactor();

            return this;
        }
        public SpriteSizeHandler Initialize(float scaleFactor, bool apply = false)
        {
            _configuration = null;
            _useManualScaleFactor = true;
            _scaleFactor = scaleFactor;

            if (apply)
                ApplyScaleFactor();

            return this;
        }

        private void Start() => ApplyScaleFactor();
        public void RequestApplyScaleFactor() => _applyScaleFactorCount = 2;

        private void ApplyScaleFactor()
        {
            if (!_sr)
                _sr = GetComponent<SpriteRenderer>();

            if (!_sr || !_sr.sprite)
                return;

            if (!_useManualScaleFactor && _configuration)
                _scaleFactor = _configuration.PixelScaleFactor;

            _sr.drawMode = SpriteDrawMode.Sliced;
            _sr.size = new Vector2(
                _scaleFactor * _sr.sprite.rect.size.x,
                _scaleFactor * _sr.sprite.rect.size.y);
        }

        private void LateUpdate()
        {
            if (_applyScaleFactorCount > 0)
            {
                _applyScaleFactorCount--;
                ApplyScaleFactor();
            }
        }
        

#if UNITY_EDITOR
        [CustomEditor(typeof(SpriteSizeHandler))]
        private class SpriteSizeHandlerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                var target = (SpriteSizeHandler)this.target;

                if (!target._useManualScaleFactor && target._configuration)
                    DrawPropertiesExcluding(serializedObject, "_scaleFactor");
                else
                    DrawDefaultInspector();

                serializedObject.ApplyModifiedProperties();

                if (GUILayout.Button("Apply"))
                    target.ApplyScaleFactor();
            }
        }
#endif
    }
}
