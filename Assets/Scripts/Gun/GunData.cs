using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun Data Object", menuName = "Weapon System/Gun Data")]
public class GunData : ScriptableObject
{
    [Header("WeaponProperties")]
    public string weaponname;
    public float damage;
    public float distance;
    public float firerate;
    public float recoilforce;
    public float bullettraveltime;
    public float explosionforce;

    [Header("WeaponEffects")]
    public Vector3 camrecoildirection;
    public GameObject muzzle;
    public GameObject explosion;
    public GameObject trail;
}
