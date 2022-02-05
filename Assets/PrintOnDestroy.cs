using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintOnDestroy : MonoBehaviour
{
    // Start is called before the first frame update
    void Destroy()
    {
        Debug.Log(gameObject.name + " foi destruido");
    }


}
