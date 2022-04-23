using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPathInPlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPathInPlayer()
    {
        PlayerController playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        playerController.SetPathfinder();
    }
}
