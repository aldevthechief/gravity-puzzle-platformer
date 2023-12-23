using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : MonoBehaviour
{
    [Header("GunInfo")]
    public GunData gundata;

    private float lastshottime;
    private RaycastHit hit;

    [Header("References")]
    public Rigidbody playerrb;
    public Transform cam;
    public Transform guntip;
    public LayerMask ignoreshooting;
    private WeaponRecoil weaponrecoil;
    private CamRecoil camrecoil;

    private bool CanShoot() => lastshottime > 1 / (gundata.firerate / 60);

    void Start()
    {
        weaponrecoil = GetComponent<WeaponRecoil>();
        camrecoil = gameObject.GetComponentInParent<CamRecoil>();
    }

    void Update()
    {
        lastshottime += Time.deltaTime;
        if(Input.GetMouseButtonDown(0))
            Shoot();
    }

    void Shoot()
    {
        if(CanShoot())
        {
            if(Physics.Raycast(cam.position, cam.forward, out hit, gundata.distance, ~ignoreshooting))
            {
                playerrb.AddForce(-cam.transform.forward * gundata.recoilforce, ForceMode.Impulse);
                weaponrecoil.Recoil();
                camrecoil.Fire(gundata.camrecoildirection);
                Instantiate(gundata.muzzle, guntip.position, guntip.rotation);

                TrailRenderer trail = Instantiate(gundata.trail.GetComponent<TrailRenderer>(), guntip.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true));
            }
            else
            {
                playerrb.AddForce(-cam.transform.forward * gundata.recoilforce, ForceMode.Impulse);
                weaponrecoil.Recoil();
                camrecoil.Fire(gundata.camrecoildirection);
                Instantiate(gundata.muzzle, guntip.position, guntip.rotation);

                TrailRenderer trail = Instantiate(gundata.trail.GetComponent<TrailRenderer>(), guntip.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, cam.transform.forward * 1000f, -cam.transform.forward, false));
            }
            lastshottime = 0;
        }
    }

    IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, bool MadeImpact)
    {
        Vector3 startPosition = Trail.transform.position;
        float distance = Vector3.Distance(Trail.transform.position, HitPoint);
        float remainingDistance = distance;

        while(remainingDistance > 0)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (remainingDistance / distance));
            remainingDistance -= gundata.bullettraveltime * Time.deltaTime;
            yield return null;
        }

        Trail.transform.position = HitPoint;
        if(MadeImpact)
        {
            Instantiate(gundata.explosion, HitPoint, Quaternion.LookRotation(HitNormal));

            Rigidbody hitrb = hit.rigidbody;
            if(hitrb != null)
            {
                hitrb.AddForceAtPosition(-HitNormal * gundata.explosionforce, HitPoint, ForceMode.Impulse);
            }
        }
        Destroy(Trail.gameObject, Trail.time);
    }
}
