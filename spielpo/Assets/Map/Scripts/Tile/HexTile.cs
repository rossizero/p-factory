using Game;
using Map.Biome.Resources;
using Map.Chunk;
using Map.Shader;
using System.Collections.Generic;
using UnityEngine;

namespace Map.Tile
{
    [RequireComponent(typeof(TileData))]
    public class HexTile : MonoBehaviour
    {
        public HexCoordinates Coordinate { get; set; } //"hex" position of the cell
        public RectTransform uiRect; //display field for coordinates for example
        [SerializeField] HexTile[] neighbours;
        int elevation;  //elevation of the cell
        int visibility; //cell visible counter
        public int index; //a unique index
        private int terrainTypeIndex; //what terrain type does the cell have
        public HexGridChunk chunk { get; private set; } //the chunk of this cell
        public HexCellShaderData shaderData { get; set; } //a reference to the ShaderData of the world the tile is
        public bool IsExplored { get; private set; } //has the cell been explored so far
        public ResourceType resource { get; set; } = ResourceType.NONE;
        public ResourceQuality resourceQuality { get; set; }
        public TileData tileData => GetComponent<TileData>();
        public HexTile PointsTo => GetComponent<TileData>().PointsTo;

        /// <summary>
        /// Get List of Input Tiles aka neighbors which point on this Tile and can ship.
        /// </summary>
        public List<HexTile> GetInputTiles
        {
            get
            {
                List<HexTile> List =  new List<HexTile>();
                foreach(HexTile neighbor in GetNeighbours())
                {
                    if (neighbor.tileData.HasInfrastructure && (neighbor.PointsTo != null && neighbor.PointsTo.Equals(this)))
                        List.Add(neighbor);
                }
                return List;
            }
        }

        public bool LoopToSelf
        {
            get
            {
                HexTile start = PointsTo;
                int count = 0;
                while(start != null && start.tileData.HasInfrastructure)
                {
                    start = start.PointsTo;
                    if (start == null || !start.tileData.HasInfrastructure || count > 20)
                        return false;
                    else if (start.Equals(this))
                        return true;
                    count++;
                }
                return false;
            }
        }

        public int TerrainTypeIndex
        {
            get
            {
                return terrainTypeIndex;
            }
            set
            {
                if (terrainTypeIndex != value)
                {
                    terrainTypeIndex = value;
                    shaderData.RefreshTerrain(this);
                }
            }
        }
        public bool IsVisible //is the cell vible right now
        {
            get
            {
                return visibility > 0;
            }
        }

        public int Elevation
        {
            get
            {
                return elevation;
            }
            set
            {
                elevation = value;

                //add a random offset to the given value to make everything look more natural
                Vector3 position = transform.localPosition;
                position.y = value * HexMetrics.elevationStep;
                position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
                transform.localPosition = position;

                //update ui label position
                Vector3 uiPosition = uiRect.localPosition;
                uiPosition.z = -position.y;
                uiRect.localPosition = uiPosition;
            }
        }

        /// <summary>
        /// Sets the chunk of the cell
        /// </summary>
        /// <param name="chunk">chunk the cell lies in</param>
        public void setChunk(HexGridChunk chunk)
        {
            this.chunk = chunk;
            shaderData = chunk.cellShaderData;
        }

        public float WaterSurfaceY //where to place water
        {
            get
            {
                return (HexMetrics.waterElevationOffset);// + 2 * HexMetrics.elevationStep;
            }
        }

        /// <summary>
        /// To call if this cell can be seen by a unit or so
        /// </summary>
        public void IncreaseVisibility()
        {
            visibility += 1;
            if (visibility == 1)
            {
                IsExplored = true;
                shaderData.RefreshVisibility(this);
                enableChildGameobjects(true);
            }
        }

        /// <summary>
        /// To call if a unit can not longer see this cell
        /// </summary>
        public void DecreaseVisibility()
        {
            visibility -= 1;
            if (visibility == 0)
            {
                shaderData.RefreshVisibility(this);
                enableChildGameobjects(false);
            }
            visibility = Mathf.Max(visibility, 0);
        }

