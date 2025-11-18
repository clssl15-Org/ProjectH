using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actors.Monsters.Stage3Bosses
{
    internal interface ITwinBoss : IMonsterInternal
    { 
        const string IsAwaken = "IsAwaken";

        void Die();
    }
}
