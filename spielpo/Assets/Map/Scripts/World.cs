using System.Collections.Generic;
using UnityEngine;
using Map.Tile;
using Map.Chunk;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Collections.Concurrent;
using Map.Biome;

namespace Map
{
    public class World : MonoBehaviour
    {
        //prefabs to be instatiated
        public Text cellLabelPrefab;
        public HexTile cellPrefab;
        public HexGridChunk chunkPrefab;

        //how many chunks should be generated around a UpdateChunks()-Request
        public int renderDistance = 4;

        //Queue with jobs to be done in MainThread
        public readonly ConcurrentQueue<Action> toDoInMainThread = new ConcurrentQueue<Action>();

        Dictionary<(int, int), HexGridChunk> loadedChunks; //to render the mesh of chunk

        public Texture2D noiseSource; //random noise used for small differences during map rendering

        //a unique index, hextiles retreive here
        //used as index of cellTextureData
        private static int index = 0;

        [SerializeField] public Biomes biomeList;

        public static int nextIndex
        {
            get
            {
                return index++;
            }
        }

        void Awake()
        {
            HexMetrics.noiseSource = noiseSource;
            loadedChunks = new Dictionary<(int, int), HexGridChunk>();
        }
        private void Start()
        {
            UpdateChunks(new Vector3(0f, 0f, 0f));
        }
        /// <summary>
        /// Works through the MainThread queue.
        /// </summary>
        void Update()
        {
            if (!toDoInMainThread.IsEmpty)
            {
                while (toDoInMainThread.TryDequeue(out var action))
                {
                    action?.Invoke();
                }
            }

            // comment to disable chunks appearing around main camera
            UpdateChunks();
        }

        /// <summary>
        /// Function to create chunks around a scene position
        /// </summary>
        /// <param name="position"></param>
        public void UpdateChunks(Vector3 position)
        {
            HexGridChunk currentChunk = GetChunkAtPosition(position, true);
            for (int x = -renderDistance; x <= renderDistance; x++)
            {
                for (int z = -renderDistance; z <= renderDistance; z++)
                {
                    if (!loadedChunks.ContainsKey((currentChunk.x + x, currentChunk.z + z)))
                    {
                        loadedChunks[(currentChunk.x + x, currentChunk.z + z)] = GetChunk(currentChunk.x + x, currentChunk.z + z, true);
                    }
                }
            }
        }

        /// <summary>
        /// Threaded: Makes neighbour references for each tile in given chunk.
        /// Put in Main Thread queue: Refresh calls of all neighbour chunks, to triangulate the chunkborders correctly.
        /// </summary>
        /// <param name="chunk"></param>
        void makeNeighbours(HexGridChunk chunk)
        {
            new Thread(delegate ()
            {
                foreach (HexTile tile in chunk.tiles.Values)
                {
                    for (HexDirection dir = HexDirection.NE; dir <= HexDirection.NW; dir++)
                    {
                        HexTile neighbour = GetTile(tile.Coordinate.GetNeighbour(dir));
                        if (neighbour != null)
                        {
                            tile.SetNeighbour(dir, neighbour);
                        }
                    }
                }


                toDoInMainThread.Enqueue(() =>
                {
                    // re-render direct neighbour chunks
                    if (loadedChunks.ContainsKey((chunk.x + 1, chunk.z)))
                        loadedChunks[(chunk.x + 1, chunk.z)].Refresh();
                    if (loadedChunks.ContainsKey((chunk.x - 1, chunk.z)))
                        loadedChunks[(chunk.x - 1, chunk.z)].Refresh();
                    if (loadedChunks.ContainsKey((chunk.x, chunk.z + 1)))
                        loadedChunks[(chunk.x, chunk.z + 1)].Refresh();
                    if (loadedChunks.ContainsKey((chunk.x, chunk.z - 1)))
                        loadedChunks[(chunk.x, chunk.z - 1)].Refresh();

                    // and diagonal neighbours
                    if (loadedChunks.ContainsKey((chunk.x + 1, chunk.z + 1)))
                        loadedChunks[(chunk.x + 1, chunk.z + 1)].Refresh();
                    if (loadedChunks.ContainsKey((chunk.x - 1, chunk.z - 1)))
                        loadedChunks[(chunk.x - 1, chunk.z - 1)].Refresh();
                    if (loadedChunks.ContainsKey((chunk.x - 1, chunk.z + 1)))
                        loadedChunks[(chunk.x - 1, chunk.z + 1)].Refresh();
                    if (loadedChunks.ContainsKey((chunk.x + 1, chunk.z - 1)))
                        loadedChunks[(chunk.x + 1, chunk.z - 1)].Refresh();
                });

            }).Start();
        }

