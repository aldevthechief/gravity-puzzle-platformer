using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Outline;

public class OutlineHandler : MonoBehaviour
{
    public LayerMask invlayers;
    public Color grabcolor;

    private Transform crosshair;
    private ObjectGrabbing grabscript;

    private OutlineBehaviour prevline;
    private bool grabbingsumn = false;

    void Start()
    {
        crosshair = GameObject.Find("CrosshairSystem").transform;
        grabscript = GetComponent<ObjectGrabbing>();
    }

    void Update()
    {
        crosshair.GetChild(0).gameObject.SetActive(!grabbingsumn);
        crosshair.GetChild(1).gameObject.SetActive(grabbingsumn);

        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 5f, ~invlayers) && !grabscript.heldobj)
        {
            OutlineBehaviour currline = hit.transform.gameObject.GetComponent<OutlineBehaviour>();
            if(currline != null)
            {
                grabbingsumn = true;
                if(prevline != null)
                    prevline.OutlineColor = Color.white;
                prevline = currline;
                prevline.OutlineColor = grabcolor;
            }
            else grabbingsumn = false;
        }
        else 
        {
            grabbingsumn = false;
            if(prevline != null)
                prevline.OutlineColor = Color.white;
        }
    }
}
