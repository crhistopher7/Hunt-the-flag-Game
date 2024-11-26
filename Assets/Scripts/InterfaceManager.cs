using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    Transform CanvasConfigurationsTransform;
    // Start is called before the first frame update
    void Start()
    {
        Transform parentTransform = transform.parent;
        CanvasConfigurationsTransform = parentTransform.Find("CanvasConfigurations");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (CanvasConfigurationsTransform != null)
            {
                bool isActive = CanvasConfigurationsTransform.gameObject.activeSelf;
                CanvasConfigurationsTransform.gameObject.SetActive(!isActive);
            }
        }
    }
}
