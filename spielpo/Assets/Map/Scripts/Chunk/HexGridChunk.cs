using Map.Shader;
using Map.Tile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Map.Chunk
{
    public class HexGridChunk : MonoBehaviour
    {
        //colors used to blend three possible textures touching in one vertex of the mesh
        static Color weights1 = new Color(1f, 0f, 0f);
        static Color weights2 = new Color(0f, 1f, 0f);
        //static Color weights3 = new Color(0f, 0f, 1f); //BUG: if we use this, triangles are messed up (wrong 3d terrain index)
        static Color weights3 = new Color(0.5f, 0.5f, 0f); //using this instead for now

        public HexCellShaderData cellShaderData { get; private set; }

        public Dictionary<HexCoordinates, HexTile> tiles; //to retreive tile data by their coordinates

        public HexMesh terrain, water;
        Canvas gridCanvas;

        //make private if not longer needed in inspector
        public int x;
        public int z;

        void Awake()
        {
            gridCanvas = GetComponentInChildren<Canvas>();
            tiles = new Dictionary<HexCoordinates, HexTile>();
            cellShaderData = gameObject.AddComponent<HexCellShaderData>();
        }
        /// <summary>
        /// To tell the chunk it's coordinates
        /// </summary>
        /// <param name="x">x coordinate of chunk</param>
        /// <param name="z">z coordinate of chunk</param>
        public void setChunkCoordinates(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        /// <summary>
        /// get index of a tile (leftover of an older version, sorry)
        /// </summary>
        /// <param name="tile"></param>
        /// <returns>unique tile index</returns>
        public int getIndexOfTile(HexTile tile)
        {
            return tile.index;
        }

        /// <summary>
        /// to get a tile from this chunk
        /// </summary>
        /// <param name="coordinates">HexCoordinates of that tile</param>
        /// <returns></returns>
        public HexTile getTile(HexCoordinates coordinates)
        {
            if (tiles.ContainsKey(coordinates))
                return tiles[coordinates];
            return null;
        }

        /// <summary>
        /// Add's a tile to the chunk
        /// </summary>
        /// <param name="cell"></param>
        public void AddCell(HexTile cell)
        {
            tiles[cell.Coordinate] = cell;
            cell.uiRect.SetParent(gridCanvas.transform, false);
        }

        /// <summary>
        /// Redraws the whole mesh
        /// </summary>
        void LateUpdate()
        {
            Triangulate();
            enabled = false;
        }

        /// <summary>
        /// enables this component, triggers a redraw of mesh in LateUpdate()
        /// </summary>
        public void Refresh()
        {
            enabled = true;
        }

        /// <summary>
        /// creates the mesh for the whole chunk
        /// </summary>
        public void Triangulate()
        {
            terrain.Clear();
            water.Clear();
            foreach (HexTile tile in tiles.Values)
            {
                Triangulate(tile);
            }
            terrain.Apply();
            water.Apply();
        }

        /// <summary>
        /// creates the mesh for a tile
        /// </summary>
        /// <param name="tile">tile to create a mesh for</param>
        void Triangulate(HexTile tile)
        {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                Triangulate(d, tile);
            }
        }

        // ----------------------------------------------------------------------
        // The following code was mostly taken from
        // https://catlikecoding.com/unity/tutorials/hex-map/
        // (it's a mix of code fragments from many of those tutorials though)
        // ----------------------------------------------------------------------

        void Triangulate(HexDirection direction, HexTile cell)
        {
            Vector3 center = cell.transform.localPosition;
            EdgeVertices e = new EdgeVertices(
                center + HexMetrics.GetFirstSolidCorner(direction),
                center + HexMetrics.GetSecondSolidCorner(direction)
            );
            TriangulateEdgeFan(center, e, getIndexOfTile(cell));


            if (direction <= HexDirection.SE)
            {
                TriangulateConnection(direction, cell, e);
            }

            if (cell.IsUnderwater)
            {
                TriangulateWater(direction, cell, center);
            }

        }


        void TriangulateWater(HexDirection direction, HexTile cell, Vector3 center)
        {
            center.y = cell.WaterSurfaceY;
            Vector3 c1 = center + HexMetrics.GetFirstSolidCorner(direction);
            Vector3 c2 = center + HexMetrics.GetSecondSolidCorner(direction);

            water.AddTriangle(center, c1, c2);
            Vector3 indices;
            indices.x = indices.y = indices.z = getIndexOfTile(cell);
            water.AddTriangleCellData(indices, weights1);

            if (direction <= HexDirection.SE)
            {
                HexTile neighbor = cell.GetNeighbour(direction);
                if (neighbor == null)
                {
                    return;
                }

                Vector3 bridge = HexMetrics.GetBridge(direction);
                Vector3 e1 = c1 + bridge;
                Vector3 e2 = c2 + bridge;

                water.AddQuad(c1, c2, e1, e2);
                indices.y = getIndexOfTile(neighbor);
                water.AddQuadCellData(indices, weights1, weights2);
                if (direction <= HexDirection.E)
                {
                    HexTile nextNeighbor = (HexTile)cell.GetNeighbour(direction.Next());
                    if (nextNeighbor == null)
                    {
                        return;
                    }
                    water.AddTriangle(c2, e2, c2 + HexMetrics.GetBridge(direction.Next()));
                    indices.z = getIndexOfTile(neighbor);
                    water.AddTriangleCellData(indices, weights1, weights2, weights3);
                }
            }
        }

        void TriangulateConnection(HexDirection direction, HexTile cell, EdgeVertices e1)
        {
            HexTile neighbor = (HexTile)cell.GetNeighbour(direction);
            if (neighbor == null)
            {
                return;
            }

            Vector3 bridge = HexMetrics.GetBridge(direction);
            bridge.y = neighbor.transform.localPosition.y - cell.transform.localPosition.y;
            EdgeVertices e2 = new EdgeVertices(e1.v1 + bridge, e1.v4 + bridge);

            if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
            {
                TriangulateEdgeTerraces(e1, cell, e2, neighbor);
            }
            else
            {
                TriangulateEdgeStrip(e1, weights1, getIndexOfTile(cell), e2, weights2, getIndexOfTile(neighbor));
            }


            HexTile nextNeighbor = cell.GetNeighbour(direction.Next());
            if (direction <= HexDirection.E && nextNeighbor != null)
            {
                Vector3 v5 = e1.v4 + HexMetrics.GetBridge(direction.Next());
                v5.y = nextNeighbor.transform.localPosition.y;

                if (cell.Elevation <= neighbor.Elevation)
                {
                    if (cell.Elevation <= nextNeighbor.Elevation)
                    {
                        TriangulateCorner(e1.v4, cell, e2.v4, neighbor, v5, nextNeighbor);
                    }
                    else
                    {
                        TriangulateCorner(v5, nextNeighbor, e1.v4, cell, e2.v4, neighbor);
                    }
                }
                else if (neighbor.Elevation <= nextNeighbor.Elevation)
                {
                    TriangulateCorner(e2.v4, neighbor, v5, nextNeighbor, e1.v4, cell);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbor, e1.v4, cell, e2.v4, neighbor);
                }
            }
        }
        void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, float index)
        {
            terrain.AddTriangle(center, edge.v1, edge.v2);
            terrain.AddTriangle(center, edge.v2, edge.v3);
            terrain.AddTriangle(center, edge.v3, edge.v4);

            Vector3 indices;
            indices.x = indices.y = indices.z = index;
            terrain.AddTriangleCellData(indices, weights1);
            terrain.AddTriangleCellData(indices, weights1);
            terrain.AddTriangleCellData(indices, weights1);
        }
        void TriangulateEdgeStrip(EdgeVertices e1, Color w1, float index1, EdgeVertices e2, Color w2, float index2)
        {
            terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);

            Vector3 indices;
            indices.x = indices.z = index1;
            indices.y = index2;
            terrain.AddQuadCellData(indices, w1, w2);
            terrain.AddQuadCellData(indices, w1, w2);
            terrain.AddQuadCellData(indices, w1, w2);
        }
        void TriangulateEdgeTerraces(EdgeVertices begin, HexTile beginCell, EdgeVertices end, HexTile endCell)
        {
            EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
            Color w2 = HexMetrics.TerraceLerp(weights1, weights2, 1);
            float i1 = getIndexOfTile(beginCell);
            float i2 = getIndexOfTile(endCell);

            TriangulateEdgeStrip(begin, weights1, i1, e2, w2, i2);

            for (int i = 2; i < HexMetrics.terraceSteps; i++)
            {
                EdgeVertices e1 = e2;
                Color w1 = w2;
                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                w2 = HexMetrics.TerraceLerp(weights1, weights2, i);
                TriangulateEdgeStrip(e1, w1, i1, e2, w2, i2);
            }

            TriangulateEdgeStrip(e2, w2, i1, end, weights2, i2);
        }
        void TriangulateCorner(Vector3 bottom, HexTile bottomCell, Vector3 left, HexTile leftCell, Vector3 right, HexTile rightCell)
        {
            HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
            HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

            if (leftEdgeType == HexEdgeType.Slope)
            {
                if (rightEdgeType == HexEdgeType.Slope)
                {
                    TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                }
                else if (rightEdgeType == HexEdgeType.Flat)
                {
                    TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                }
                else
                {
                    TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
                }
            }
            else if (rightEdgeType == HexEdgeType.Slope)
            {
                if (leftEdgeType == HexEdgeType.Flat)
                {
                    TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                }
                else
                {
                    TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                }
            }
            else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                if (leftCell.Elevation < rightCell.Elevation)
                {
                    TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                }
                else
                {
                    TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
                }
            }
            else
            {
                terrain.AddTriangle(bottom, left, right);
                Vector3 indices;
                indices.x = getIndexOfTile(bottomCell);
                indices.y = getIndexOfTile(leftCell);
                indices.z = getIndexOfTile(rightCell);
                terrain.AddTriangleCellData(indices, weights1, weights2, weights3);
            }
        }
        void TriangulateCornerTerraces(Vector3 begin, HexTile beginCell, Vector3 left, HexTile leftCell, Vector3 right, HexTile rightCell)
        {
            Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
            Color w3 = HexMetrics.TerraceLerp(weights1, weights2, 1);
            Color w4 = HexMetrics.TerraceLerp(weights1, weights3, 1);
            Vector3 indices;
            indices.x = getIndexOfTile(beginCell);
            indices.y = getIndexOfTile(leftCell);
            indices.z = getIndexOfTile(rightCell);

            terrain.AddTriangle(begin, v3, v4);
            terrain.AddTriangleCellData(indices, weights1, w3, w4);

            for (int i = 2; i < HexMetrics.terraceSteps; i++)
            {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color w1 = w3;
                Color w2 = w4;
                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                w3 = HexMetrics.TerraceLerp(weights1, weights2, i);
                w4 = HexMetrics.TerraceLerp(weights1, weights3, i);
                terrain.AddQuad(v1, v2, v3, v4);
                terrain.AddQuadCellData(indices, w1, w2, w3, w4);
            }
            terrain.AddQuad(v3, v4, left, right);
            terrain.AddQuadCellData(indices, w3, w4, weights2, weights3);
        }
        void TriangulateCornerTerracesCliff(Vector3 begin, HexTile beginCell, Vector3 left, HexTile leftCell, Vector3 right, HexTile rightCell)
        {
            float b = 1f / (rightCell.Elevation - beginCell.Elevation);
            if (b < 0)
            {
                b = -b;
            }
            Vector3 boundary = Vector3.Lerp(terrain.Perturb(begin), terrain.Perturb(right), b);
            Color boundaryWeights = Color.Lerp(weights1, weights3, b);
            Vector3 indices;
            indices.x = getIndexOfTile(beginCell);
            indices.y = getIndexOfTile(leftCell);
            indices.z = getIndexOfTile(rightCell);

            TriangulateBoundaryTriangle(begin, weights1, left, weights2, boundary, boundaryWeights, indices);
            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, weights2, right, weights3, boundary, boundaryWeights, indices);
            }
            else
            {
                terrain.AddTriangleUnperturbed(terrain.Perturb(left), terrain.Perturb(right), boundary);

                terrain.AddTriangleCellData(indices, weights2, weights3, boundaryWeights);
            }
        }
        void TriangulateCornerCliffTerraces(Vector3 begin, HexTile beginCell, Vector3 left, HexTile leftCell, Vector3 right, HexTile rightCell)
        {
            float b = 1f / (leftCell.Elevation - beginCell.Elevation);
            if (b < 0)
            {
                b = -b;
            }
            Vector3 boundary = Vector3.Lerp(terrain.Perturb(begin), terrain.Perturb(left), b);
            Color boundaryWeights = Color.Lerp(weights1, weights2, b);
            Vector3 indices;
            indices.x = getIndexOfTile(beginCell);
            indices.y = getIndexOfTile(leftCell);
            indices.z = getIndexOfTile(rightCell);
            TriangulateBoundaryTriangle(right, weights3, begin, weights1, boundary, boundaryWeights, indices);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, weights2, right, weights3, boundary, boundaryWeights, indices);
            }
            else
            {
                terrain.AddTriangleUnperturbed(terrain.Perturb(left), terrain.Perturb(right), boundary);
                terrain.AddTriangleCellData(indices, weights2, weights3, boundaryWeights);
            }
        }
        void TriangulateBoundaryTriangle(Vector3 begin, Color beginWeights, Vector3 left, Color leftWeights, Vector3 boundary, Color boundaryWeights, Vector3 indices)
        {
            Vector3 v2 = terrain.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            Color w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, 1);
            terrain.AddTriangleUnperturbed(terrain.Perturb(begin), v2, boundary);
            terrain.AddTriangleCellData(indices, beginWeights, w2, boundaryWeights);

            for (int i = 2; i < HexMetrics.terraceSteps; i++)
            {
                Vector3 v1 = v2;
                Color w1 = w2;
                v2 = terrain.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, i);
                terrain.AddTriangleUnperturbed(v1, v2, boundary);
                terrain.AddTriangleCellData(indices, w1, w2, boundaryWeights);
            }

            terrain.AddTriangleUnperturbed(v2, terrain.Perturb(left), boundary);
            terrain.AddTriangleCellData(indices, w2, leftWeights, boundaryWeights);
        }
    }
}

