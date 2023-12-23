using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Autokill : MonoBehaviour
{
    public float time = 1.5f;

    void Start()
    {
        Destroy(gameObject, time);
    }
}    
