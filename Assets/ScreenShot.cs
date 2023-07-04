using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShot : MonoBehaviour
{
    Camera cam;
    private GameObject canvasPainels;

    private void Start()
    {
        cam = gameObject.GetComponent<Camera>();
        cam.enabled = false;
        cam.orthographicSize = Constants.CAMERA_LIMIT_PAN;
        canvasPainels = Camera.main.transform.Find("CanvasPainels").gameObject;
    }

    /// <summary>
    /// Função que ....
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IEnumerator TakeAPicture(int id, string type)
    {
        Debug.Log("Capturando imagem do(a) "+type+" id "+id.ToString());
        RenderTexture rt = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24);
        Camera.main.targetTexture = rt;
        Texture2D screenShot = new Texture2D(cam.pixelWidth, cam.pixelHeight);

        canvasPainels.SetActive(false);
        yield return new WaitForEndOfFrame();

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, cam.pixelWidth, cam.pixelHeight), 0, 0);
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(@"C:\Users\crisl\OneDrive\Documentos\GitHub\Hunt the flag Game\Assets\Resources\"+type+"_Case_"+id+".png", bytes);
        canvasPainels.SetActive(true);
    }

    public IEnumerator DoScreenShot(int id, string type)
    {
        Camera.main.orthographicSize = Constants.CAMERA_LIMIT_PAN;
        //ativar essa camera
        cam.enabled = true;

        StartCoroutine(TakeAPicture(id, type));

        //desativar essa camera
        cam.enabled = false;
        yield return null;
    }
}
