using UnityEngine;

[CreateAssetMenu(fileName = "New Player Stats", menuName = "Stats/Player Stats")]
public class PlayerStatsSO : ScriptableObject
{
    public int maxHealth = 100;
    public int attackPower = 10;
    public float moveSpeed = 5f;
}
