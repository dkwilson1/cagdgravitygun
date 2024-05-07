using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed; // The speed the character moves per update.
    public float lookSpeed; // The look sensitivity for the controller.
    public float jumpHeight; // The amount of force to apply on jump.
    public float holdRange; // The range between the camera and the held object.
    public float fireForce; // The amount of force we give to the held item after firing it.
    public float grabDistance; // The distance in which you can pickup an item. Raycast will be 3x this value, and if the raycast hits the object, but it is not close enough, we will pull it towards us instead.
    public float pullForce; // The amount of force to add when pulling an item.
    public float massLimit; // The limit of mass we can pick up.

    private float vertRot = 0f; // Rotation of camera on the vertical axis.
    private float horRot = 0f; // Rotation of character on the horizontal axis

    private Rigidbody charrb; // This character's rigidbody.
    private Rigidbody pullBody; // The rigidbody being pulled.
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
        if(heldBody == null && rb.mass<=massLimit)
        {
            heldBody = rb;
            // Clear velocity and turn off gravity (so it doesn't fall down while grabbed.)
            heldBody.useGravity = false;
            heldBody.velocity = Vector3.zero;
            heldBody.angularVelocity = Vector3.zero;
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
        if(context.performed) {
            Rigidbody rbref = Drop();
            if (rbref != null) rbref.AddForce(cam.transform.forward * fireForce, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Function to grab the object infront of the character.
    /// </summary>
    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Rigidbody item = Ray();
            if (heldBody != null)
            {
                // If the gravity gun is holding an item, drop it.
                Debug.Log("Dropping " + heldBody.name);
                Drop();
            }
            else if (item != null && heldBody == null)
            {
                if(Vector3.Distance(this.transform.position, item.position) <= grabDistance)
                {
                    Debug.Log(Vector3.Distance(this.transform.position, item.position));
                    // Close enough. Grab the item.
                    Debug.Log("Picking up " + item.name);
                    PickUp(item);
                } else if (Vector3.Distance(this.transform.position, item.position) <= grabDistance*3)
                {
                    // Not close enough. Start pulling instead.
                    Debug.Log("Pulling " + item.name);
                    pullBody = item;
                }
                // If the gravity gun is currently not holding an item, and the raycast hits an item, pick up the raycast result.
            }
        }
        else if (context.canceled)
        {
            // If we are pulling an object, stop pulling it.
            if (pullBody != null) pullBody = null;

        }
    }

    private void FixedUpdate()
    {
        // If the controller is inputting movement controls, move the character.
        if (moveVector != Vector2.zero) this.transform.position += (this.transform.forward*moveVector.y + this.transform.right*moveVector.x)*moveSpeed;

        // If we are holding an item, set it's location to stay infront of the camera. If not holding an item, and we are pulling on. Add force to the body of the one we're pulling.
        if (heldBody != null) heldBody.transform.position = getHoldLocation();
        else if (pullBody != null) {
            // If the item with the pullBody gets close enough, pick it up.
            pullBody.AddForce((this.transform.position-pullBody.transform.position).normalized * (pullForce*(pullBody.mass*Mathf.Pow(.96f, pullBody.mass-1))));
            if (Vector3.Distance(this.transform.position, pullBody.position) <= grabDistance)
            {
                PickUp(pullBody);
                pullBody = null;
            }
        }
    }
}
