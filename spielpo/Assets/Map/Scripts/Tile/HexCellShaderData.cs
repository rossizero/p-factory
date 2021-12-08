using Map.Tile;
using System;
using System.IO;
using UnityEngine;

namespace Map.Shader
{
    public class HexCellShaderData : MonoBehaviour
    {

        public static Action<HexTile> onChange;

        //texture to be published to all shaders, containing colors that will be used to 
        //apply certain behaviour to each cell
        //size = rectangle seen somewhere below
        static Texture2D texture;

        //used to fill the texture after every change in size of the current map
        //r = visible by any house or unit?
        //g = explored or not?
        //b = still free :)
        //a = terrainTypeIndex
        static Color32[] cellTextureData;

        /* Keep track of the rectangle that includes all chunks
		 * to resize the texture accordingly
		 * 
		 * (x1, z1) ***************
		 * *					  *
		 * *****************(x2, z2)
		 */
        static int x1, x2, z1, z2;

        /// <summary>
        /// If enabled somewhere below, redo the texture
        /// </summary>
        void LateUpdate()
        {
            texture.SetPixels32(cellTextureData);
            texture.Apply();
            enabled = false;
        }

        /// <summary>
        /// A new instantiated chunk has to register itself here to be visible.
        /// The texture size will be changed, so every tile in every chunk lies within it's bounds
        /// </summary>
        /// <param name="chunkX">x coordinate of the chunk</param>
        /// <param name="chunkZ">y coordinate of the chunk</param>
        public void registerChunk(int chunkX, int chunkZ)
        {

            // left = negative x
            // top = positive z
            x1 = Mathf.Min(chunkX, x1);
            z1 = Mathf.Max(chunkZ, z1);

            x2 = Mathf.Max(chunkX, x2);
            z2 = Mathf.Min(chunkZ, z2);

            int width = (Mathf.Abs(x1 - x2) + 1) * HexMetrics.chunkSizeX;
            int height = (Mathf.Abs(z1 - z2) + 1) * HexMetrics.chunkSizeZ;
            Initialize(width, height);
        }

        /// <summary>
        /// Publishes data to all shaders, creates the texture and updates cellTextureData
        /// in case it got too small
        /// </summary>
        /// <param name="width">width of the current map</param>
        /// <param name="height">height of the current map</param>
        private void Initialize(int width, int height)
        {
            enabled = true;
            if (texture)
            {
                texture.Resize(width, height);
            }
            else
            {
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                UnityEngine.Shader.SetGlobalTexture("_HexCellData", texture);
            }
            UnityEngine.Shader.SetGlobalVector("_HexCellData_TexelSizeProperty", new Vector4(1f / width, 1f / height, width, height));

            if (cellTextureData == null)
                cellTextureData = new Color32[width * height];
            if (cellTextureData.Length != width * height)
            {
                Color32[] tmp = new Color32[width * height];
                Array.Copy(cellTextureData, 0, tmp, 0, cellTextureData.Length);
                cellTextureData = tmp;
            }
        }

        /// <summary>
        /// To refresh the shader data (visibilty and exporation) of given tile
        /// </summary>
        /// <param name="tile">tile to be refreshed</param>
        public void RefreshVisibility(HexTile tile)
        {
            cellTextureData[tile.index].r = tile.IsVisible ? (byte)255 : (byte)0;
            cellTextureData[tile.index].g = tile.IsExplored ? (byte)255 : (byte)0;
            enabled = true;

            onChange(tile);
        }

        /// <summary>
        /// To refresh the shader data for the terrainType of given tile
        /// </summary>
        /// <param name="tile"></param>
        public void RefreshTerrain(HexTile tile)
        {
            cellTextureData[tile.index].a = (byte)(tile.TerrainTypeIndex);
            enabled = true;

            onChange(tile);
        }
    }
}
