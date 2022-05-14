using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PopUpSelectAStar : MonoBehaviour
{
    public GameObject canvasSelectPathfinder;
    public Dropdown dropdown;
    public RectTransform rectPanel;
    public RectTransform rectCanvas;
    public Camera cam;

    string pathSelected;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            canvasSelectPathfinder.SetActive(true);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectCanvas, Input.mousePosition, cam, out Vector2 anchoredPos);

            anchoredPos = new Vector2(anchoredPos.x, anchoredPos.y);
            rectPanel.anchoredPosition = anchoredPos;
        }
    }

    public void SetPathfinder()
    {
        pathSelected = dropdown.options[dropdown.value].text;
        Debug.Log("Path selected: "+pathSelected);

        canvasSelectPathfinder.SetActive(false);
    }
}
