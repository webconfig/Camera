using System;
using UnityEngine;
public class TextureScale2
{
    private static Color[] texColors;
    private static Color[] newColors;
    private static int w;
    private static float ratioX;
    private static float ratioY;

    private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight)
    {
        texColors = tex.GetPixels();
        newColors = new Color[newWidth * newHeight];
        ratioX = tex.width / newWidth * 1.0f;
        ratioY = tex.height / newHeight * 1.0f;
        w = tex.width;
        //PointScale(0, newHeight);
        int num = 0, num2 = 0, j = 0, i = 0;
        for (i = 0; i < newHeight; i++)
        {
            num = (int)(ratioY * i) * w;
            num2 = i * newWidth;
            for (j = 0; j < newWidth; j++)
            {
                newColors[num2 + j] = texColors[(int)(num + ratioX * j)];
            }
        }

        tex.Resize(newWidth, newHeight);
        tex.SetPixels(newColors);
        tex.Apply();
    }
    //public static void PointScale(int start, int end)
    //{
    //    int num = 0, num2 = 0, j = 0, i = 0;
    //    for (i = start; i < end; i++)
    //    {
    //        num = (int)(ratioY * i) * w;
    //        num2 = i * w2;
    //        for (j = 0; j < w2; j++)
    //        {
    //            newColors[num2 + j] = texColors[(int)(num + ratioX * j)];
    //        }
    //    }
    //}
}