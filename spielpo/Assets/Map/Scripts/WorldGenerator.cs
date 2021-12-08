using Map.Tile;
using System.Collections.Generic;
using UnityEngine;
using Map.Biome;
using Map.Biome.Resources;

namespace Map
{
    public class WorldGenerator : MonoBehaviour
    {
        private static Dictionary<World, int[]> biomeWeights = new Dictionary<World, int[]>();
        private static Dictionary<World, int> biomeWeightsSums = new Dictionary<World, int>();

        /// <summary>
        /// Fill up weights for the list of biomes of a world. (Used to pick higher weighted biomes more often)
        /// Currently we use the biomeSize as the weight for each biome of this world.
        /// </summary>
        /// <param name="biomes"></param>
        private static void prepare(World world)
        {
            int i = 0;
            biomeWeights[world] = new int[world.biomeList.biomeList.Length];
            biomeWeightsSums[world] = 0;

            foreach (Biome.Biome b in world.biomeList.biomeList)
            {
                biomeWeights[world][i++] = b.biomeSize;
                biomeWeightsSums[world] += b.biomeSize;
            }
        }

        /// <summary>
        /// Creates a 'unique' integer for a pair of numbers. Formular from Matthew Szudzik.
        /// Rounded to int, because it is only being used to initialize unity's random function
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int convert2dto1dSeed(float a, float b)
        {
            return Mathf.RoundToInt(a >= b ? a * a + a + b : a + b * b);
        }

        /// <summary>
        /// Using the voronio noise to randomly place biomes and assign the corresponding one to the given tile
        /// Got some information from this link, implementing it as a shader: https://www.ronja-tutorials.com/post/028-voronoi-noise/
        /// 
        /// Calculating biome borders, to smooth out the cuts created by the different biome settings
        /// Place features onto the tile, provided by its prior selected biome
        /// </summary>
        /// <param name="world">World to generate in (make sure the biomeList is filled up with at least 1 biome)</param>
        /// <param name="tile">The tile to apply World generation stuff to</param>
        public static void generate(World world, HexTile tile)
        {
            //remember old random state
            UnityEngine.Random.State state = UnityEngine.Random.state;

            if (!biomeWeightsSums.ContainsKey(world))
            {
                prepare(world);
            }
            Vector3 position = tile.transform.localPosition;
            float voronoiCellSize = HexMetrics.biomsize;

            //cell coordinate
            int x = Mathf.RoundToInt((float)position.x / (float)voronoiCellSize);
            int y = Mathf.RoundToInt((float)position.z / (float)voronoiCellSize);

            //reference point in this cell
            UnityEngine.Random.InitState(convert2dto1dSeed(x, y));
            float baseCellX = UnityEngine.Random.Range(x * voronoiCellSize, x * voronoiCellSize + voronoiCellSize);
            float baseCellY = UnityEngine.Random.Range(y * voronoiCellSize, y * voronoiCellSize + voronoiCellSize);

            //initial closest cell values
            float minDistToCell = float.MaxValue;
            float closestCellX = baseCellX;
            float closestCellY = baseCellY;
            Vector2 toClosestCell = new Vector2();

            //get the closest cell for this position
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int cellX = x + i;
                    int cellY = y + j;

                    //reference point in other cell
                    UnityEngine.Random.InitState(convert2dto1dSeed(cellX, cellY));
                    float otherCellX = UnityEngine.Random.Range(cellX * voronoiCellSize, cellX * voronoiCellSize + voronoiCellSize);
                    float otherCellY = UnityEngine.Random.Range(cellY * voronoiCellSize, cellY * voronoiCellSize + voronoiCellSize);

                    //distance to other cell
                    float distSquared = Mathf.Pow((otherCellX - position.x), 2) + Mathf.Pow((otherCellY - position.z), 2);
                    if (distSquared < minDistToCell)
                    {
                        minDistToCell = distSquared;
                        closestCellX = otherCellX;
                        closestCellY = otherCellY;
                        toClosestCell = new Vector2(otherCellX - position.x, otherCellY - position.z);
                    }
                }
            }

            //get the distance to the biomes borders
            float distanceToEdge = float.MaxValue;
            Vector2 secondClosestCellGlobal = new Vector2(closestCellX, closestCellY);

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int cellX = x + i;
                    int cellY = y + j;

