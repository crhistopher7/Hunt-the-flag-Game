using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectController : MonoBehaviour
{
    private SimulationController simulationController;
    private bool dragSelect;

    private Vector3 mousePosition1;
    private Vector3 mousePosition2;
    private Vector2[] corners;

    // Start is called before the first frame update
    void Start()
    {
        FindComponents();
        SetVariables();
    }

    // Update is called once per frame
    void Update()
    {
        SelectAgentsMode();
    }


    private void FindComponents()
    {
        simulationController = GameObject.Find("Simulation Controller").GetComponent<SimulationController>();
    }

    private void SetVariables()
    {
        dragSelect = false;
    }


    private void SelectAgentsMode()
    {
        //1. Quando pressiona o botão, pega o ponto
        if (Input.GetMouseButtonDown(0))
            if (Input.GetKey(KeyCode.LeftShift))
                mousePosition1 = Input.mousePosition;
            else
                if (simulationController.VerifySelectedSingleAgent(Input.mousePosition)) //Clicou em um agente. Perguntar Path
                {
                    simulationController.pointOfCanvasPath = Input.mousePosition;
                    simulationController.DesableComponentSelectController();
                }

        //2. Se continuou pressionando e moveu um ponto, pode criar a box
        if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftShift))
            if ((mousePosition1 - Input.mousePosition).magnitude > 40)
                dragSelect = true;

        //3. Quando soltar o botão...
        if (Input.GetMouseButtonUp(0) && Input.GetKey(KeyCode.LeftShift) && dragSelect)
        {
            var pointOfCanvasPath = new Vector3((mousePosition1.x + Input.mousePosition.x) / 2, (mousePosition1.y + Input.mousePosition.y) / 2, mousePosition1.z);

            mousePosition1 = Camera.main.ScreenToWorldPoint(mousePosition1);
            mousePosition2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            corners = GetBoundingBox(mousePosition1, mousePosition2);

            if (simulationController.VerifySelectedGroupAgent(corners))
            {
                simulationController.pointOfCanvasPath = pointOfCanvasPath;
                simulationController.DesableComponentSelectController();
            }
            dragSelect = false;
        }
    }


    //create a bounding box (4 corners in order) from the start and end mouse position
    public Vector2[] GetBoundingBox(Vector2 p1, Vector2 p2)
    {
        var bottomLeft = Vector3.Min(p1, p2);
        var topRight = Vector3.Max(p1, p2);

        // 0 = top left; 1 = top right; 2 = bottom left; 3 = bottom right;
        Vector2[] corners =
        {
            new Vector2(bottomLeft.x, topRight.y),
            new Vector2(topRight.x, topRight.y),
            new Vector2(bottomLeft.x, bottomLeft.y),
            new Vector2(topRight.x, bottomLeft.y)
        };
        return corners;
    }

    private void OnGUI()
    {
        if (dragSelect == true)
        {
            var rect = Utils.GetScreenRect(mousePosition1, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }
}
