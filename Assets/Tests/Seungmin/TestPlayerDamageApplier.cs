using Actors.PlayerSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tests.Seungmin
{
    public class TestPlayerDamageApplier : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private int _damage = 10000;

        private void Awake()
        {
            if (!_player)
                _player = GetComponent<Player>();
        }

        public void ApplyDamage() => ApplyDamage(_damage);
        public void ApplyDamage(int damage)
        {
            if (!_player) return;
            _player.GetComponent<PlayerHealth>().TakeDamage(damage);
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(TestPlayerDamageApplier))]
        private class TestPlayerDamageApplierEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                GUILayout.Space(8);

                if (Application.isPlaying)
                {
                    var target = (TestPlayerDamageApplier)base.target;

                    if (GUILayout.Button("Apply Damage"))
                        target.ApplyDamage();
                }
                else
                    GUILayout.Label("Enter play mode to apply damage", EditorStyles.centeredGreyMiniLabel);
            }
        }
#endif
    }
}
