using System.Collections.Generic;
using UnityEngine;
/*Nadelwald
Laubwald
Rote Wüste -> mit vielen steilen einzelnen Tiles
Sümpfe -> mit vielen Löchern
Verbrannter wald (COAL)
Kitsch mit Plumbusore1 (selten)
Schneelandschaft mit und ohne Bäume (Nadelbäume)
Blumenwiese (Farmland resource) done (fehlen features)
Felsenwüste mit Steinvorkommen (features raus)
Ocean (FISH am shore) done
*/
namespace Map.Biome
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Biome/Biomes", order = 1)]
    public class Biomes : ScriptableObject
    {
        [SerializeField] public Biome[] biomeList;

        public Texture2D[] TextureList
        {
            get
            {
                List<Texture2D> textures = new List<Texture2D>();
                foreach (Biome b in biomeList)
                {
                    if (b.Textures.Length > 0)
                        textures.AddRange(b.Textures);
                }
                return textures.ToArray();
            }
        }

        /// <summary>
        /// Get the global index of a texture for given biome
        /// </summary>
        /// <param name="biome">biome</param>
        /// <param name="terrainIndex">index of terrain in given biome</param>
        /// <returns>global index of this texture</returns>
        public int getIndexOfTerrain(Biome biome, int terrainIndex)
        {
            int ret = 0;
            foreach (Biome b in biomeList)
            {
                if (!b.Equals(biome))
                {
                    ret += b.Textures.Length;
                }
                else
                {
                    break;
                }
            }
            return ret + terrainIndex;
        }
    }
}