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
    /// Fun��o que busca componentes na cena e atribui a vari�veis
    /// </summary>
    private void FindComponents()
    {
        simulationController = GameObject.Find(Constants.SIMULATION_CONTROLLER).GetComponent<SimulationController>();
    }

    /// <summary>
    /// Fun��o que seta valores iniciais em vari�veis
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

    private void BuildingObjectivesPoints(bool debug = false)
    {
        if (debug) // Deixar os objetivos fixos
        {
            Vector3Int realGoal = new Vector3Int(0, 0, 0) / Constants.MAP_OFFSET;
            Vector3Int deceptiveGoal = new Vector3Int(0, 0, 0) / Constants.MAP_OFFSET;

            simulationController.ReceiveObjectivePositions(realGoal, deceptiveGoal);
        }


        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int positionClick = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            Utils.DestroyLineDrawer();
            pathType = simulationController.GetPathType();

            if (pathType.Equals(PathType.NORMAL))
            {
                positionClick = new Vector3Int(positionClick.x, positionClick.y, 0) / Constants.MAP_OFFSET;
                Debug.Log(positionClick);
                simulationController.ReceiveObjectivePositions(positionClick, positionClick); //Caso seja normal, n�o tem posi��o enganosa
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
