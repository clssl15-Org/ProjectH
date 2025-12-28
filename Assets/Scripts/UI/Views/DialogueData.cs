using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public enum Character
    {
        Player,
        Rubiel_Small,
        Rubiel_Big,
        Belia,
        DarkTherion,
        Werbellion,
    }

    [Serializable]
    public struct DialogueData
    {
        public Character Character;
        public string Name;
        public string Dialogue;
    }
}
