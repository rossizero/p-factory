using UnityEngine;

namespace Map.Biome.Resources
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Biome/Resource", order = 1)]
    public class Resource : ScriptableObject
    {
        [SerializeField] public GameObject resourceVisualisation; // if needed
        [SerializeField] public ResourceQuality[] resourceQualities; // will choose randomly between all given qualities
        [SerializeField] public ResourceType resourceType;
    }
}
