using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int heigth)
    {
        Texture2D texture = new Texture2D(width, heigth);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeigthMap(float[,] heigthMap)
    {
        int width = heigthMap.GetLength(0);
        int heigth = heigthMap.GetLength(1);

        Color[] colourMap = new Color[width * heigth];
        for (int y = 0; y < heigth; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heigthMap[x, y]);
            }
        }

        return TextureFromColourMap(colourMap, width, heigth);
    }
}