        /// <summary>
        /// Test function to create chunks around the main camera
        /// </summary>
        public void UpdateChunks()
        {
            UpdateChunks(Camera.main.transform.position);
        }

        /// <summary>
        /// To get the chunk at given world coordinates
        /// </summary>
        /// <param name="position">world coordinates of the chunk</param>
        /// <param name="loadIfNotInstantiated">instantiate if not done yet or return null</param>
        /// <returns>null or the given chunk</returns>
        HexGridChunk GetChunkAtPosition(Vector3 position, bool loadIfNotInstantiated = false)
        {

            position = transform.InverseTransformPoint(position);
            HexCoordinates coordinates = HexCoordinates.FromPosition(position);
            return GetChunkAtPosition(coordinates, loadIfNotInstantiated);
        }

        /// <summary>
        /// To get the chunk at given HexCoordinates
        /// </summary>
        /// <param name="coordinates">HexCoordinates of the chunk</param>
        /// <param name="loadIfNotInstantiated">instantiate if not done yet or return null</param>
        /// <returns>null or the given chunk</returns>
        HexGridChunk GetChunkAtPosition(HexCoordinates coordinates, bool loadIfNotInstantiated = false)
        {
            //convert to "squared" coordinates
            HexCoordinates converted = HexCoordinates.ToOffsetCoordinates(coordinates.X, coordinates.Z);

            //tranformation to chunk coordinates (sorry it was late)
            int x = 0, z = 0;
            if (converted.X < 0)
            {
                x = (int)(Mathf.Sign(converted.X) * Mathf.Ceil(Mathf.Abs((float)converted.X / HexMetrics.chunkSizeX)));
            }
            else
            {
                x = (int)(Mathf.Sign(converted.X) * Mathf.Ceil(Mathf.Abs(converted.X / HexMetrics.chunkSizeX)));
            }
            if (converted.Z < 0)
            {
                z = (int)(Mathf.Sign(converted.Z) * Mathf.Ceil(Mathf.Abs((float)converted.Z / HexMetrics.chunkSizeZ)));
            }
            else
            {
                z = (int)(Mathf.Sign(converted.Z) * Mathf.Ceil(Mathf.Abs(converted.Z / HexMetrics.chunkSizeZ)));
            }

            //actually retrieve or instantiate chunk
            HexGridChunk chunk = GetChunk(x, z, loadIfNotInstantiated);
            return chunk;
        }

        /// <summary>
        /// To get a tile at certain world position
        /// </summary>
        /// <param name="position">position of the tile in the scene</param>
        /// <param name="loadIfNotInstantiated">instantiate if not done yet or return null</param>
        /// <returns>null or the given tile</returns>
        public HexTile GetTile(Vector3 position, bool loadIfNotInstantiated = false)
        {
            HexCoordinates coordinates = HexCoordinates.FromPosition(position);
            return GetTile(coordinates, loadIfNotInstantiated);
        }

        /// <summary>
        /// To get a tile at certain HexCoordinates
        /// </summary>
        /// <param name="coordinates">Coordinates of the tile</param>
        /// <param name="loadIfNotInstantiated">instantiate if not done yet or return null</param>
        /// <returns>null or the given tile</returns>
        public HexTile GetTile(HexCoordinates coordinates, bool loadIfNotInstantiated = false)
        {
            if (GetChunkAtPosition(coordinates, loadIfNotInstantiated) != null)
            {
                HexGridChunk chunk = GetChunkAtPosition(coordinates, loadIfNotInstantiated);
                return GetChunkAtPosition(coordinates, loadIfNotInstantiated).getTile(coordinates);
            }
            return null;
        }

