using System;
using Actors;
using UnityEngine;

namespace Game.Stage
{
    public class PlayerManager : MonoBehaviour
    {
        // Internal
        private IPlayer _player;


        // Content
        public bool Register(IPlayer player)
        {
            if (player == null || !player.gameObject)
                throw new ArgumentException(
                    Ctx($"유효하지 않은 인자 '{player?.gameObject.name ?? "null"}'이(가) 입력되었습니다."),
                    nameof(player));

            if (_player != null)
            {
                if (_player != player)
                    throw new ArgumentException(
                        Ctx($"이미 기존 플레이어 '{_player.name}'이(가) 존재하기 때문에 새 플레이어 '{player.name}'을(를) 등록할 수 없습니다."),
                        nameof(player));
                else
                    return false;
            }

            _player = player;
            player.Destroyed += () => _player = null;

            return true;
        }

        internal void Destroy()
        {
            if (_player != null && _player.gameObject)
                Destroy(_player.gameObject);

            _player = null;
        }

        private string Ctx(string message) => $"[{nameof(MonsterManager)}] {message}";
    }
}
