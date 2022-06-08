using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender;
    public bool flip;

    public void DrawnTexture(Texture2D texture, float sizeX, float sizeY)
    {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(sizeX, 1, sizeY);

        if (flip)
        {
            Vector3 theScale = textureRender.transform.localScale;
            theScale.x *= -1;
            theScale.z *= -1;
            textureRender.transform.localScale = theScale;
        }    
    }
}
