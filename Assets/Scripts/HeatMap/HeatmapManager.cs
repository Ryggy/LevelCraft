using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class HeatmapManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2 gridMin = new Vector2(-10, -10);
    public Vector2 gridMax = new Vector2(10, 10);
    public Vector2Int gridResolution = new Vector2Int(64, 64);

    private Dictionary<Vector2Int, int> heatmapData = new Dictionary<Vector2Int, int>();
    [SerializeField]
    private List<EventLog> eventLogs = new List<EventLog>();
    
    [Header("Tracking Settings")]
    public bool isTracking = false;
    private List<HeatmapTracker> activeTrackers = new List<HeatmapTracker>();
    public Dictionary<string, UnityEvent> eventDictionary = new Dictionary<string, UnityEvent>();
    
    private string logFilePath;

    [System.Serializable]
    private class EventLog
    {
        public Vector3 position;
        public string eventType;
        public float timestamp;

        public EventLog(Vector3 position, string eventType, float timestamp)
        {
            this.position = position;
            this.eventType = eventType;
            this.timestamp = timestamp;
        }
    }

    void Start()
    {
        logFilePath = Path.Combine(Application.dataPath, "LevelStatistics", "HeatmapEventLogs.json");
    }
    
    void Update()
    {
        foreach (var tracker in activeTrackers)
        {
            if (tracker.Type == HeatmapTracker.HeatmapTrackerType.Discrete)
            {
                HandleDiscreteTracker(tracker);
            }
            else if (tracker.Type == HeatmapTracker.HeatmapTrackerType.Continuous)
            {
                HandleContinuousTracker(tracker);
            }
        }
    }

    public void ToggleTracking(bool enable)
    {
        isTracking = enable;

        if (isTracking)
        {
            Debug.Log("Tracking started.");
            heatmapData.Clear();
            eventLogs.Clear();
        }
        else
        {
            Debug.Log("Tracking stopped.");
            SaveLogs();
            GenerateHeatmap();
        }
    }
    
    public void AddTracker(HeatmapTracker tracker)
    {
        if (tracker != null)
        {
            activeTrackers.Add(tracker);
        }
    }

    public void ClearTrackers()
    {
        activeTrackers.Clear();
    }

    private void HandleDiscreteTracker(HeatmapTracker tracker)
    {
        if (tracker.TargetObject != null && tracker.CheckTrigger())
        {
            LogEvent(tracker.TargetObject.transform.position, tracker.EventName);
        }
    }

    private void HandleContinuousTracker(HeatmapTracker tracker)
    {
        if (tracker.TargetObject != null)
        {
            LogEvent(tracker.TargetObject.transform.position, tracker.EventName);
        }
    }
    
    public void InvokeTrackerEvent(string eventName)
    {
        foreach (var tracker in activeTrackers)
        {
            if (tracker.EventName == eventName && tracker.TriggerMode == HeatmapTracker.TriggerType.Event)
            {
                tracker.InvokeEvent();
            }
        }
    }

    public void LogEvent(Vector3 position, string eventType)
    {
        if (!isTracking) return;

        Vector2Int gridPosition = WorldToGrid(new Vector2(position.x, position.z));
        if (!heatmapData.ContainsKey(gridPosition))
        {
            heatmapData[gridPosition] = 1;
        }

        heatmapData[gridPosition]++;
        Debug.Log(eventType + "logged at : "+ position);
        eventLogs.Add(new EventLog(position, eventType, Time.time));
    }

    private void GenerateHeatmap()
    {
        float[,] heatmapGrid = new float[gridResolution.x, gridResolution.y];

        foreach (var kvp in heatmapData)
        {
            heatmapGrid[kvp.Key.x, kvp.Key.y] = kvp.Value;
        }

        HeatmapTextureManager textureManager = GetComponent<HeatmapTextureManager>();
        if (textureManager != null)
        {
            textureManager.UpdateTexture(heatmapGrid);
        }
        else
        {
            Debug.LogError("HeatmapTextureManager is not attached.");
        }
    }

    private void SaveLogs()
    {
        string json = JsonUtility.ToJson(new SerializableEventLogList(eventLogs));
        File.WriteAllText(logFilePath, json);
        AssetDatabase.Refresh();
        Debug.Log($"Event logs saved to {logFilePath}");
    }

    public Vector2Int WorldToGrid(Vector2 worldPosition)
    {
        float cellWidth = (gridMax.x - gridMin.x) / gridResolution.x;
        float cellHeight = (gridMax.y - gridMin.y) / gridResolution.y;

        int x = Mathf.FloorToInt((worldPosition.x - gridMin.x) / cellWidth);
        int y = Mathf.FloorToInt((worldPosition.y - gridMin.y) / cellHeight);

        return new Vector2Int(Mathf.Clamp(x, 0, gridResolution.x - 1), Mathf.Clamp(y, 0, gridResolution.y - 1));
    }

    [System.Serializable]
    private class SerializableEventLogList
    {
        public List<EventLog> eventLogs;

        public SerializableEventLogList(List<EventLog> eventLogs)
        {
            this.eventLogs = eventLogs;
        }
    }
}