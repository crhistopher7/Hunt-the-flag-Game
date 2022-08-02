using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    private LineRenderer lr;
    [SerializeField] private Material normal;
    [SerializeField] private Material dotted;
    [SerializeField] private Material trail;
    [SerializeField] private Material arrow;
    [SerializeField] private Material star;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.material = normal;
    }

    public void SetUpLine(List<LogicMap> path, Vector3 initialPosition, PathType pathType)
    {   
        lr.positionCount = path.Count + 1;
        transform.position = initialPosition;
        
        List<Vector3> points = new List<Vector3>
        {
            new Vector3(initialPosition.x, initialPosition.y, -0.5f) * Constants.MAP_OFFSET
        };
        foreach (LogicMap p in path)
        {
            points.Add(new Vector3(p.ClickPosition.x, p.ClickPosition.y, -0.5f) * Constants.MAP_OFFSET);
        }
        lr.material = SetMaterialByPathType(pathType);
        DrawPath(points);
    }

    private Material SetMaterialByPathType(PathType pathType)
    {
        if (pathType == PathType.NORMAL)
            return normal;

        else if (pathType == PathType.DECEPTIVE_1)
            return dotted;

        else if (pathType == PathType.DECEPTIVE_2)
            return trail;

        else if (pathType == PathType.DECEPTIVE_3)
            return arrow;

        else
            return star;
    }

    private void DrawPath(List<Vector3> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            lr.SetPosition(i, points[i]);
        }
    }
}
