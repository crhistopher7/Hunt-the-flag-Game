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
    IEnumerator TakeAPicture(int id)
    {
        Debug.Log("Capturando imagem do caso");
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
        System.IO.File.WriteAllBytes("Case_"+id+".png", bytes);
        canvasPainels.SetActive(true);
    }

    public void DoScreenShot(int id)
    {
        Camera.main.orthographicSize = Constants.CAMERA_LIMIT_PAN;
        //ativar essa camera
        cam.enabled = true;

        StartCoroutine(TakeAPicture(id));

        //desativar essa camera
        cam.enabled = false;
    }
}
