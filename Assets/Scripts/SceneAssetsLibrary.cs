using UnityEngine;

public class SceneAssetsLibrary : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material solidColor;

    public Material SolidColor => Instantiate(solidColor);
}