        /// <summary>
        /// To get a chunk at given (chunk) position
        /// </summary>
        /// <param name="chunkX">x position of chunk</param>
        /// <param name="chunkZ">z position of chunk</param>
        /// <param name="loadIfNotInstantiated">instantiate if not done yet or return null</param>
        /// <returns>null or the given chunk</returns>
        HexGridChunk GetChunk(int chunkX, int chunkZ, bool loadIfNotInstantiated = false)
        {
            HexGridChunk chunk = null;
            if (loadedChunks.ContainsKey((chunkX, chunkZ)))
            {
                chunk = loadedChunks[(chunkX, chunkZ)];

            }
            if (chunk == null && loadIfNotInstantiated)
            {
                chunk = InstantiateChunk(chunkX, chunkZ);
            }
            return chunk;
        }

        /// <summary>
        /// Instantiates a new chunk (and it's tiles) at given chunk position
        /// </summary>
        /// <param name="chunkX">x position of chunk in HexMetrics.chunkSizeX steps</param>
        /// <param name="chunkZ">z position of chunk in HexMetrics.chunkSizeZ steps</param>
        /// <returns>freshly instantiated chunk filled with fresh tiles</returns>
        HexGridChunk InstantiateChunk(int chunkX, int chunkZ)
        {
            //Debug.LogWarning("Instantiate Chunk " + chunkX + " " + chunkZ);
            HexGridChunk chunk = Instantiate(chunkPrefab);
            chunk.setChunkCoordinates(chunkX, chunkZ);
            chunk.transform.parent = transform;
            chunk.cellShaderData.registerChunk(chunkX, chunkZ);
            loadedChunks[(chunk.x, chunk.z)] = chunk;

            for (int x = 0; x < HexMetrics.chunkSizeX; x++)
            {
                for (int z = 0; z < HexMetrics.chunkSizeZ; z++)
                {
                    HexTile tile = InstantiateTile(chunk, x, z);
                    chunk.AddCell(tile);
                }
            }
            makeNeighbours(chunk);
            return chunk;
        }

        /// <summary>
        /// Instatiates a new tile for given chunk
        /// </summary>
        /// <param name="chunk">Parent chunk of the tile</param>
        /// <param name="x">x coordinate in the chunk</param>
        /// <param name="z">z coordinate in the chunk</param>
        /// <returns>a freshly instantiated tile</returns>
        HexTile InstantiateTile(HexGridChunk chunk, int x, int z)
        {
            //transform x and z to scene coordinates for the actual gameobject
            Vector3 position;
            x += chunk.x * HexMetrics.chunkSizeX;
            z += chunk.z * HexMetrics.chunkSizeZ;
            position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
            position.y = 0f;
            position.z = z * (HexMetrics.outerRadius * 1.5f);

            //create GameObject, set position and parent
            HexTile cell = Instantiate<HexTile>(cellPrefab);
            cell.transform.localPosition = position;
            cell.transform.parent = chunk.transform;
            cell.Coordinate = HexCoordinates.FromOffsetCoordinates(x, z);

            //Fill up label
            Text label = Instantiate<Text>(cellLabelPrefab);
            label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);

            label.text = cell.Coordinate.ToStringOnSeparateLines();
            label.text = HexCoordinates.ToOffsetCoordinates(cell.Coordinate.X, cell.Coordinate.Z).ToStringOnSeparateLines();
            cell.uiRect = label.rectTransform;

            //set it's chunk and index (used to differentiate tiles in terrain shader)
            cell.setChunk(chunk);
            cell.index = World.nextIndex; //could of course be put somewhere else, I don't carrot all

            WorldGenerator.generate(this, cell);

            //cell.IncreaseVisibility();
            return cell;
        }
    }
}
