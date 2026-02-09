using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infrastructure;

namespace Actors.PlayerSystem
{
    public class Die : CharacterState
    {
        private bool isDone = true;
        public override void EnterBehaviour(float dt)
        {
            ResetState();
        }
        public override void UpdateBehaviour(float dt)
        {
            
        }
        private void ResetState()
        {
            isDone = false;
        }
    }
}