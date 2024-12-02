using System.Collections.Generic;
using UnityEngine;
using DataVisualization;

public class SimpleHeatmap : MonoBehaviour
{
    public Gradient heatmapGradient; 
    public Vector2 gridMin = new Vector2(-10, -10); // Bottom-left corner of grid
    public Vector2 gridMax = new Vector2(10, 10);  // Top-right corner of grid
    public Vector2Int gridResolution = new Vector2Int(50, 50); // Grid resolution
    public float bandwidth = 1.0f; // Bandwidth for KDE
    public float kernelMaxDistance = 5.0f; // Max distance for kernel

    private BaseHeatmap heatmap;
    private BaseKDE kde;
    private GridTransformation.Settings gridSettings;
    private List<Vector3> samplePositions;

    public MeshRenderer displayRenderer; // Assign the renderer of a Quad/Plane

    void Start()
    {
        // Set up the grid
        gridSettings = new GridTransformation.Settings
        {
            resolution = gridResolution,
            minEdge = gridMin,
            maxEdge = gridMax
        };

        //  Initialise sample positions
        samplePositions = new List<Vector3>();
        
        // Add random sample positions
        for (int i = 0; i < 100; i++) 
        {
            samplePositions.Add(new Vector3(
                Random.Range(gridMin.x, gridMax.x),
                0,
                Random.Range(gridMin.y, gridMax.y)
            ));
        }

        // Set up KDE
        kde = new KDE(samplePositions, gridSettings, bandwidth, kernelMaxDistance);

        //  Create the heatmap
        heatmap = new BaseHeatmap(kde.Grid, heatmapGradient);

        // Assign the texture to the display object
        displayRenderer.material.mainTexture = heatmap.Texture;
    }

    void Update()
    {
        // Update KDE and heatmap dynamically (e.g., on position changes)
        if (Input.GetKeyDown(KeyCode.Space)) // Example trigger for updating positions
        {
            // Add a new random sample position
            samplePositions.Add(new Vector3(
                Random.Range(gridMin.x, gridMax.x),
                0,
                Random.Range(gridMin.y, gridMax.y)
            ));

            // Update KDE with new positions
            kde.Update(samplePositions);

            // Update heatmap texture
            heatmap.Update(kde.Grid);
        }
    }

    private void OnDestroy()
    {
        // Clean up resources
        heatmap.Dispose();
        kde.Dispose();
    }
}

