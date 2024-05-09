using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Explode : MonoBehaviour
{
    public float minImpulse; // The minimum amount of impulse needed to explode.
    public ParticleSystem ps; // Particle system for the explosion.
    private ParticleSystem curParticle; // The current particle as it is spawned.

    private Rigidbody rb;
    private MeshFilter mF;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mF = this.gameObject.GetComponent<MeshFilter>();
    }

    private void Update()
    {
        // Once the explosion particle has depsawn. Destroy this object.
        if (mF == null && curParticle == null) Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.impulse.magnitude >= minImpulse && curParticle == null)
        {
            Debug.Log("Explode " + collision.impulse.magnitude);
            rb.isKinematic = true;
            this.gameObject.transform.localScale = Vector3.one;
            Instantiate(ps, this.transform);
            this.gameObject.GetComponent<MeshFilter>().mesh = null;
        }
    }
}
