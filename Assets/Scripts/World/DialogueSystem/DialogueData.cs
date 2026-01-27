using System;
using UnityEngine;

namespace World
{
    [Serializable]
    public struct DialogueData
    {
        public Character Character;
        public string Dialogue;
        [Space]
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
