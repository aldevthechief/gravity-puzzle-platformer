using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Outline;

public class ObjectGrabbing : MonoBehaviour
{
    public Transform holdpos;
    public LayerMask grablayer;
    [HideInInspector] public GameObject heldobj;
    private Rigidbody heldobjrb;
    private Vector3 distforce;

    public Collider playercoll;
    private GravityController gravityController;

    public float pickuprange = 5f;
    public float pickupforce = 150f;
    public float torquemult;

    void Start()
    {
        gravityController = playercoll.GetComponent<GravityController>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if(heldobj == null)
            {
                RaycastHit hit;
                if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickuprange, grablayer))
                {
                    TakeObject(hit.transform.gameObject);
                }
            }
            else
                ThrowObject();
        }
        if(heldobj != null)
            MoveObject();      
    }

    void FixedUpdate()
    {
        if(heldobj != null)
        {
            heldobjrb.AddForce(distforce);
            Vector3 crossvector = Vector3.Cross(transform.forward, heldobjrb.transform.forward);
            Vector3 planeforward = Vector3.Cross(gravityController.groundup, gravityController.groundright);
            Vector3 resultcross = Vector3.ProjectOnPlane(crossvector, planeforward);
            heldobjrb.AddTorque((resultcross + Vector3.Project(Vector3.Cross(-transform.up, heldobjrb.transform.up), planeforward)) * pickupforce * torquemult * heldobjrb.mass);
        }
    }

    void MoveObject()
    {
        distforce = (holdpos.position - heldobj.transform.position) * pickupforce * heldobjrb.mass;  
    }

    void TakeObject(GameObject pickobj)
    {
        Rigidbody objrb = pickobj.GetComponent<Rigidbody>();
        if(objrb != null)
        {
            heldobjrb = objrb;
            heldobjrb.GetComponent<OutlineBehaviour>().enabled = false;
            Physics.IgnoreCollision(playercoll, heldobjrb.GetComponentInChildren<Collider>(), true);
            heldobjrb.useGravity = false;
            // heldobjrb.interpolation = RigidbodyInterpolation.None;
            heldobjrb.drag = 30f;
            heldobjrb.angularDrag = 10f;
            // heldobjrb.constraints = RigidbodyConstraints.FreezeRotationZ;

            // heldobjrb.transform.parent = holdpos;
            heldobj = pickobj;
        }
    }

    void ThrowObject()
    {
        heldobjrb.GetComponent<OutlineBehaviour>().enabled = true;
        // int grablayerint = (int) (Mathf.Log10(heldobjrb.gameObject.layer) / Mathf.Log10(2));
        Physics.IgnoreCollision(playercoll, heldobjrb.GetComponentInChildren<Collider>(), false);
        // heldobjrb.interpolation = RigidbodyInterpolation.Interpolate;
        heldobjrb.useGravity = true;
        heldobjrb.drag = 0;
        heldobjrb.angularDrag = 0.05f;
        // heldobjrb.constraints = RigidbodyConstraints.None;

        // heldobjrb.transform.parent = null;
        heldobj = null;
    }
}
