using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawArrowLine : MonoBehaviour
{
    public LineRenderer LineRenderer;
    public Vector3 initialPosition;

    // Update is called once per frame
    void Update()
    {
        DrawLine(initialPosition, Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition))); 
    }

    private void DrawLine(Vector3 position, Vector3 mousePosition)
    {
        LineRenderer.SetPosition(0, new Vector3(position.x, position.y, 0));
        LineRenderer.SetPosition(1, new Vector3(mousePosition.x, mousePosition.y, 0));
    }
}
