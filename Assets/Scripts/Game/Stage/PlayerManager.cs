using System;
using Actors;
using BlackboxSystem;
using UnityEngine;

namespace Game.Stage
{
    public class PlayerManager : MonoBehaviour
    {
        // Internal
        private IPlayer _player;
        private bool _isDestroyed = false;


        // Content
        public bool Register(IPlayer player)
        {
            if (player == null || !player.gameObject)
            {
                throw new ArgumentException(BlackboxHandle.Of(this).CrashExport(
                    Ctx($"Register: 유효하지 않은 인자 '{((player != null && player.gameObject) ? player.gameObject.name : "null")}'이(가) 입력되었습니다.")),
                    nameof(player));
            }

            using var _ = BlackboxHandle.Of(this).ExertScope(player, "플레이어 등록");

            if (_player != null)
            {
                if (_player != player)
                {
                    throw new ArgumentException(BlackboxHandle.Of(this).CrashExport(
                        Ctx($"Register: 이미 기존 플레이어 '{_player.name}'이(가) 존재하기 때문에 새 플레이어 '{player.name}'을(를) 등록할 수 없습니다.")),
                        nameof(player));
                }
                else
                    return false;
            }

            _player = player;
            player.Destroying += () =>
            {
                if (!_isDestroyed)
                    _player = null;
            };

            return true;
        }

        private void OnDestroy() => Destroy();
        internal void Destroy()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;

            using var _ = BlackboxHandle.Of(this).WriteScope("Destroy");

            if (_player != null && _player.gameObject)
            {
                BlackboxHandle.Of(this).Exert(_player, "플레이어 Destroy");
                Destroy(_player.gameObject);
            }

            _player = null;
        }

        private string Ctx(string message) => $"[{nameof(PlayerManager)}] {message}";
    }
}
