using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectController : MonoBehaviour
{
    private SimulationController simulationController;
    private bool dragSelect;
    private bool settingNewPosition;

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

    /// <summary>
    /// Fun��o que busca componentes na cena e atribui a vari�veis
    /// </summary>
    private void FindComponents()
    {
        simulationController = GameObject.Find("SimulationController").GetComponent<SimulationController>();
    }

    /// <summary>
    /// Fun��o que seta valores iniciais em vari�veis
    /// </summary>
    private void SetVariables()
    {
        dragSelect = false;
        settingNewPosition = false;
    }


    private void SelectAgentsMode()
    {
        // Se apertar o direito e tiver um agente ou objetivo
        if (Input.GetMouseButtonDown(1) && !dragSelect)
        {
            if (settingNewPosition)
            {
                simulationController.ClearSelectedObjects();
                settingNewPosition = false;
            }
            else if (simulationController.VerifySelectedSingleObject(Input.mousePosition)) // Clicou em um agente ou bandeira e n�o esta setando uma nova posi��o 
                settingNewPosition = true;
        }
            
        else if (!settingNewPosition)
        {
            //1. Quando pressiona o bot�o, pega o ponto
            if (Input.GetMouseButtonDown(0))
                if (Input.GetKey(KeyCode.LeftShift))
                    mousePosition1 = Input.mousePosition;
                else
                    if (simulationController.VerifySelectedSingleAgent(Input.mousePosition)) // Clicou em um agente. Perguntar Path
                    simulationController.DesableComponentSelectController();

            //2. Se continuou pressionando e moveu um ponto, pode criar a box
            if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftShift))
                if ((mousePosition1 - Input.mousePosition).magnitude > 40)
                    dragSelect = true;

            //3. Quando soltar o bot�o...
            if (Input.GetMouseButtonUp(0) && dragSelect)
            {
                var pointOfCanvasPath = new Vector3((mousePosition1.x + Input.mousePosition.x) / 2, (mousePosition1.y + Input.mousePosition.y) / 2, mousePosition1.z);

                mousePosition1 = Camera.main.ScreenToWorldPoint(mousePosition1);
                mousePosition2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                corners = GetBoundingBox(mousePosition1, mousePosition2);

                if (simulationController.VerifySelectedGroupAgent(corners))
                {
                    simulationController.DesableComponentSelectController();
                }
                dragSelect = false;
            }
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
