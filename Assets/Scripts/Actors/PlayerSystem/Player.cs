using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.PlayerSystem
{
    public class Player : MonoBehaviour
    {
        public PlayerStatsSO playerStats;
        public bool Invincible
        {
            get => invincible;
            set => invincible = value;
        }
        private bool invincible = false;
    }
}
