using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunTipSnapper : MonoBehaviour
{
    public LayerMask groundlayer;
    public Transform tip;
    public Transform tiproot;
    public Transform castpos;
    public float raylength = 3;
    
    void Update()
    {
        if(Physics.Raycast(castpos.position, transform.forward, out RaycastHit hit, raylength, groundlayer))
        {
            tip.position = Vector3.Lerp(tip.position, hit.point + hit.normal * hit.distance, Time.deltaTime * 3);
        }
        else
        {
            tip.position = Vector3.Lerp(tip.position, tiproot.position, Time.deltaTime * 3);
        }
    }
}
