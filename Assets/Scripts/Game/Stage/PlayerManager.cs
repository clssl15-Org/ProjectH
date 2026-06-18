using System;
using Actors;
using BlackThunder.BlackboxSystem;
using UnityEngine;

namespace Game.Stage
{
    public class PlayerManager : MonoBehaviour
    {
        // Internal
        private IPlayer _player;
        private bool _isDestroyed = false;
        private BlackboxHandle _blackbox;


        // Content
        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).Construct("플레이어 매니저 초기화를 시작합니다.", out _blackbox);
        }

        public bool Register(IPlayer player)
        {
            using var _ = _blackbox.Scope("플레이어를 등록합니다.").With(player);

            if (player == null || !player.gameObject)
            {
                throw new ArgumentException(Ctx($"Register: ��ȿ���� ���� ���� '{((player != null && player.gameObject) ? player.gameObject.name : "null")}'��(��) �ԷµǾ����ϴ�."),
                    nameof(player));
            }


            if (_player != null)
            {
                if (_player != player)
                {
                    throw new ArgumentException(Ctx($"Register: �̹� ���� �÷��̾� '{_player.name}'��(��) �����ϱ� ������ �� �÷��̾� '{player.name}'��(��) ����� �� �����ϴ�."),
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
            using var _ = _blackbox.Scope("등록된 플레이어를 정리합니다.");

            if (_isDestroyed) return;
            _isDestroyed = true;


            if (_player != null && _player.gameObject)
            {
                Destroy(_player.gameObject);
            }

            _player = null;
        }

        private string Ctx(string message) => $"[{nameof(PlayerManager)}] {message}";
    }
}
