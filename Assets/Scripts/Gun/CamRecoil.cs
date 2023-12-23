using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRecoil : MonoBehaviour
{
    public float rotationSpeed = 6;
    public float returnSpeed = 25;
    private Vector3 currentRotation;
    private Vector3 Rot;

    void Update()
    {
        currentRotation = Vector3.Lerp(currentRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        Rot = Vector3.Slerp(Rot, currentRotation, rotationSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(Rot);
    }

    public void Fire(Vector3 RecoilRotation)
    {
        currentRotation += new Vector3(-RecoilRotation.x, Random.Range(-RecoilRotation.y, RecoilRotation.y), Random.Range(-RecoilRotation.z, RecoilRotation.z));
    }
}
