using Map;
using Map.Shader;
using Map.Tile;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    World world;
    [SerializeField] RawImage image;
    Texture2D texture;
    int x1, x2, z1, z2, size;
    Color[,] bitMap;
    Color[] textureColors;


    void Awake()
    {
        //connect refresh function to onChange of HexCellShaderData
        HexCellShaderData.onChange += refresh;
        texture = new Texture2D(0, 0, TextureFormat.RGB24, false, false);
        texture.filterMode = FilterMode.Point;
        bitMap = new Color[1, 1];
    }

    private void Start()
    {
        world = GetComponent<World>();
        textureColors = new Color[world.biomeList.TextureList.Length];
        int count = 0;
        foreach (Texture2D tex in world.biomeList.TextureList)
        {
            Color col = tex.GetPixel(tex.width / 2, tex.height / 2);
            textureColors[count++] = col;
        }

    }

    void FixedUpdate()
    {
        texture.Resize(size, size);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                texture.SetPixel(i, size - 1 - j, bitMap[i, j]);
            }
        }
        texture.Apply();
        image.texture = texture;
        enabled = false;
    }


    /// <summary>
    /// Adds given HexTile to the minimap texture, expanding it if needed
    /// Called from within HexCellShaderData
    /// </summary>
    /// <param name="tile">Tile to add</param>
    private void refresh(HexTile tile)
    {
        addTile(tile);
        enabled = true;
    }

    private void addTile(HexTile tile)
    {
        // left = negative x
        // top = positive z
        int left = 0;
        int top = 0;


        int oldWidth = (Mathf.Abs(x1 - x2) + 1);
        int oldHeight = (Mathf.Abs(z1 - z2) + 1);
        int oldSize = Mathf.Max(oldWidth, oldHeight);

        HexCoordinates coordinates = HexCoordinates.ToOffsetCoordinates(tile.Coordinate);
        if (coordinates.X < x1)
        {
            left = x1 - coordinates.X;
            x1 = coordinates.X;
        }
        if (coordinates.Z > z1)
        {
            top = coordinates.Z - z1;
            z1 = coordinates.Z;
        }
        x2 = Mathf.Max(coordinates.X, x2);
        z2 = Mathf.Min(coordinates.Z, z2);

        int width = (Mathf.Abs(x1 - x2) + 1);
        int height = (Mathf.Abs(z1 - z2) + 1);

        size = Mathf.Max(width, height);

        Color[,] tmp = new Color[size, size];
        for (int i = left; i < oldSize; i++)
        {
            for (int j = top; j < oldSize; j++)
            {
                tmp[i, j] = bitMap[i - left, j - top];
            }
        }

        if (tile.IsExplored)
        {
            Color col = textureColors[tile.TerrainTypeIndex];
            if (tile.IsUnderwater)
            {
                col = Color.blue;
            }
            if (!tile.IsVisible)
            {
                col.r /= 2;
                col.g /= 2;
                col.b /= 2;
                col.a /= 2;
            }
            tmp[coordinates.X - x1, z1 - coordinates.Z] = col;
        }

        bitMap = tmp;
    }
}
