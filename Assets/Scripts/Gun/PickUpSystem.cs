using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Outline;

public class PickUpSystem : MonoBehaviour
{
    public Transform cam;
    public LayerMask defaultgunlayer;
    public float smooth;
    public float throwforce;

    private GameObject grabbedgun;
    private Rigidbody grabbedgunrb;
    private Vector3 refvel = Vector3.zero;
    private Vector3 desiredpos = Vector3.zero;
    private Quaternion desiredrot = Quaternion.identity;

    void Update()
    {

        if(Input.GetKeyDown(KeyCode.Q))
        {
            if(grabbedgun == null)
            {
                RaycastHit hit;
                if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 5f, defaultgunlayer))
                {
                    if(hit.transform.gameObject.GetComponentInParent<Shotgun>() != null)
                    {
                        grabbedgunrb = hit.transform.gameObject.GetComponentInParent<Rigidbody>();
                        TakeObject();
                    }
                }
            }
            else
                ThrowObject();
        }

        if(grabbedgun != null)
        {
            grabbedgun.transform.localRotation = Quaternion.Slerp(grabbedgunrb.transform.localRotation, desiredrot, Time.deltaTime * smooth);
            grabbedgun.transform.localPosition = Vector3.SmoothDamp(grabbedgunrb.transform.localPosition, desiredpos, ref refvel, 1 / smooth);
        }  
    }

    void TakeObject()
    {
        grabbedgunrb.transform.SetParent(transform);

        MonoBehaviour[] components = grabbedgunrb.gameObject.GetComponents<MonoBehaviour>();
        foreach(MonoBehaviour i in components)
        {
            i.enabled = true;
        }

        grabbedgunrb.GetComponent<OutlineBehaviour>().enabled = false;
        foreach(Transform i in grabbedgunrb.transform)
        {
            i.gameObject.layer = LayerMask.NameToLayer("GunLayer");
        }

        MeshCollider mesh = grabbedgunrb.GetComponentInChildren<MeshCollider>();
        mesh.enabled = false;
        BoxCollider[] boxes = grabbedgunrb.GetComponentsInChildren<BoxCollider>();
        foreach(BoxCollider i in boxes)
        {
            i.enabled = false;
        }
        
        grabbedgunrb.isKinematic = true;
        grabbedgunrb.useGravity = false;
        grabbedgunrb.interpolation = RigidbodyInterpolation.None;
        grabbedgun = grabbedgunrb.gameObject; 
    }

    void ThrowObject()
    {
        grabbedgunrb.transform.SetParent(null);

        MonoBehaviour[] components = grabbedgunrb.gameObject.GetComponents<MonoBehaviour>();
        foreach(MonoBehaviour i in components)
        {
            i.enabled = false;
        }

        grabbedgunrb.GetComponent<OutlineBehaviour>().enabled = true;
        foreach(Transform i in grabbedgunrb.transform)
        {
            i.gameObject.layer = LayerMask.NameToLayer("DefaultGunLayer");
        }

        MeshCollider mesh = grabbedgunrb.GetComponentInChildren<MeshCollider>();
        mesh.enabled = true;
        BoxCollider[] boxes = grabbedgunrb.GetComponentsInChildren<BoxCollider>();
        foreach(BoxCollider i in boxes)
        {
            i.enabled = true;
        }

        grabbedgunrb.isKinematic = false;
        grabbedgunrb.useGravity = true;
        grabbedgunrb.interpolation = RigidbodyInterpolation.Interpolate;
        grabbedgunrb.AddForce(transform.forward * throwforce, ForceMode.Impulse);
        grabbedgun = null; 
    }
}