        /// <summary>
        /// Enables disables all child components
        /// </summary>
        /// <param name="enable"></param>
        private void enableChildGameobjects(bool enable)
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                //Debug.Log(transform.GetChild(i).GetType() + " " + (transform.GetChild(i).gameObject.GetType().Equals(typeof(UI.Ingame.HexTileHighlighter))));
                //Debug.Log(transform.GetChild(i).gameObject.Equals(highlighter));
                if (!transform.GetChild(i).GetType().Equals(typeof(RectTransform)))
                    transform.GetChild(i).gameObject.SetActive(IsExplored);
            }
        }

        /// <summary>
        /// Check if childGameobjects are disabled/enabled correctly
        /// </summary>
        public void checkChildGameobjects()
        {

            for (int i = 0; i < transform.childCount; i++)
            {
                if (!transform.GetChild(i).GetType().Equals(typeof(RectTransform)))
                    transform.GetChild(i).gameObject.SetActive(IsExplored);
            }
        }

        /// <summary>
        /// Get's the neighbour of this tile in a certian HexDirection
        /// </summary>
        /// <param name="direction">direction of the neighbour</param>
        /// <returns></returns>
        public HexTile GetNeighbour(HexDirection direction)
        {
            return neighbours[(int)direction];
        }

        /// <summary>
        /// Set's the neighbour of the cell in a certain HexDirection
        /// </summary>
        /// <param name="direction">direction of the neighbour</param>
        /// <param name="tile">reference to the tile</param>
        public void SetNeighbour(HexDirection direction, HexTile tile)
        {
            neighbours[(int)direction] = tile;
            if (tile != null)
                tile.neighbours[(int)direction.Opposite()] = this;
        }

        public HexEdgeType GetEdgeType(HexDirection direction)
        {
            return HexMetrics.GetEdgeType(elevation, neighbours[(int)direction].elevation);
        }
        public HexEdgeType GetEdgeType(HexTile otherCell)
        {
            return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
        }

        public HexTile[] GetNeighbours()
        {
            return neighbours;
        }

        public HexDirection GetDirectionTowards(HexTile tile)
        {
            Vector3 up = new Vector3(0, 0, 1);
            Vector3 diff = tile.transform.position - transform.position;
            float angle = Vector3.SignedAngle(up, diff, new Vector3(0, 1, 0));

            if (angle < 0)
                angle += 360;
            int i = Mathf.RoundToInt((angle - 30) / 60);
            return (HexDirection)i;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius">max distance to tile</param>
        /// <returns>all neighbours in given radius</returns>
        public List<HexTile> GetNeighbours(int radius)
        {
            List<HexTile> ret = new List<HexTile>(neighbours);
            if(radius > 1)
            {
                foreach (HexTile tile in neighbours)
                {
                    if (tile != null)
                    {
                        foreach (HexTile t in tile.GetNeighbours(radius - 1))
                        {
                            if (!ret.Contains(t))
                                ret.Add(t);
                        }

                        if (!ret.Contains(tile))
                            ret.Add(tile);
                    }
                }
            }
            return ret;
        }

        public ICollection<Vector3> GetCorners(HexDirection direction)
        {
            List<Vector3> corners = new List<Vector3>();
            corners.Add(HexMetrics.GetFirstCorner(direction) + transform.position);
            corners.Add(HexMetrics.GetSecondCorner(direction) + transform.position);
            return corners;
        }

        public ICollection<Vector3> GetCorners()
        {
            List<Vector3> corners = new List<Vector3>();
            for (int i = 0; i < 6; i++)
            {
                corners.Add(HexMetrics.corners[i] + transform.position);
            }
            return corners;
        }

        public bool IsUnderwater
        {
            get
            {
                return elevation < 0;
            }
        }

        public void Refresh()
        {
            if (chunk != null)
            {
                chunk.Refresh();
                foreach (HexTile neighbour in neighbours)
                {
                    if (neighbour != null && neighbour.chunk != chunk)
                    {
                        neighbour.chunk.Refresh();
                    }
                }
            }
        }
        public UI.Ingame.HexTileHighlighter highlighter => GetComponentInChildren<UI.Ingame.HexTileHighlighter>();
        public UI.Ingame.HexTileInfrastructureHighlighter infrastructure => GetComponentInChildren<UI.Ingame.HexTileInfrastructureHighlighter>();

        public override bool Equals(object o)
        {
            HexTile tile = o as HexTile;
            if (tile == null) return false;
            if (!this.Coordinate.Equals(tile.Coordinate)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
