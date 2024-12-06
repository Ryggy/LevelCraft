using System.Collections.Generic;
using UnityEngine;

public class HeatmapManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2 gridMin = new Vector2(-10, -10); // Bottom-left corner of the grid
    public Vector2 gridMax = new Vector2(10, 10);  // Top-right corner of the grid
    public Vector2Int gridResolution = new Vector2Int(256, 256); // Grid resolution

    private float[,] positionGrid; // 2D array for storing position intensity values
    private float[,] interactionGrid; // 2D array for storing interaction intensity values
    
    private HeatmapTextureManager textureManager;

    private List<PlayerSensor> sensors = new List<PlayerSensor>(); // Registered sensors

    private enum ActiveGrid
    {
        Position,
        Interaction
    }
    
    private ActiveGrid currentGrid = ActiveGrid.Position;
    
    void Start()
    {
        // Initialize the data grids
        positionGrid = new float[gridResolution.x, gridResolution.y];
        interactionGrid = new float[gridResolution.x, gridResolution.y];

        // Find the texture manager
        textureManager = GetComponent<HeatmapTextureManager>();
        if (textureManager == null)
        {
            Debug.LogError("HeatmapTextureManager is required.");
            return;
        }

        // Register sensors in the scene
        RegisterSensors();
        
        // Initially update the texture with the position grid
        textureManager.UpdateTexture(positionGrid);
    }

    void Update()
    {
        // Handle switching grids
        if (Input.GetKeyDown(KeyCode.Comma)) // <
        {
            currentGrid = ActiveGrid.Position;
        }
        else if (Input.GetKeyDown(KeyCode.Period)) // >
        {
            currentGrid = ActiveGrid.Interaction;
        }
        
        UpdateGridTexture();
    }

    /// <summary>
    /// Switches to the specified grid and updates the texture.
    /// </summary>
    private void UpdateGridTexture()
    {
        switch (currentGrid)
        {
            case ActiveGrid.Position:
                textureManager.UpdateTexture(positionGrid);
                break;

            case ActiveGrid.Interaction:
                textureManager.UpdateTexture(interactionGrid);
                break;
        }
    }
    
    /// <summary>
    /// Registers all PlayerSensors in the scene.
    /// </summary>
    private void RegisterSensors()
    {
        PlayerSensor[] foundSensors = FindObjectsOfType<PlayerSensor>();
        foreach (var sensor in foundSensors)
        {
            sensors.Add(sensor);
            sensor.OnGridPositionUpdated += HandleSensorGridPositionUpdated;
            sensor.OnInteraction += HandleSensorInteraction;
        }
    }

    /// <summary>
    /// Handles grid position updates from PlayerSensors.
    /// </summary>
    private void HandleSensorGridPositionUpdated(Vector2Int gridPosition)
    {
        IncrementCell(positionGrid, gridPosition.x, gridPosition.y, Time.deltaTime);
    }
    
    /// <summary>
    /// Handles interaction events from PlayerSensors.
    /// </summary>
    private void HandleSensorInteraction(Vector2Int gridPosition)
    {
        IncrementCell(interactionGrid, gridPosition.x, gridPosition.y, 1f);
    }

    /// <summary>
    /// Increments the value of a grid cell in the specified grid.
    /// </summary>
    private void IncrementCell(float[,] grid, int x, int y, float amount)
    {
        if (x >= 0 && x < gridResolution.x && y >= 0 && y < gridResolution.y)
        {
            grid[x, y] += amount;
        }
    }

    /// <summary>
    /// Converts a world position to grid coordinates.
    /// </summary>
    public Vector2Int WorldToGrid(Vector2 worldPosition)
    {
        float cellWidth = (gridMax.x - gridMin.x) / gridResolution.x;
        float cellHeight = (gridMax.y - gridMin.y) / gridResolution.y;

        int x = Mathf.FloorToInt((worldPosition.x - gridMin.x) / cellWidth);
        int y = Mathf.FloorToInt((worldPosition.y - gridMin.y) / cellHeight);

        return new Vector2Int(Mathf.Clamp(x, 0, gridResolution.x - 1), Mathf.Clamp(y, 0, gridResolution.y - 1));
    }

    private void OnDestroy()
    {
        foreach (var sensor in sensors)
        {
            sensor.OnGridPositionUpdated -= HandleSensorGridPositionUpdated;
            sensor.OnInteraction -= HandleSensorInteraction;
        }
    }
}
