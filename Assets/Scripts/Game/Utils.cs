using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class MatchBehaviour : MonoBehaviour
{

}
public static class Utils
{
    static Texture2D _whiteTexture;
    public static Texture2D WhiteTexture
    {
        get
        {
            if (_whiteTexture == null)
            {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }

            return _whiteTexture;
        }
    }

    public static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, WhiteTexture);
        GUI.color = Color.white;
    }

    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Top
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        Utils.DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }

    public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        // Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    public static void InstantiateLineDrawer(Vector3 position, Color color)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Line");
        GameObject go = Object.Instantiate(prefab);
        DrawArrowLine drawLine = go.GetComponent<DrawArrowLine>();
        drawLine.initialPosition = position;
        drawLine.LineRenderer.startColor = color;
        drawLine.LineRenderer.endColor = color;
    }

    public static void DestroyLineDrawer()
    {
        var lines = Object.FindObjectsOfType<DrawArrowLine>();

        foreach (var line in lines)
            Object.Destroy(line.gameObject);
    }

    public static bool VerifyPointInLimits(Vector3 positon, Vector2[] corners)
    {
        if (positon.x <= corners[3].x && positon.x >= corners[0].x
            && positon.y >= corners[3].y && positon.y <= corners[0].y)
            return true;
        return false;
    }

    public static Vector3Int PositionByDistanceAndAngle(double angle, double distance, Vector2 point)
    {
        // Co (x) = sen * D
        double x = Math.Sin(angle) * distance;
        // Ca (y) = cos * D
        double y = Math.Cos(angle) * distance;

        //offset em relação ao ponto
        x += point.x;
        y += point.y;

        return new Vector3Int((int)x, (int)y, 0);
    }

    public static string GetObjetive(Vector3 vector)
    {
        Ray ray = Camera.main.ScreenPointToRay(vector);
        if (Physics.Raycast(ray, out RaycastHit hit, 50000.0f))
            if (hit.transform != null)
                if (hit.transform.CompareTag(Config.TAG_TEAM_1) || hit.transform.CompareTag(Config.TAG_TEAM_2))
                    return hit.transform.gameObject.name;              

        return "";
    }
}

