using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Mushroom : MonoBehaviour
{
    public float minforce = 15f, maxforce = 55f;
    public float relvelmult = 1f;
    public Vector3 squeezeforce;
    public float squeezetime;
    public Transform[] squeezetransf;
    public GameObject bounceparticles;

    public float timebtwcoll = 0.25f;
    private bool hasColl = false;

    void OnCollisionEnter(Collision other)
    {
        if(!hasColl)
        {
            hasColl = true;
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            GravityController gravctrl = GameObject.FindGameObjectWithTag("Player").GetComponent<GravityController>();
            if(rb != null && other.gameObject.CompareTag("Player"))
            {
                foreach(Transform i in squeezetransf)
                {
                    i.DOKill(true);
                    i.DOPunchPosition(Vector3.down * squeezeforce.y, squeezetime);
                    i.DOPunchScale(squeezeforce, squeezetime);
                }

                PlayerMovement player = rb.GetComponent<PlayerMovement>();
                if(gravctrl != null)
                {
                    StartCoroutine(player.BlockForces(0.25f));
                    Vector3 dir = gravctrl.groundup;
                    rb.AddForce(dir * Mathf.Clamp(other.relativeVelocity.magnitude, minforce, maxforce) * relvelmult, ForceMode.Impulse);
                }  

                Instantiate(bounceparticles, other.transform.position, Quaternion.identity);
            }
            // code to make any other rigidbody go up
            // else if(rb != null)
            // {
            //     foreach(Transform i in squeezetransf)
            //     {
            //         i.DOKill(true);
            //         i.DOPunchPosition(Vector3.down * squeezeforce.y, squeezetime);
            //         i.DOPunchScale(squeezeforce, squeezetime);
            //     }

            //     Vector3 dir = gravctrl.groundup;
            //     rb.AddForce(dir * Mathf.Clamp(other.relativeVelocity.magnitude, minforce, maxforce) * relvelmult, ForceMode.Impulse);

            //     Instantiate(bounceparticles, other.transform.position, Quaternion.identity);
            // }
            Invoke("UnColl", timebtwcoll);
        }
    }

    void UnColl()
    {
        hasColl = false;
    }
}
