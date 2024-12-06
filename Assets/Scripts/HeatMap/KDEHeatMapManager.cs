using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using DataVisualization;

public class KDEHeatmapManager : MonoBehaviour
{
    public Gradient heatmapGradient;
    public Vector2 gridMin = new Vector2(-10, -10);
    public Vector2 gridMax = new Vector2(10, 10);
    public Vector2Int gridResolution = new Vector2Int(50, 50);
    public float bandwidth = 1.0f;
    public float kernelMaxDistance = 5.0f;

    private BaseHeatmap heatmap;
    private BaseKDE kde;
    private GridTransformation.Settings gridSettings;

    private List<ISensor> sensors = new List<ISensor>(); // Observers
    private PositionDataManager positionDataManager = new PositionDataManager();
    public MeshRenderer displayRenderer;
    
    private string saveDirectory = Application.dataPath + "/../SaveImages/";

    void Start()
    {
        // Set up grid and KDE
        gridSettings = new GridTransformation.Settings
        {
            resolution = gridResolution,
            minEdge = gridMin,
            maxEdge = gridMax
        };

        // Automatically register sensors in the scene
        RegisterSensors();
       
        // Validate sensor positions
        List<Vector3> initialPositions = CollectPositions();
        if (initialPositions.Count == 0)
        {
            Debug.LogWarning("No sensor positions found at initialization. Creating an empty heatmap.");
            InitialiseKDEWithDefaults();
        }
        else
        {
            InitialiseKDE(initialPositions);
        }

        displayRenderer.material.mainTexture = heatmap.Texture;

    }

    void Update()
    {
        // Collect positions and update KDE
        List<Vector3> positions = CollectPositions();

        kde.Update(positions); // Update KDE with current sensor positions
        heatmap.Update(kde.Grid); // Update heatmap texture
        
        // Add cumulative data
        positionDataManager.AddPositions(positions);
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            TakeSnapshot();
        }
    }
    
    public void TakeSnapshot()
    {
        List<Vector3> currentPositions = CollectPositions();
        positionDataManager.TakeSnapshot(currentPositions);
        Debug.Log("Snapshot taken: " + currentPositions.Count + " positions.");
    }

    private void RegisterSensors()
    {
        // Find all GameObjects with ISensor components
        ISensor[] foundSensors = FindObjectsOfType<MonoBehaviour>().OfType<ISensor>().ToArray();
        sensors.AddRange(foundSensors);
    }

    private List<Vector3> CollectPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (var sensor in sensors)
        {
            positions.Add(sensor.GetPosition());
        }
        return positions;
    }
    
    private void InitialiseKDE(List<Vector3> positions)
    {
        kde = new KDE(positions, gridSettings, bandwidth, kernelMaxDistance);
        heatmap = new BaseHeatmap(kde.Grid, heatmapGradient);
    }

    private void InitialiseKDEWithDefaults()
    {
        // Use a grid with zero density for initialisation
        kde = new KDE(new List<Vector3>(), gridSettings, bandwidth, kernelMaxDistance);
        heatmap = new BaseHeatmap(kde.Grid, heatmapGradient, 0, 1); // Default min and max
    }
    
    [ContextMenu("Save Cumulative Heatmap")]
    public void SaveCumulativeHeatmap()
    {
        SaveTexture(heatmap.Texture, "CumulativeHeatmap.png");
        Debug.Log("Cumulative heatmap saved.");
    }

    [ContextMenu("Save Snapshots")]
    public void SaveSnapshots()
    {
        var snapshots = positionDataManager.GetAllSnapshots();

        for (int i = 0; i < snapshots.Count; i++)
        {
            var snapshot = snapshots[i];

            // Create a KDE for the snapshot
            var snapshotKDE = new KDE(snapshot, gridSettings, bandwidth, kernelMaxDistance);
            var snapshotHeatmap = new BaseHeatmap(snapshotKDE.Grid, heatmapGradient);

            SaveTexture(snapshotHeatmap.Texture, $"Snapshot_{i}.png");

            // Dispose of temporary heatmap and KDE
            snapshotHeatmap.Dispose();
            snapshotKDE.Dispose();
        }

        Debug.Log("All snapshots saved.");
    }

    private void SaveTexture(Texture2D texture, string fileName)
    {
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(saveDirectory, fileName), bytes);
    }
    
    private void OnDestroy()
    {
        heatmap.Dispose();
        kde.Dispose();
    }
}