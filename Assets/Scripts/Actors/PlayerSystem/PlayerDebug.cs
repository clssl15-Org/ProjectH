using System.Collections;
using System.Collections.Generic;
using Actors.Monsters;
using Actors.PlayerSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.PlayerSystem
{
    public class PlayerDebug : MonoBehaviour
    {
#if PLAYER_DEBUG_MODE
        [SerializeField]
        private PlayerHealth playerHealth;
        [SerializeField]
        int damageAmount = 10;
        [SerializeField]
        private int relicKey;
        private Player player;

        private void Awake()
        {
            if (!playerHealth)
                playerHealth = FindAnyObjectByType<PlayerHealth>();

            player = GetComponent<Player>();
        }
        private void Start()
        {
            if (!playerHealth)
                playerHealth = FindAnyObjectByType<PlayerHealth>();
            //AddFirstSkill();
            //AddSecondSkill();
            //AddThirdSkill();
            //AddUltimate();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                KillAllEnemiesInStage();
            }
        }

        public void DamageToPlayer()
        {
            playerHealth.TakeDamage(damageAmount);
        }

        public void KillAllEnemiesInStage()
        {
            MonsterDamageReceiver[] monsters = FindObjectsByType<MonsterDamageReceiver>(FindObjectsSortMode.None);
            int killedCount = 0;
            const int instantKillDamage = 999999;

            foreach (MonsterDamageReceiver monster in monsters)
            {
                if (monster == null || !monster.Interactable)
                    continue;

                monster.TakeDamage(instantKillDamage);
                killedCount++;
            }

            Debug.Log($"[PlayerDebug] L key kill-all triggered. Monsters hit: {killedCount}", this);
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
        public void AddRelic()
        {
            RelicManager.Instance.AddRelic(relicKey, out _, false);
        }
#endif
    }

#if UNITY_EDITOR && PLAYER_DEBUG_MODE 
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
                script.AddRelic();
            }
        }
    }
#endif
}
