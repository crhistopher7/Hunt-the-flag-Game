using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    public void SetUpLine(List<LogicMap> path, Vector3 initialPosition, PathType pathType)
    {
        //TODO: de acordo com o pathType, desenhar um tipo de linha diferente

        transform.position = initialPosition;
        lr.positionCount = path.Count + 1;
        List<Vector3> points = new List<Vector3>();
        points.Add(new Vector3(initialPosition.x, initialPosition.y, -0.5f) * Constants.MAP_OFFSET);
        foreach (LogicMap p in path)
        {
            points.Add(new Vector3(p.ClickPosition.x, p.ClickPosition.y, -0.5f) * Constants.MAP_OFFSET);
        }
        DrawPath(points);
    }

    private void DrawPath(List<Vector3> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            lr.SetPosition(i, points[i]);
        }
    }
}
