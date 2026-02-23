using UnityEngine;

namespace World
{
    [CreateAssetMenu(fileName = "Character Info", menuName = "Project H/Character Info")]
    public class CharacterInfoSO : ScriptableObject
    {
        [field: SerializeField] public Character Character { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public Sprite Portrait { get; set; }
    }
}
