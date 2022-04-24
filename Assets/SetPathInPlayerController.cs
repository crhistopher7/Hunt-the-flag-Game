using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPathInPlayerController : MonoBehaviour
{
    public void SetPathInPlayer()
    {
        PlayerController playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        playerController.SetPathfinder();
    }
}
