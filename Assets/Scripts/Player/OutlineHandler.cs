using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Outline;

public class OutlineHandler : MonoBehaviour
{
    public LayerMask invlayers;
    public Color grabcolor;
    private Transform crosshair;
    private OutlineBehaviour line;
    private bool ishit;

    void Start()
    {
        crosshair = GameObject.Find("CrosshairSystem").transform;
    }

    void Update()
    {
        crosshair.GetChild(0).gameObject.SetActive(!ishit);
        crosshair.GetChild(1).gameObject.SetActive(ishit);
        
        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 5f, ~invlayers))
        {
            line = hit.transform.gameObject.GetComponent<OutlineBehaviour>();
            if(line != null)
            {
                line.OutlineColor = grabcolor;
                ishit = true;
            }
        }
        else if(ishit)
        {
            ishit = false;
            if(line != null)
                line.OutlineColor = Color.white;
        }
    }
}
