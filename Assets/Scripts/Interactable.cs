using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    private float interactionRange = 3f; // Maximum distance to interact
    [SerializeField]
    private string interactionMessage = "Press E to interact"; // Message displayed when in range

    private Transform player; // Reference to the player

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (player == null) return;

        // Check distance to the player
        float distance = Vector3.Distance(player.position, transform.position);
        if (distance <= interactionRange)
        {
            DisplayInteractionMessage();

            // Check for interaction input
            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }
    }

    private void DisplayInteractionMessage()
    {
        Debug.Log(interactionMessage);
    }

    protected virtual void Interact()
    {
        // Placeholder for interaction logic
        Debug.Log($"Interacted with {gameObject.name}");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the interaction range in the Scene View
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
