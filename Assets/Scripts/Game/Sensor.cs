using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    const float sensorLength = 1000.0f;
    const float frontSideSensorStartingPoint = 0;
    const float frontSensorAngle = 40;

    public List<RaycastHit> Check()
    {
        List<RaycastHit> listOfHit = new List<RaycastHit>();
        RaycastHit hit;

        for (float x = -1; x < 2f; x++)
        {
            for (float y = -1; y <= 1f; y += 0.01f)
            {
                if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(x, y, 0)), out hit, 20))
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(x, y, 0)) * 20, Color.red);
                    //tocou em alguma coisa
                    listOfHit.Add(hit);
                }

                if (Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(y, x, 0)), out hit, 20))
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(y, x, 0)) * 20, Color.red);
                    //tocou em alguma coisa
                    listOfHit.Add(hit);
                }
            }
        }

        return listOfHit;
    }

    public static float[] CheckDirection(Transform a, Transform b)
    {
        RaycastHit hit;
        float distance = Vector3.Distance(a.position, b.position);
        for (float x = -1; x < 2f; x++)
        {
            for (float y = -1; y <= 1f; y += 0.001f)
            {
                if (Physics.Raycast(a.position, a.TransformDirection(new Vector3(x, y, 0)), out hit, distance))
                {
                    if(GameObject.ReferenceEquals(hit.transform.gameObject, b.gameObject))
                    {
                        Debug.DrawRay(a.position, a.TransformDirection(new Vector3(x, y, 0) * distance), Color.red);
                        return new float[] { x, y };
                    }
                    else
                    {
                        // devo verificar se o agente q quero encontrar não esta atras do hit encontrado
                        while (Physics.Raycast(hit.transform.position, hit.transform.TransformDirection(new Vector3(x, y, 0)), out hit, distance))
                        {
                            if (GameObject.ReferenceEquals(hit.transform.gameObject, b.gameObject))
                            {
                                Debug.DrawRay(hit.transform.position, hit.transform.TransformDirection(new Vector3(x, y, 0) * distance), Color.green);
                                return new float[] { x, y };
                            }
                        }
                    }
                    
                }
                if (Physics.Raycast(a.position, a.TransformDirection(new Vector3(y, x, 0)), out hit, distance))
                {
                    if (GameObject.ReferenceEquals(hit.transform.gameObject, b.gameObject))
                    {
                        Debug.DrawRay(a.position, a.TransformDirection(new Vector3(y, x, 0) * distance), Color.red);
                        return new float[] { y, x };
                    }
                    else
                    {
                        // devo verificar se o agente q quero encontrar não esta atras do hit encontrado
                        while (Physics.Raycast(hit.transform.position, hit.transform.TransformDirection(new Vector3(y, x, 0)), out hit, distance))
                        {
                            if (GameObject.ReferenceEquals(hit.transform.gameObject, b.gameObject))
                            {
                                Debug.DrawRay(hit.transform.position, hit.transform.TransformDirection(new Vector3(y, x, 0) * distance), Color.green);
                                return new float[] { y, x };
                            }
                            else
                            {
                                Debug.DrawRay(hit.transform.position, hit.transform.TransformDirection(new Vector3(y, x, 0) * distance), Color.blue);
                            }
                        }
                    }
                }
            }
        }
        return new float[] { 0, 0};
    }

    public static void CheckDirectionNew(Transform a, Transform b)
    {
        float angle = Vector3.Angle(a.position, (b.position - a.position));
        print("Distance to other: " + (b.position - a.position));
        if (Physics.Raycast(a.position, a.TransformDirection(b.position - a.position), out RaycastHit hit))
            Debug.DrawRay(a.position, a.TransformDirection(b.position - a.position), Color.red);
         else
            Debug.DrawRay(a.position, a.TransformDirection(b.position - a.position), Color.blue);


    }
    


    float FrontSideSensors(Vector3 frontPosition, out RaycastHit hit, float sensorDirection){
        float avoidDirection = 0;
        
        Vector3 sensorPosition = frontPosition+(transform.right*frontSideSensorStartingPoint*sensorDirection);
        Vector3 sensorAngle = Quaternion.AngleAxis(frontSensorAngle*sensorDirection, transform.up)*transform.forward;
        if(Physics.Raycast(sensorPosition, transform.forward, out hit, sensorLength)){
            avoidDirection += 1;
            Debug.DrawLine(sensorPosition, hit.point, Color.black);
        }
        if(Physics.Raycast(sensorPosition, sensorAngle, out hit, sensorLength)){
            avoidDirection += 0.5f;
            Debug.DrawLine(sensorPosition, hit.point, Color.black);
        }
        return avoidDirection;
    }
}
