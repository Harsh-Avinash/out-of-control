
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {

    public new Rigidbody rigidbody;
    public Transform shootHole;
    public ParticleSystem muzzleFlash;

    const float shootForce = 5;

    void Update() {
        if (Input.GetButtonDown("Fire1")) {
            rigidbody.AddForceAtPosition(-shootHole.forward * shootForce, shootHole.position, ForceMode.VelocityChange);
            muzzleFlash.Play();
        }
    }
}