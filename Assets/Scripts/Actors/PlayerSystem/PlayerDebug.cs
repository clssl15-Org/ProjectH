using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Actors.PlayerSystem
{
    public class PlayerDebug : MonoBehaviour
    {
        [SerializeField]
        private PlayerHealth playerHealth;
        [SerializeField]
        int damageAmount = 10;
        private Player player;

        private void Awake()
        {
            if (!playerHealth)
                playerHealth = this.transform.root.GetComponentInChildren<PlayerHealth>();

            player = GetComponent<Player>();
        }
        private void Start()
        {
        }
        public void DamageToPlayer()
        {
            playerHealth.TakeDamage(damageAmount);
        }
        public void AddFirstSkill()
        {
            RushStabbing firstSkill = player.StatesGO.GetComponent<RushStabbing>();
            player.SkillManager.AddSkill(firstSkill);
        }
        public void AddSecondSkill()
        {
            StrongAttack secondSkill = player.StatesGO.GetComponent<StrongAttack>();
            player.SkillManager.AddSkill(secondSkill);
        }
        public void AddThirdSkill()
        {
            Skill3 thirdSkill = player.StatesGO.GetComponent<Skill3>();
            player.SkillManager.AddSkill(thirdSkill);
        }
        public void StunPlayer()
        {
            playerHealth.Stun();
        }
    }

    [CustomEditor(typeof(PlayerDebug))]
    public class DebugButton : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PlayerDebug script = (PlayerDebug)target;
            if (GUILayout.Button("Damage to Player"))
            {
                script.DamageToPlayer();
            }
            if (GUILayout.Button("Add First Skill"))
            {
                script.AddFirstSkill();
            }
            if (GUILayout.Button("Add Second Skill"))
            {
                script.AddSecondSkill();
            }
            if (GUILayout.Button("Add Third Skill"))
            {
                script.AddThirdSkill();
            }
            if (GUILayout.Button("Stun Player"))
            {
                script.StunPlayer();
            }
        }
    }
}
