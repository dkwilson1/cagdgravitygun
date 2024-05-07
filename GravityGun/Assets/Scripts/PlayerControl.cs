using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed; // The speed the character moves per update.
    public float lookSpeed; // The look sensitivity for the controller.
    public float jumpHeight; // The amount of force to apply on jump.
    public float holdRange; // The range between the camera and the held object.
    public float fireForce; // The amount of force we give to the held item after firing it.

    private float vertRot = 0f; // Rotation of camera on the vertical axis.
    private float horRot = 0f; // Rotation of character on the horizontal axis

    private Rigidbody charrb; // This character's rigidbody.
    private Camera cam; // The camera for this character.

    private Vector2 moveVector; // The directional vector for movement.

    Rigidbody heldBody; // The rigidbody of the object currently held by the gravity gun.


    private void Awake()
    {
        // Hide and lock the mouse cursor to make the way for first person camera controls.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set camera reference.
        cam = Camera.main;

        // Set rigidbody reference.
        charrb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Returns a rigidbody if a raycast hits one.
    /// </summary>
    private Rigidbody Ray ()
    {
        Rigidbody rb;
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 100))
        {
            if(hit.collider.gameObject.GetComponent<Rigidbody>() != null)
            {
                rb = hit.collider.gameObject.GetComponent<Rigidbody>();
                return rb;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns the location where we want the held body to be located.
    /// </summary>
    private Vector3 getHoldLocation()
    {
        return cam.transform.position + (cam.transform.forward * holdRange);
    }

    /// <summary>
    /// Function to pickup an object by the rigidbody and set it's location to the desired location.
    /// </summary>
    private void PickUp(Rigidbody rb)
    {
        if(heldBody == null)
        {
            heldBody = rb;
            heldBody.useGravity = false;
        }
    }

    /// <summary>
    /// Function to drop the currently held rigidbody. Returns the rigidbody in the case we want to throw it.
    /// </summary>
    private Rigidbody Drop()
    {
        if(heldBody != null)
        {
            Rigidbody rbref = heldBody;
            rbref.useGravity = true;
            heldBody = null;
            return rbref;
        }
        return null;
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
        if(context.performed && charrb.velocity.y == 0) charrb.AddForce(Vector3.up*jumpHeight, ForceMode.Impulse);
    }

    /// <summary>
    /// Function to fire the object the character is holding.
    /// </summary>
    public void OnFire(InputAction.CallbackContext context)
    {
        Rigidbody rbref = Drop();
        if(rbref != null) rbref.AddForce(cam.transform.forward*fireForce, ForceMode.Impulse);
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
            Debug.Log("Picking up "+item.name);
            PickUp(item);
        } else if (heldBody != null)
        {
            // If the gravity gun is holding an item, drop it.
            Debug.Log("Dropping "+heldBody.name);
            Drop();
        }
    }

    private void FixedUpdate()
    {
        // If the controller is inputting movement controls, move the character.
        if (moveVector != Vector2.zero) this.transform.position += (this.transform.forward*moveVector.y + this.transform.right*moveVector.x)*moveSpeed;

        // If we are holding an object, set it's location to stay infront of the camera.
        if (heldBody != null) heldBody.transform.position = getHoldLocation();
    }
}
