using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private readonly float panSpeed = 1000f;
    private readonly float panBorderThickeness = 5f;
    private readonly float scrollSpeed = 1000f;

    Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographicSize = Constants.CAMERA_LIMIT_PAN;
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

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 50f, Constants.CAMERA_LIMIT_PAN);


        
        // 4 8+1 (x*2 + x/4)

        if (cam.orthographicSize >= Constants.CAMERA_LIMIT_PAN/2)
        {
            pos.x = 0;
        }
        else
        {
            float size = (cam.orthographicSize * cam.aspect);
            pos.x = Mathf.Clamp(pos.x, -Constants.CAMERA_LIMIT_PAN + size, Constants.CAMERA_LIMIT_PAN - size);
        }
        pos.y = Mathf.Clamp(pos.y, -Constants.CAMERA_LIMIT_PAN + cam.orthographicSize, Constants.CAMERA_LIMIT_PAN - cam.orthographicSize);

        transform.position = pos;
    }
}
