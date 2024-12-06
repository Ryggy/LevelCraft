using System;
using UnityEngine;

public class PlayerSensor : MonoBehaviour
{
    public event Action<Vector2Int> OnGridPositionUpdated;
    public event Action<Vector2Int> OnInteraction;

    private Vector2Int lastGridPosition;
    private HeatmapManager heatmapManager;

    void Start()
    {
        // Find the HeatmapManager
        heatmapManager = FindObjectOfType<HeatmapManager>();
        if (heatmapManager == null)
        {
            Debug.LogError("HeatmapManager not found!");
            return;
        }

        // Initialize the last grid position
        UpdateGridPosition();
    }

    void Update()
    {
        // Check if the sensor has moved to a new grid position
        if (UpdateGridPosition())
        {
            // Notify listeners of the updated grid position
            OnGridPositionUpdated?.Invoke(lastGridPosition);
        }
        
        // Check for interaction key press
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnInteraction?.Invoke(lastGridPosition);
        }
    }

    /// <summary>
    /// Updates the grid position based on the current world position.
    /// Returns true if the grid position has changed.
    /// </summary>
    private bool UpdateGridPosition()
    {
        if (heatmapManager == null) return false;

        Vector2 worldPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2Int currentGridPosition = heatmapManager.WorldToGrid(worldPosition);

        if (currentGridPosition != lastGridPosition)
        {
            lastGridPosition = currentGridPosition;
            return true;
        }

        return false;
    }
}