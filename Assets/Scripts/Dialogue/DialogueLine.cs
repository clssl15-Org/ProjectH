using System;
using UnityEngine;
using World;

namespace Dialogue
{
    [Serializable]
    public struct DialogueLine
    {
        public Character Character;
        public string Dialogue;
        [SerializeField] private DialogueOverrides _overrides;

        public readonly string NameOverride => _overrides.Name;
        public readonly Sprite PortraitOverride => _overrides.Portrait;


        [Serializable]
        public struct DialogueOverrides
        {
            public string Name;
            public Sprite Portrait;
        }
    }
}
