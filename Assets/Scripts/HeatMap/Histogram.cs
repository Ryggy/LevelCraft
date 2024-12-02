using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataVisualization;
using UnityEngine;

public class Histogram : MonoBehaviour
{
    public Gradient heatmapGradient; // Assign via Inspector
    public Vector2 gridMin = new Vector2(-10, -10); // Bottom-left corner
    public Vector2 gridMax = new Vector2(10, 10);  // Top-right corner
    public Vector2Int gridResolution = new Vector2Int(50, 50); // Grid resolution

    private BaseHeatmap heatmap;
    private NativeGrid histogramGrid; // Grid to store histogram counts
    private GridTransformation gridTransformation;

    private List<ISensor> sensors = new List<ISensor>();

    public MeshRenderer displayRenderer; // Assign the MeshRenderer of a Quad/Plane

    void Start()
    {
        // Set up grid transformation
        GridTransformation.Settings gridSettings = new GridTransformation.Settings
        {
            resolution = gridResolution,
            minEdge = gridMin,
            maxEdge = gridMax
        };
        gridTransformation = new GridTransformation(gridSettings);

        // Initialize histogram grid
        histogramGrid = new NativeGrid(gridResolution.y, gridResolution.x);

        // Create heatmap for visualization
        heatmap = new BaseHeatmap(histogramGrid, heatmapGradient, 0, 10); // Initial range for color mapping

        // Automatically register sensors in the scene
        RegisterSensors();

        // Assign the texture to the display object
        displayRenderer.material.mainTexture = heatmap.Texture;
    }

    void Update()
    {
        // Clear previous histogram data
        //ClearHistogram();

        // Update histogram based on sensor positions
        foreach (var sensor in sensors)
        {
            Vector3 position = sensor.GetPosition();
            if (IsWithinGridBounds(position))
            {
                int row, column;
                gridTransformation.GlobalPositionToRowColumn(position, out row, out column);
                histogramGrid[row, column]++; // Increment the grid cell count
            }
        }

        // Update heatmap visualization
        heatmap.Update(histogramGrid);
    }

    private void ClearHistogram()
    {
        for (int i = 0; i < histogramGrid.Length; i++)
        {
            histogramGrid[i] = 0;
        }
    }

    private bool IsWithinGridBounds(Vector3 position)
    {
        return position.x >= gridMin.x && position.x <= gridMax.x &&
               position.z >= gridMin.y && position.z <= gridMax.y;
    }

    private void RegisterSensors()
    {
        ISensor[] foundSensors = FindObjectsOfType<MonoBehaviour>().OfType<ISensor>().ToArray();
        sensors.AddRange(foundSensors);
    }

    private void OnDestroy()
    {
        heatmap.Dispose();
        histogramGrid.Dispose();
    }
}
