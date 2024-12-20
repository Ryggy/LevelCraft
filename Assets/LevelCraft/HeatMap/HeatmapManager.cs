using System.Collections;
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
    
    [Header("Real-Time Heatmap Settings")]
    public bool isRealTimeEnabled = false; // Toggle to enable/disable real-time updates
    public float realTimeUpdateDelay = 1f; // Delay between each real-time update
    private Coroutine realTimeCoroutine;
    
    private string logFilePath
    {
        get
        {
            return Path.Combine(Application.dataPath, "LevelStatistics", "HeatmapEventLogs.json");
        }
    }

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
            heatmapData[gridPosition] = 0;
        }

        heatmapData[gridPosition]++;
        //Debug.Log(eventType + "logged at : "+ position);
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
    
    public void ClearHeatmap()
    {
        // Clear the heatmap data
        heatmapData.Clear();
        eventLogs.Clear();

        // Clear the heatmap texture
        HeatmapTextureManager textureManager = GetComponent<HeatmapTextureManager>();
        if (textureManager != null)
        {
            textureManager.ClearTexture();
        }
        else
        {
            Debug.LogError("HeatmapTextureManager is not attached.");
        }

        Debug.Log("Heatmap data and event logs have been cleared.");
    }
    
    /// <summary>
    /// Starts or stops real-time heatmap updates.
    /// </summary>
    /// <param name="enable">Enable or disable real-time heatmap updates.</param>
    public void ToggleRealTimeUpdates(bool enable)
    {
        isRealTimeEnabled = enable;

        if (isRealTimeEnabled)
        {
            if (realTimeCoroutine != null)
            {
                StopCoroutine(realTimeCoroutine);
            }
            realTimeCoroutine = StartCoroutine(RealTimeHeatmapUpdate());
            Debug.Log("Real-time heatmap updates enabled.");
        }
        else
        {
            if (realTimeCoroutine != null)
            {
                StopCoroutine(realTimeCoroutine);
                realTimeCoroutine = null;
            }
            Debug.Log("Real-time heatmap updates disabled.");
        }
    }

    /// <summary>
    /// Coroutine that generates the heatmap at regular intervals.
    /// </summary>
    private IEnumerator RealTimeHeatmapUpdate()
    {
        while (isRealTimeEnabled)
        {
            GenerateHeatmap();
            yield return new WaitForSeconds(realTimeUpdateDelay);
        }
    }

    private void SaveLogs()
    {
        string json = JsonUtility.ToJson(new SerializableEventLogList(eventLogs));
        File.WriteAllText(logFilePath, json);
        AssetDatabase.Refresh();
        Debug.Log($"Event logs saved to {logFilePath}");
    }
    
    /// <summary>
    /// Loads the event log JSON file and generates the heatmap.
    /// </summary>
    public void LoadPreviousHeatmap()
    {
        if (!File.Exists(logFilePath))
        {
            Debug.LogWarning("No event log file found at " + logFilePath);
            return;
        }

        string json = File.ReadAllText(logFilePath);
        SerializableEventLogList logList = JsonUtility.FromJson<SerializableEventLogList>(json);
        eventLogs = logList.eventLogs;
        Debug.Log($"Event logs loaded from {logFilePath}");

        // Clear current heatmap data
        heatmapData.Clear();

        // Populate the heatmapData using loaded logs
        foreach (var log in eventLogs)
        {
            Vector2Int gridPosition = WorldToGrid(new Vector2(log.position.x, log.position.z));
            if (!heatmapData.ContainsKey(gridPosition))
            {
                heatmapData[gridPosition] = 0;
            }
            heatmapData[gridPosition]++;
        }

        // Regenerate the heatmap
        GenerateHeatmap();
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