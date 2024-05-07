using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = .2f; // The speed the character moves per update.
    public float lookSpeed = .1f; // The look sensitivity for the controller.
    float jumpHeight = 400f; // The amount of force to apply on jump.

    float vertRot = 0f; // Rotation of camera on the vertical axis.
    float horRot = 0f; // Rotation of character on the horizontal axis

    Rigidbody charrb; // This character's rigidbody.
    public Camera cam; // The camera for this character.

    private Vector2 moveVector; // The directional vector for movement.

    Rigidbody heldBody; // The rigidbody of the object currently held by the gravity gun.


    private void Awake()
    {
        // Hide and lock the mouse cursor to make the way for first person camera controls.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set rigidbody reference.
        charrb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Returns a rigidbody if a raycast hits one.
    /// </summary>
    private Rigidbody Ray ()
    {
        RaycastHit hit;
        Rigidbody rb;
        Debug.DrawRay(cam.transform.position, cam.transform.forward*100, Color.yellow);
        if(Physics.Raycast(cam.transform.position, cam.transform.forward*100, out hit))
        {
            if(hit.collider.gameObject.GetComponent<Rigidbody>() != null)
            {
                rb = hit.collider.gameObject.GetComponent<Rigidbody>();
                return rb;
            }
        }
        return null;
    }

    private void PickUp(Rigidbody rb)
    {
        
    }



    /// <summary>
    ///  Function to read movement input.
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        moveVector = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Function to read look input.
    /// </summary>
    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 myVector = context.ReadValue<Vector2>();
        //Debug.Log(myVector);

        float rotX = myVector.x;
        float rotY = myVector.y;

        vertRot -= rotY * lookSpeed;
        horRot += rotX * lookSpeed;

        // Apply the rotations.
        this.transform.localEulerAngles = Vector3.up * horRot;
        cam.transform.localEulerAngles = Vector3.right * Mathf.Clamp(vertRot, -90f, 90f);
    }

    /// <summary>
    /// Function to read jump input.
    /// </summary>
    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed && charrb.velocity.y == 0) charrb.AddForce(Vector3.up*jumpHeight);
    }

    /// <summary>
    /// Function to fire the object the character is holding.
    /// </summary>
    public void OnFire(InputAction.CallbackContext context)
    {

    }

    /// <summary>
    /// Function to grab the object infront of the character.
    /// </summary>
    public void OnGrab(InputAction.CallbackContext context)
    {
        Rigidbody item = Ray();
        if(item != null && heldBody == null)
        {
            // If the gravity gun is currently not holding an item, and the raycast hits an item, pick up the raycast result.
        } else if (heldBody != null)
        {
            // If the gravity gun is holding an item, drop it.
        }
    }

    private void FixedUpdate()
    {
        // If the controller is inputting movement controls, move the character.
        if (moveVector != Vector2.zero) this.transform.position += (this.transform.forward*moveVector.y + this.transform.right*moveVector.x)*moveSpeed;
    }
}
