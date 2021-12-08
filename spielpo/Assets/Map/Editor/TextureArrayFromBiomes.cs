using Map.Biome;
using UnityEditor;
using UnityEngine;

public class TextureArrayFromBiomes : ScriptableWizard
{
    public Biomes biomeList;

    [MenuItem("Assets/Create/BiomeTextureArray")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<TextureArrayFromBiomes>(
            "Create Texture Array from biomes", "Create"
        );
    }

    void OnWizardCreate()
    {
        if (biomeList.TextureList.Length == 0)
            return;

        string path = EditorUtility.SaveFilePanelInProject("Save Texture Array", "Texture Array", "asset", "Save Texture Array");

        if (path.Length == 0)
            return;

        Texture2D t = biomeList.TextureList[0];
        Texture2DArray textureArray = new Texture2DArray(t.width, t.height, biomeList.TextureList.Length, t.format, t.mipmapCount > 1);
        textureArray.anisoLevel = t.anisoLevel;
        textureArray.filterMode = t.filterMode;
        textureArray.wrapMode = t.wrapMode;

        for (int i = 0; i < biomeList.TextureList.Length; i++)
        {
            for (int m = 0; m < t.mipmapCount; m++)
            {
                Graphics.CopyTexture(biomeList.TextureList[i], 0, m, textureArray, i, m);
            }
        }
        AssetDatabase.CreateAsset(textureArray, path);
    }

}