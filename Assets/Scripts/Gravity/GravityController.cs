using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    [Header("gravity walls")]
    public bool isgravityrotallowed = true;
    public float gravaccel = -40;
    [HideInInspector] public Vector3 groundup = Vector3.up;
    [HideInInspector] public Vector3 groundright = Vector3.right;
    private PlayerMovement movement;

    [Header("gravity sphere")]
    [HideInInspector] public bool isaffectedbygravity = false;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Ground") && isgravityrotallowed)
        {
            groundup = other.contacts[0].normal;
            groundright = Vector3.RotateTowards(groundup, -groundup, Mathf.PI / 2, 0);
            Physics.gravity = groundup * gravaccel;
            movement.NormalRotation();
        }
    }

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
