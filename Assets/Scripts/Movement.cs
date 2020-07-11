using UnityEngine;
using System.Collections;
 
public class Movement : MonoBehaviour {
 
    //these can be constants
    const float sprintSpeed = 15f;
    const float walkSpeed = 7.5f;
    const float acceleration = 60f;
    const float jumpSpeed = 15f;

    public new Rigidbody rigidbody;
 
    void Start () {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() {
        //GetKeyDown/GetButtonDown will sometimes drop inputs when called from FixedUpdate (which can sometimes be called more often than the input system updates)
        if (Input.GetButtonDown("Jump")) {
            rigidbody.AddForce(Vector3.up * jumpSpeed, ForceMode.VelocityChange);
        }
    }

    void FixedUpdate () {

        //isSprinting and playerSpeed can be declared locally
        bool isSprinting = Input.GetButton("Sprint");
        float playerSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 horizontalVelocity = rigidbody.velocity;
        horizontalVelocity.y = 0;

        Vector3 targetHorizontalVelocity = transform.right * Input.GetAxisRaw("Horizontal") * playerSpeed +
            transform.forward * Input.GetAxisRaw("Vertical") * playerSpeed;

        //rigidbodies should not be moved with transform.Translate
        rigidbody.AddForce(Vector3.ClampMagnitude(targetHorizontalVelocity - horizontalVelocity, Time.deltaTime * acceleration), ForceMode.VelocityChange);
    }
}