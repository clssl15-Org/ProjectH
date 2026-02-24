using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

namespace Actors.PlayerSystem
{
    public class FallingJump : CharacterState
    {
        private Tilemap oneWayPlatformTilemap;
        public override bool CheckEnterTransition(CharacterState fromState)
        {
            oneWayPlatformTilemap = Player.GetComponentInChildren<PlatformDetector>().GetPlatformManager().OneWayPlatformTilemap;

            if (oneWayPlatformTilemap != null) return true;
            else return false;
        }
        public override void CheckExitTransition()
        {
            CharacterStateController.EnqueueTransition<NormalMovement>();
        }
        public override void EnterBehaviour(float dt)
        {
            StartCoroutine(LeavePlatform());
        }

        private IEnumerator LeavePlatform()
        {
            Collider2D oneWayPlatformCollider = Player.GetComponentInChildren<PlatformDetector>().GetPlatformManager().OneWayPlatformTilemap.GetComponent<Collider2D>();

            // 플레이어와 해당 플랫폼 사이의 총돌만 무시합니다.
            Physics2D.IgnoreCollision(CharacterActor.Collider, oneWayPlatformCollider, true);

            yield return new WaitForSeconds(0.2f);

            Physics2D.IgnoreCollision(CharacterActor.Collider, oneWayPlatformCollider, false);
        }
        public override void UpdateBehaviour(float dt)
        {

        }
    }
}
