using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WASDController : MonoBehaviour
{
    public float speed = 5f; // Movement speed
    Rigidbody myRigidbody;
    Camera viewCamera;
    
    void Start () {
        myRigidbody = GetComponent<Rigidbody> ();
        viewCamera = Camera.main;
    }
    
    void Update()
    {
        // Get input from WASD or arrow keys
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow
        float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down arrow

        // Calculate movement direction
        Vector3 movement = new Vector3(horizontal, 0f, vertical);
        Vector3 mousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
        transform.LookAt (mousePos + Vector3.up * transform.position.y);
        // Move the object
        transform.Translate(movement * speed * Time.deltaTime, Space.World);
    }
}
