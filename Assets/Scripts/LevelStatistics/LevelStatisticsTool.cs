using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelStatisticsTool : EditorWindow
{
    private float playableAreaSize = 0f;
    private List<Tracker> trackers = new List<Tracker>();
    private string reportTimestamp;
    
    [MenuItem("Tools/Level Statistics Report")]
    public static void ShowWindow()
    {
        GetWindow<LevelStatisticsTool>("Level Statistics Report");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Statistics Report", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Tracker"))
        {
            trackers.Add(new Tracker());
        }

        // Draw each tracker
        for (int i = 0; i < trackers.Count; i++)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Tracker {i + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                trackers.RemoveAt(i);
                continue;
            }
            GUILayout.EndHorizontal();

            trackers[i].DrawGUI();

            GUILayout.EndVertical();
        }

        if (GUILayout.Button("Generate Report"))
        {
            GenerateReport();
        }
        
        if (reportTimestamp != null)
        {
            GUILayout.Label("Scene: " + SceneManager.GetActiveScene().name);
            GUILayout.Label($"Generated At: {reportTimestamp}");
        }
        else
        {
            GUILayout.Label("Click 'Generate Report' to analyse the level.");
        }
        
        if (playableAreaSize > 0)
        {
            GUILayout.Label($"Playable Area Size: {playableAreaSize:F2} units²");
        }
        
        foreach (var tracker in trackers)
        {
            GUILayout.Label($"{tracker.TrackerName}: {tracker.TrackerCount}");
        }

        if (GUILayout.Button("Save Report to File"))
        {
            SaveReportToFile();
        }
    }

    private void GenerateReport()
    {
        // Capture the timestamp
        reportTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // Reset statistics
        playableAreaSize = 0f;

        // Calculate Playable Area
        CalculatePlayableArea();

        // Update each tracker
        foreach (var tracker in trackers)
        {
            tracker.UpdateCount();
        }

        Debug.Log("Level Statistics Report Generated.");
    }

    private void CalculatePlayableArea()
    {
        var ground = GameObject.FindGameObjectWithTag("Ground");
        if (ground != null && ground.TryGetComponent<Collider>(out var groundCollider))
        {
            var bounds = groundCollider.bounds;
            playableAreaSize = bounds.size.x * bounds.size.z;
        }
        else
        {
            Debug.LogWarning("No ground object found with a collider tagged 'Ground'. Playable area size not calculated.");
        }
    }

    private void SaveReportToFile()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("Level Statistics Report");
        report.AppendLine("Scene: " + SceneManager.GetActiveScene().name);
        report.AppendLine($"Generated At: {reportTimestamp}");
        report.AppendLine("=======================");
        report.AppendLine($"Playable Area Size: {playableAreaSize:F2} units²");

        foreach (var tracker in trackers)
        {
            report.AppendLine($"{tracker.TrackerName}: {tracker.TrackerCount}");
        }

        string path = EditorUtility.SaveFilePanel("Save Report", Application.dataPath, "LevelStatisticsReport", "txt");
        if (!string.IsNullOrEmpty(path))
        {
            System.IO.File.WriteAllText(path, report.ToString());
            Debug.Log($"Level Statistics Report saved to {path}");
        }
    }
}

[Serializable]
public class Tracker
{
    public enum TrackerType { Tag, Component, Object }
    public TrackerType Type;
    public string TrackerName;
    public string TagName;
    public MonoScript ComponentScript;
    public GameObject ObjectPrefab;

    public int TrackerCount { get; private set; }

    public void DrawGUI()
    {
        Type = (TrackerType)EditorGUILayout.EnumPopup("Type", Type);

        TrackerName = EditorGUILayout.TextField("Tracker Name", TrackerName);

        switch (Type)
        {
            case TrackerType.Tag:
                TagName = EditorGUILayout.TagField("Tag", TagName);
                break;

            case TrackerType.Component:
                ComponentScript = (MonoScript)EditorGUILayout.ObjectField("Component", ComponentScript, typeof(MonoScript), false);
                break;

            case TrackerType.Object:
                ObjectPrefab = (GameObject)EditorGUILayout.ObjectField("Object Prefab", ObjectPrefab, typeof(GameObject), false);
                break;
        }
    }

    public void UpdateCount()
    {
        TrackerCount = 0;

        switch (Type)
        {
            case TrackerType.Tag:
                TrackerCount = GameObject.FindGameObjectsWithTag(TagName).Length;
                break;

            case TrackerType.Component:
                if (ComponentScript == null)
                {
                    Debug.LogWarning($"No component script assigned for tracker '{TrackerName}'.");
                    break;
                }

                // Get the type of the selected MonoBehaviour
                var componentType = ComponentScript.GetClass();
                if (componentType == null || !typeof(MonoBehaviour).IsAssignableFrom(componentType))
                {
                    Debug.LogWarning($"Invalid MonoBehaviour type for tracker '{TrackerName}'.");
                    break;
                }

                // Count all objects in the scene with this component
                TrackerCount = UnityEngine.Object.FindObjectsOfType(componentType).Length;
                break;

            case TrackerType.Object:
                if (ObjectPrefab != null)
                {
                    TrackerCount = UnityEngine.Object.FindObjectsOfType<GameObject>().Count(obj => obj.name == ObjectPrefab.name);
                }
                break;
        }
    }
}
