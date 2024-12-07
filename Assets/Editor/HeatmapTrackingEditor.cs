using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HeatmapTrackingEditor : EditorWindow
{
    private HeatmapManager heatmapManager;

    private List<HeatmapTracker> trackers = new List<HeatmapTracker>();
    private List<string> activeTrackers = new List<string>();

    private bool realTimeHeatmap = false;
    private bool previousRealTimeHeatmap = false; // Store the previous state of the toggle
    
    [MenuItem("Tools/Heatmap Tracking Editor")]
    public static void ShowWindow()
    {
        GetWindow<HeatmapTrackingEditor>("Heatmap Tracking");
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        FindHeatmapManager();
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
        {
            FindHeatmapManager();
        }
    }

    private void FindHeatmapManager()
    {
        heatmapManager = FindObjectOfType<HeatmapManager>();

        if (heatmapManager == null)
        {
            Debug.LogWarning("HeatmapManager not found in the scene. Ensure it is added.");
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Heatmap Tracking Tool", EditorStyles.boldLabel);

        if (heatmapManager == null)
        {
            GUILayout.Label("HeatmapManager is not found in the scene.");
            if (GUILayout.Button("Retry"))
            {
                FindHeatmapManager();
            }
            return;
        }

        // Start/Stop Tracking
        if (GUILayout.Button(heatmapManager.isTracking ? "Stop Tracking" : "Start Tracking"))
        {
            heatmapManager.ToggleTracking(!heatmapManager.isTracking);
        }

        GUILayout.Space(10);

        GUILayout.Label("Trackers", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Tracker"))
        {
            trackers.Add(new HeatmapTracker());
        }

        for (int i = 0; i < trackers.Count; i++)
        {
            GUILayout.BeginVertical("box");
            trackers[i].DrawTrackerUI();

            if (GUILayout.Button("Remove Tracker"))
            {
                trackers.RemoveAt(i);
                break;
            }
            GUILayout.EndVertical();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Apply Trackers"))
        {
            ApplyTrackers();
        }

        GUILayout.Space(10);

        // Real-time Heatmap toggle
        realTimeHeatmap = GUILayout.Toggle(realTimeHeatmap, "Enable Real-Time Heatmap Generation");
        
        if (realTimeHeatmap)
        {
            EditorGUILayout.HelpBox("Enabling real-time heatmap generation may impact performance.", MessageType.Warning);
        }

        // Call ToggleRealTimeUpdates only if the toggle changes
        if (realTimeHeatmap != previousRealTimeHeatmap)
        {
            if (heatmapManager != null)
            {
                heatmapManager.ToggleRealTimeUpdates(realTimeHeatmap);
                Debug.Log($"Real-Time Heatmap Generation set to: {realTimeHeatmap}");
            }
            else
            {
                Debug.LogWarning("HeatmapManager not found. Unable to toggle real-time updates.");
            }

            previousRealTimeHeatmap = realTimeHeatmap; // Update the previous state
        }

        GUILayout.Space(20);

        GUILayout.Label("Active Trackers", EditorStyles.boldLabel);
        if (activeTrackers.Count == 0)
        {
            GUILayout.Label("No trackers applied.", EditorStyles.helpBox);
        }
        else
        {
            foreach (string tracker in activeTrackers)
            {
                GUILayout.Label($"- {tracker}", EditorStyles.label);
            }
        }
        
        GUILayout.Space(20);

        // Load Previous Heatmap Button
        if (GUILayout.Button("Load Previous Heatmap"))
        {
            if (heatmapManager != null)
            {
                heatmapManager.LoadPreviousHeatmap();
            }
            else
            {
                Debug.LogWarning("HeatmapManager not found. Unable to load previous heatmap.");
            }
        }

        GUILayout.Space(10);

        // Clear Heatmap Button
        if (GUILayout.Button("Clear Heatmap"))
        {
            if (heatmapManager != null)
            {
                heatmapManager.ClearHeatmap();
                Debug.Log("Heatmap has been cleared.");
            }
            else
            {
                Debug.LogWarning("HeatmapManager not found. Unable to clear heatmap.");
            }
        }
    }

    private void ApplyTrackers()
    {
        activeTrackers.Clear();

        if (heatmapManager == null)
        {
            Debug.LogError("HeatmapManager not found.");
            return;
        }

        heatmapManager.ClearTrackers();

        foreach (var tracker in trackers)
        {
            heatmapManager.AddTracker(tracker);
            activeTrackers.Add($"{tracker.EventName} ({tracker.Type})");
        }

        Debug.Log("Trackers applied.");
    }
}