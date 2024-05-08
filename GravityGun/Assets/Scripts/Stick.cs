using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When an item hits a surface at high enough speeds, it will stick there instead of bouncing off.
/// </summary>
public class Stick : MonoBehaviour
{
    public float minImpulse; // The minimum amount of impulse needed to stick.

    Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.impulse.magnitude >= minImpulse)
        {
            Debug.Log("Stick " + rb.velocity);
            rb.gameObject.transform.position += rb.velocity * .02f;
            rb.isKinematic = true;
        }
    }
}
