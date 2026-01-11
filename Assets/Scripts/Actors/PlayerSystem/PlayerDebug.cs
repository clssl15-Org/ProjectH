using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
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
            AddFirstSkill();
            AddSecondSkill();
            AddThirdSkill();
            AddUltimate();
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
        public void AddUltimate()
        {
            Ultimate ultimateSkill = player.StatesGO.GetComponent<Ultimate>();
            player.SkillManager.AddUltimateSkill(ultimateSkill);
        }
        public void AddRelic(int key)
        {
            RelicManager.Instance.AddRelic(key, false);
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
            if(GUILayout.Button("Add Ultimate Skill"))
            {
                script.AddUltimate();
            }
            if (GUILayout.Button("Stun Player"))
            {
                script.StunPlayer();
            }
            if (GUILayout.Button("Add Relic"))
            {
                script.AddRelic(5);
            }
        }
    }
}
