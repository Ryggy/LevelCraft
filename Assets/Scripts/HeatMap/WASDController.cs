using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WASDController : MonoBehaviour
{
    public float speed = 5f; // Movement speed

    void Update()
    {
        // Get input from WASD or arrow keys
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow
        float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down arrow

        // Calculate movement direction
        Vector3 movement = new Vector3(horizontal, 0f, vertical);

        // Move the object
        transform.Translate(movement * speed * Time.deltaTime, Space.World);
    }
}
