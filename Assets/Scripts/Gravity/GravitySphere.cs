using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitySphere : MonoBehaviour
{
    public float gravitymult = 0;
    private float gravityvalue;

    void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();
        if(rb != null)
        {
            rb.useGravity = false;
            gravityvalue += gravitymult * Time.fixedDeltaTime;
            rb.AddForce(Vector3.down * gravityvalue, ForceMode.Acceleration);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();
        if(rb != null)
            rb.useGravity = true;
    }
}
