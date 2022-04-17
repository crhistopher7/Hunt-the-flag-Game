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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            canvasSelectPathfinder.SetActive(true);
            Vector2 anchoredPos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectCanvas, Input.mousePosition, cam, out anchoredPos);

            anchoredPos = new Vector2(anchoredPos.x + 80, anchoredPos.y - 30);
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
