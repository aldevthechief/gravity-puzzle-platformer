using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Outline;

public class EssentialsPickUp : MonoBehaviour
{
    public Transform cam;
    public LayerMask defaultobjlayer;
    public float smooth;
    public float throwforce;

    private GameObject grabbedobj;
    private Rigidbody grabbedobjrb;
    private Vector3 refvel = Vector3.zero;
    private Vector3 desiredpos = Vector3.zero;
    private Quaternion desiredrot = Quaternion.identity;

    void Update()
    {

        if(Input.GetKeyDown(KeyCode.Q))
        {
            if(grabbedobj == null)
            {
                RaycastHit hit;
                if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 5f, defaultobjlayer))
                {
                    // if(hit.transform.gameObject.GetComponentInParent<Shotgun>() != null)
                    // {
                        grabbedobjrb = hit.transform.gameObject.GetComponentInParent<Rigidbody>();
                        TakeObject();
                    // }
                }
            }
            else
                ThrowObject();
        }

        if(grabbedobj != null)
        {
            grabbedobj.transform.localRotation = Quaternion.Slerp(grabbedobjrb.transform.localRotation, desiredrot, Time.deltaTime * smooth);
            grabbedobj.transform.localPosition = Vector3.SmoothDamp(grabbedobjrb.transform.localPosition, desiredpos, ref refvel, 1 / smooth);
        }  
    }

    void TakeObject()
    {
        grabbedobjrb.transform.SetParent(transform);

        MonoBehaviour[] components = grabbedobjrb.gameObject.GetComponents<MonoBehaviour>();
        foreach(MonoBehaviour i in components)
        {
            i.enabled = true;
        }

        grabbedobjrb.GetComponent<OutlineBehaviour>().enabled = false;
        foreach(Transform i in grabbedobjrb.transform)
        {
            i.gameObject.layer = LayerMask.NameToLayer("ClockLayer");
        }

        MeshCollider mesh = grabbedobjrb.GetComponentInChildren<MeshCollider>();
        mesh.enabled = false;
        BoxCollider[] boxes = grabbedobjrb.GetComponentsInChildren<BoxCollider>();
        foreach(BoxCollider i in boxes)
        {
            i.enabled = false;
        }
        
        grabbedobjrb.isKinematic = true;
        grabbedobjrb.useGravity = false;
        grabbedobjrb.interpolation = RigidbodyInterpolation.None;
        grabbedobj = grabbedobjrb.gameObject; 
    }

    void ThrowObject()
    {
        grabbedobjrb.transform.SetParent(null);

        MonoBehaviour[] components = grabbedobjrb.gameObject.GetComponents<MonoBehaviour>();
        foreach(MonoBehaviour i in components)
        {
            i.enabled = false;
        }

        grabbedobjrb.GetComponent<OutlineBehaviour>().enabled = true;
        foreach(Transform i in grabbedobjrb.transform)
        {
            i.gameObject.layer = LayerMask.NameToLayer("DefaultClockLayer");
        }

        MeshCollider mesh = grabbedobjrb.GetComponentInChildren<MeshCollider>();
        mesh.enabled = true;
        BoxCollider[] boxes = grabbedobjrb.GetComponentsInChildren<BoxCollider>();
        foreach(BoxCollider i in boxes)
        {
            i.enabled = true;
        }

        grabbedobjrb.isKinematic = false;
        grabbedobjrb.useGravity = true;
        grabbedobjrb.interpolation = RigidbodyInterpolation.Interpolate;
        grabbedobjrb.AddForce(transform.forward * throwforce, ForceMode.Impulse);
        grabbedobj = null; 
    }
}
