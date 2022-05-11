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
    private void FindComponents()
    {
        simulationController = GameObject.Find("Simulation Controller").GetComponent<SimulationController>();
    }

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
            Debug.Log("Pegando o ponto de path");
            Vector3Int positionClick = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            Utils.DestroyLineDrawer();
            if (pathType.Equals(PathType.NORMAL))
            {
                positionClick = new Vector3Int(positionClick.x, positionClick.y, 0) / 10;
                simulationController.ReceiveObjectivePositions(positionClick, deceptivePosition);
            }
            else if (!hasDeceptivePosition)
            {
                deceptivePosition = new Vector3Int(positionClick.x, positionClick.y, 0) / 10;
                Utils.InstantiateLineDrawer(positionClick, Color.green);
                hasDeceptivePosition = true;
            }
            else
            {
                positionClick = new Vector3Int(positionClick.x, positionClick.y, 0) / 10;
                simulationController.ReceiveObjectivePositions(positionClick, deceptivePosition);
                hasDeceptivePosition = false;
            }

        }
    }
}
