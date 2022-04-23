using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawArrowLine : MonoBehaviour
{
    public LineRenderer LineRenderer;

    public bool canDraw;
    public Vector3 initialPosition;
    // Start is called before the first frame update
    void Start()
    {
        canDraw = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(canDraw)
        {
            DrawLine(initialPosition, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
           
    }

    private void DrawLine(Vector3 position, Vector3 mousePosition)
    {
        LineRenderer.SetPosition(0, new Vector2(position.x, position.y));
        LineRenderer.SetPosition(1, new Vector2(mousePosition.x, mousePosition.y));
    }

}
