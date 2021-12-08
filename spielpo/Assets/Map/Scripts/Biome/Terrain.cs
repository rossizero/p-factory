using Map.Biome.Resources;
using UnityEngine;

namespace Map.Biome
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Biome/Terrain", order = 1)]
    public class Terrain : ScriptableObject
    {
        [SerializeField] public Feature[] terrainFeatures; //terrain specific features
        [SerializeField] public Texture2D texture;
        [SerializeField] public Resource[] resources; //terrain specific resources
        [SerializeField] [Range(0, 1)] public float resourceDensity; //how many resources
    }
}
