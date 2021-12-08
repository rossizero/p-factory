using UnityEngine;

namespace Map.Biome
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Biome/Feature", order = 1)]
    public class Feature : ScriptableObject
    {
        [SerializeField] public GameObject feature;
        [SerializeField] [Range(0, 1)] public float density;
    }
}
