using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [HideInInspector] public bool isaffectedbygravity = false;

    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("GravitySphere"))
        {
            isaffectedbygravity = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("GravitySphere"))
        {
            isaffectedbygravity = false;
        }
    }
}