                    //reference point in other cell
                    UnityEngine.Random.InitState(convert2dto1dSeed(cellX, cellY));
                    float otherCellX = UnityEngine.Random.Range(cellX * voronoiCellSize, cellX * voronoiCellSize + voronoiCellSize);
                    float otherCellY = UnityEngine.Random.Range(cellY * voronoiCellSize, cellY * voronoiCellSize + voronoiCellSize);
                    Vector2 toCell = new Vector2(otherCellX - position.x, otherCellY - position.z);

                    float diffToClosestCell = Mathf.Abs((otherCellX - closestCellX)) + Mathf.Abs((otherCellY - closestCellY));
                    if (diffToClosestCell > 0.1)
                    {
                        //get the position of the centerpoint between both cells
                        Vector2 toCenter = (toClosestCell + toCell);
                        toCenter = new Vector2(toCenter.x * 0.5f, toCenter.y * 0.5f);
                        //"move" the sample point onto the connection line between both cells
                        //then calculate the distance between center and that point
                        Vector2 cellDiff = (toClosestCell - toCell).normalized;
                        float edgeDist = Vector2.Dot(toCenter, cellDiff);
                        if (distanceToEdge > Mathf.Abs(edgeDist))
                        {
                            distanceToEdge = Mathf.Abs(edgeDist);
                            secondClosestCellGlobal = new Vector2(otherCellX, otherCellY);
                        }
                    }
                }
            }

            //get the biome for this cell
            Biome.Biome biome = getBiomeAt(world, closestCellX, closestCellY);
            float elevation = getHeightAt(biome, position.x, position.z);

            //if tile is at the border of its chunk, reduce its elevation
            float factor = 1f;
            if (distanceToEdge < HexMetrics.biomeBorderWeight)
            {
                //Biome of second closest cell
                Biome.Biome biome2 = getBiomeAt(world, secondClosestCellGlobal.x, secondClosestCellGlobal.y);
                if (!biome2.Equals(biome))
                {
                    factor = (distanceToEdge / HexMetrics.biomeBorderWeight);
                }
            }

            tile.Elevation = (int)(elevation * factor);

            //set the terrainTypeIndex
            (int, int) indices = getTerrainTypeIndex(world, biome, position.x, position.z, factor);
            tile.TerrainTypeIndex = indices.Item2;

            //spawn some terrain features onto the tile (if there are some)
            if (biome.terrains[indices.Item1].terrainFeatures.Length + biome.biomeFeatures.Length > 0)
            {
                //initialize the random state to tile position
                UnityEngine.Random.InitState((convert2dto1dSeed(tile.Coordinate.X, tile.Coordinate.Z)));
                Feature[] features = biome.getFeaturesOfTerrain(indices.Item1);

                if (UnityEngine.Random.value <= biome.generalFeatureDensity) //biomes with higher density will more often let a tile have features
                {


                    for (int i = 0; i < UnityEngine.Random.value * (biome.maxFeaturesPerTile - biome.minFeaturesPerTile) + biome.minFeaturesPerTile; i++) //how many features should be placed onto that tile
                    {
                        int featureIndex = UnityEngine.Random.Range(0, features.Length);
                        if (UnityEngine.Random.value <= features[featureIndex].density) //do not always place the feature
                        {
                            //Instantiate has to be called from main thread. Let's just use the main thread queue of the given world for that
                            float lim = HexMetrics.solidFactor * HexMetrics.innerRadius / 1.5f;
                            float randX = UnityEngine.Random.Range(-lim, lim);
                            float randZ = UnityEngine.Random.Range(-lim, lim);
                            Quaternion randRotation = Quaternion.Euler(0, UnityEngine.Random.value * 360, 0);

                            world.toDoInMainThread.Enqueue(() =>
                            {
                                //create the feature gameobject and initialize its position and rotation
                                GameObject feature = Instantiate(features[featureIndex].feature);
                                feature.transform.parent = tile.transform;
                                feature.transform.rotation = randRotation;
                                MeshRenderer renderer = feature.GetComponent<MeshRenderer>();
                                float h = 0;
                                if (renderer != null)
                                    h = renderer.bounds.size.y / 2;
                                feature.transform.localPosition = new Vector3(randX, h, randZ);
                                tile.checkChildGameobjects();
                            });
                        }
                    }
                }
            }

            //spawn resources onto the tile
            if (biome.resources.Length + biome.terrains[indices.Item1].resources.Length > 0)
            {
                UnityEngine.Random.InitState((convert2dto1dSeed(tile.Coordinate.X, tile.Coordinate.Z)));
                Resource[] resources = biome.getResourcesOfTerrain(indices.Item1);

                if (UnityEngine.Random.value <= biome.biomeResourceDensity) //biomes with higher density will have more resources
                {
                    int resourcesIndex = UnityEngine.Random.Range(0, resources.Length);
                    float density = biome.biomeResourceDensity;

                    if (resourcesIndex >= biome.resources.Length)
                    {
                        density = biome.terrains[indices.Item1].resourceDensity;
                    }
                    if (UnityEngine.Random.value <= density) //do not always place the feature
                    {
                        //create the feature gameobject and initialize its position and rotation
                        if (resources[resourcesIndex].resourceVisualisation != null)
                        {
                            Quaternion randomRotation = Quaternion.Euler(0, UnityEngine.Random.value * 360, 0);
                            world.toDoInMainThread.Enqueue(() =>
                            {
                                GameObject resource = Instantiate(resources[resourcesIndex].resourceVisualisation);
                                resource.transform.parent = tile.transform;
                                resource.transform.rotation = randomRotation;
                                resource.transform.localPosition = new Vector3(0, 0, 0);

                                tile.checkChildGameobjects();
                            });
                        }
                        tile.resource = resources[resourcesIndex].resourceType;
                        tile.resourceQuality = resources[resourcesIndex].resourceQualities[UnityEngine.Random.Range(0, resources[resourcesIndex].resourceQualities.Length)];
                    }
                }
            }
            //set old random state
            UnityEngine.Random.state = state;
        }

        /// <summary>
        /// Get the biome at x and y for given world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Biome at x and y for given world</returns>
        private static Biome.Biome getBiomeAt(World world, float x, float y)
        {
            UnityEngine.Random.InitState((convert2dto1dSeed(x, y)));

            //get the index of the closest biome of the list of possible biomes set for this World
            int tmp = UnityEngine.Random.Range(0, biomeWeightsSums[world]);
            for (int i = 0; i < biomeWeights[world].Length; i++)
            {
                if (tmp > biomeWeights[world][i])
                {
                    tmp -= biomeWeights[world][i];
                }
                else
                {
                    tmp = i;
                    break;
                }
            }
            return world.biomeList.biomeList[tmp];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>height at x and y in given biome</returns>
        private static float getHeightAt(Biome.Biome biome, float x, float y)
        {
            //Basic PerlinNoise elevation calculation
            //takes a value between 0 and n with n := number of different terrain type sorted by height
            float tmp2 = Mathf.PerlinNoise(
                (x + HexMetrics.perlinSeed.x) * HexMetrics.noiseScale,
                (y + HexMetrics.perlinSeed.y) * HexMetrics.noiseScale);
            float sample = Mathf.Min(Mathf.Clamp01(tmp2), 0.999999f) * (biome.Textures.Length);
            //make as many elevation steps, as there are textures in the biome
            float elevation = (biome.minHeight + sample * (Mathf.Abs(biome.maxHeight - biome.minHeight) / (biome.Textures.Length)));
            return elevation;
        }



        /// <summary>
        /// Returns both, the terrain index for the given biome and also the global index of the texture assiciated with that terrain
        /// </summary>
        /// <param name="world"></param>
        /// <param name="biome"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="factor">how much has the height been reduced by some other operation</param>
        /// <returns>(biome terrain index, global terrain index)</returns>

        private static (int, int) getTerrainTypeIndex(World world, Biome.Biome biome, float x, float y, float factor)
        {
            //Basic PerlinNoise elevation calculation
            //takes a value between 0 and n with n := number of different terrain type sorted by height
            float tmp2 = Mathf.PerlinNoise(
                (x + HexMetrics.perlinSeed.x) * HexMetrics.noiseScale,
                (y + HexMetrics.perlinSeed.y) * HexMetrics.noiseScale);
            float sample = Mathf.Min(Mathf.Clamp01(tmp2 * factor), 0.999999f) * (biome.terrains.Length);
            return /*distance < HexMetrics.biomeBorderWeight ? (0, 0) :*/ (Mathf.FloorToInt(sample), world.biomeList.getIndexOfTerrain(biome, Mathf.FloorToInt(sample)));
        }
    }
}
