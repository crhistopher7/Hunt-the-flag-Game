
using AnotherFileBrowser.Windows;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FileBrowserUpdate : MonoBehaviour
{
    public RawImage rawImage;

    public void OpenFileBrowser()
    {
        var bp = new BrowserProperties();
        bp.title = "Select Heightmap Image File";
        bp.filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
        bp.filterIndex = 0;
        bp.initialDir = Constants.MAP_HEIGHTMAP_DIRECTORY;

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            //Load image from local path with UWR
            //StartCoroutine(LoadImage(path));
            Debug.Log(path);
        });

        bp.title = "Select Satellite Image File";
        bp.initialDir = Constants.MAP_SATELLITE_DIRECTORY;

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            //Load image from local path with UWR
            //StartCoroutine(LoadImage(path));
            Debug.Log(path);
        });
    }

    IEnumerator LoadImage(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                var uwrTexture = DownloadHandlerTexture.GetContent(uwr);
                rawImage.texture = uwrTexture;
            }
        }
    }
}
