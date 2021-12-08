using Map.Biome.Resources;
using UnityEngine;

namespace Map.Biome
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Biome/Biome", order = 1)]
    public class Biome : ScriptableObject
    {
        [SerializeField] public Feature[] biomeFeatures; //biome wide features
        [SerializeField] public Terrain[] terrains; //bottom to top
        [SerializeField] [Range(0, 100)] public int maxFeaturesPerTile = 5;
        [SerializeField] [Range(0, 100)] public int minFeaturesPerTile = 0;
        [SerializeField] [Range(0, 1)] public float generalFeatureDensity = 0.3f;
        [SerializeField] public Resource[] resources; //biome wide resources
        [SerializeField] [Range(0, 1)] public float biomeResourceDensity; //how many resources
        public Texture2D[] Textures
        {
            get
            {
                Texture2D[] ret = new Texture2D[terrains.Length];
                for (int i = 0; i < terrains.Length; i++)
                {
                    ret[i] = terrains[i].texture;
                }
                return ret;
            }
        }
        [Range(-100f, 100f)]
        [SerializeField] public float minHeight;
        [Range(-100f, 100f)]
        [SerializeField] public float maxHeight;
        [SerializeField] string biomeName;
        [Range(1, 100)]
        [SerializeField] public int biomeSize = 25;

        public Feature[] getFeaturesOfTerrain(int terrainId)
        {
            int l = biomeFeatures.Length + terrains[terrainId].terrainFeatures.Length;
            Feature[] ret = new Feature[l];
            for (int i = 0; i < l; i++)
            {
                if (i < biomeFeatures.Length)
                {
                    ret[i] = biomeFeatures[i];
                }
                else
                {
                    ret[i] = terrains[terrainId].terrainFeatures[i - biomeFeatures.Length];
                }
            }
            return ret;
        }

        public Resource[] getResourcesOfTerrain(int terrainId)
        {
            int l = resources.Length + terrains[terrainId].resources.Length;
            Resource[] ret = new Resource[l];
            for (int i = 0; i < l; i++)
            {
                if (i < resources.Length)
                {
                    ret[i] = resources[i];
                }
                else
                {
                    ret[i] = terrains[terrainId].resources[i - resources.Length];
                }
            }
            return ret;
        }
    }
}
