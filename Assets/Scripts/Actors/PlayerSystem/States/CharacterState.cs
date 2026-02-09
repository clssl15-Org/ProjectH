using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actors.PlayerSystem
{
    public enum SkillType
    {
        None,
        Undefined,
        RushStabbing,
        RangedAttack,
        StrongAttack,
    }

    public abstract class CharacterState : MonoBehaviour
    {
        // 어떤 스킬인지
        [field: SerializeField]
        public SkillType SkillType { get; protected set; } = SkillType.Undefined;

        [Space]
        [SerializeField]
        RuntimeAnimatorController runtimeAnimatorController = null;
        public RuntimeAnimatorController RuntimeAnimatorController => runtimeAnimatorController;
        public CharacterActor CharacterActor { get; private set; }
        CharacterBrain CharacterBrain = null;
        public DamageRoulette DamageRoulette { get; private set; }

        public CharacterActions CharacterActions => CharacterBrain.CharacterActions;
        public CharacterStateController CharacterStateController { get; private set; }
        public SkillManager SkillManager { get; private set; }
        public Player Player { get; private set; }
        protected virtual void Awake()
        {
            // find the CharacterBrain component in the root of the hierarchy.
            // If there are multiple target components under the root, it may not work correctly.
            CharacterActor = this.transform.root.GetComponentInChildren<CharacterActor>();
            CharacterBrain = this.transform.root.GetComponentInChildren<CharacterBrain>();
            DamageRoulette = this.transform.root.GetComponentInChildren<DamageRoulette>();
            SkillManager = this.transform.root.GetComponentInChildren<SkillManager>();
            CharacterStateController = this.transform.root.GetComponentInChildren<CharacterStateController>();
            Player = this.transform.root.GetComponentInChildren<Player>();
        }
        protected virtual void Start()
        {
        }
        // This method runs once when the state has entered the state machine.
        public virtual void EnterBehaviour(float dt)
        {
        }

        // This methods runs before the main Update method.
        public virtual void PreUpdateBehaviour(float dt)
        {
        }
        // This method runs frame by frame, and should be implemented by the derived state class.

        public abstract void UpdateBehaviour(float dt);

        // This methods runs after the main Update method.
        public virtual void PostUpdateBehaviour(float dt)
        {
        }

        // This method runs once when the state has exited the state machine.
        public virtual void ExitBehaviour(float dt)
        {
        }

        // Checks if the required conditions to exit this state are true. If so it returns the desired state (null otherwise). After this the state machine will
        // proceed to evaluate the "enter transition" condition on the target state.
        public virtual void CheckExitTransition()
        {
        }

        // Checks if the required conditions to enter this state are true. If so the state machine will automatically change the current state to the desired one.
        public virtual bool CheckEnterTransition(CharacterState fromState)
        {
            return true;
        }

        // 
        public virtual void UpdateBufferedActions(float dt)
        {
        }
    }
}
