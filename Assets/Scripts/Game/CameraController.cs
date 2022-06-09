using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float panSpeed = 200f;
    private float panBorderThickeness = 5f;
    private float scrollSpeed = 100f;

    Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;

        if (Input.mousePosition.y >= Screen.height - panBorderThickeness)
            pos.y += panSpeed * Time.deltaTime;
        if (Input.mousePosition.y <= panBorderThickeness)
            pos.y -= panSpeed * Time.deltaTime;
        if (Input.mousePosition.x >= Screen.width - panBorderThickeness)
            pos.x += panSpeed * Time.deltaTime;
        if (Input.mousePosition.x <= panBorderThickeness)
            pos.x -= panSpeed * Time.deltaTime;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * scrollSpeed * 100f * Time.deltaTime;

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 50f, Config.CAMERA_LIMIT_PAN);


        
        // 4 8+1 (x*2 + x/4)

        if (cam.orthographicSize >= Config.CAMERA_LIMIT_PAN/2)
        {
            pos.x = 0;
        }
        else
        {
            float size = (cam.orthographicSize * cam.aspect);
            pos.x = Mathf.Clamp(pos.x, -Config.CAMERA_LIMIT_PAN + size, Config.CAMERA_LIMIT_PAN - size);
        }
        pos.y = Mathf.Clamp(pos.y, -Config.CAMERA_LIMIT_PAN + cam.orthographicSize, Config.CAMERA_LIMIT_PAN - cam.orthographicSize);

        transform.position = pos;
    }
}
