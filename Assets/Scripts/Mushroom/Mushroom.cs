using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Mushroom : MonoBehaviour
{
    public float maxforce = 55f;
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
            if(rb != null && other.gameObject.CompareTag("Player"))
            {
                foreach(Transform i in squeezetransf)
                {
                    i.DOKill(true);
                    i.DOPunchPosition(Vector3.down * squeezeforce.y, squeezetime);
                    i.DOPunchScale(squeezeforce, squeezetime);
                }
                rb.AddForce(Vector3.ClampMagnitude(Vector3.up.normalized * other.relativeVelocity.magnitude * relvelmult, maxforce), ForceMode.Impulse);
                Instantiate(bounceparticles, other.transform.position, Quaternion.identity);
            }
            Invoke("UnColl", timebtwcoll);
        }
    }

    void UnColl()
    {
        hasColl = false;
    }
}
