using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfinderPointsController : MonoBehaviour
{
    private SimulationController simulationController;
    private PathType pathType;
    private Vector3Int deceptivePosition;
    private bool hasDeceptivePosition;

    // Start is called before the first frame update
    void Start()
    {
        FindComponents();
        SetVariables();
    }

    /// <summary>
    /// Função que busca componentes na cena e atribui a variáveis
    /// </summary>
    private void FindComponents()
    {
        simulationController = GameObject.Find(Constants.SIMULATION_CONTROLLER).GetComponent<SimulationController>();
    }

    /// <summary>
    /// Função que seta valores iniciais em variáveis
    /// </summary>
    private void SetVariables()
    {
        pathType = simulationController.GetPathType();
        hasDeceptivePosition = false;
    }


    // Update is called once per frame
    void Update()
    {
        BuildingObjectivesPoints();
    }

    private void BuildingObjectivesPoints()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int positionClick = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            Utils.DestroyLineDrawer();
            pathType = simulationController.GetPathType();
            if (pathType.Equals(PathType.NORMAL))
            {
                positionClick = new Vector3Int(positionClick.x, positionClick.y, 0) / Constants.MAP_OFFSET;
                simulationController.ReceiveObjectivePositions(positionClick, deceptivePosition);
            }
            else if (!hasDeceptivePosition)
            {
                deceptivePosition = new Vector3Int(positionClick.x, positionClick.y, 0) / Constants.MAP_OFFSET;
                Utils.InstantiateLineDrawer(positionClick, Color.green);
                hasDeceptivePosition = true;
            }
            else
            {
                positionClick = new Vector3Int(positionClick.x, positionClick.y, 0) / Constants.MAP_OFFSET;
                simulationController.ReceiveObjectivePositions(positionClick, deceptivePosition);
                hasDeceptivePosition = false;
            }

        }
    }
}
