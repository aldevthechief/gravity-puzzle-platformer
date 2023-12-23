using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewTweaker : MonoBehaviour
{
    private Camera cam;
    public PlayerMovement player;
    public float smooth;
    private float refvalue = 0;

    public float[] fovvalues = {90, 100, 140, 110};

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, fovvalues[(int)player.state], ref refvalue, smooth);
    }
}
