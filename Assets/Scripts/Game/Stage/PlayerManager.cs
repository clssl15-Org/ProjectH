using System;
using Actors;
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
