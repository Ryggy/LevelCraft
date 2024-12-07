using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class HeatmapTracker
{
    public enum HeatmapTrackerType { Discrete, Continuous }
    public HeatmapTrackerType Type;
    
    public enum TriggerType { Input, Event }
    public TriggerType TriggerMode;
    
    public string EventName;
    public GameObject TargetObject;
    
    // Input settings
    public KeyCode TriggerKey = KeyCode.E;

    // Event settings
    public UnityEvent OnTriggerEvent;
    
    public void DrawTrackerUI()
    {
        // Tracker Type
        Type = (HeatmapTrackerType)EditorGUILayout.EnumPopup("Tracker Type", Type);

        // Trigger Mode is only shown for Discrete trackers
        if (Type == HeatmapTrackerType.Discrete)
        {
            TriggerMode = (TriggerType)EditorGUILayout.EnumPopup("Trigger Mode", TriggerMode);

            // Display settings based on Trigger Mode
            if (TriggerMode == TriggerType.Input)
            {
                TriggerKey = (KeyCode)EditorGUILayout.EnumPopup("Trigger Key", TriggerKey);
            }
            else if (TriggerMode == TriggerType.Event)
            {
                EditorGUILayout.HelpBox("Configure UnityEvent in HeatmapManager.", MessageType.Info);
            }
        }

        // General Settings
        EventName = EditorGUILayout.TextField("Event Name", EventName);
        TargetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", TargetObject, typeof(GameObject), true);
    }

    public bool CheckTrigger()
    {
        if (Type == HeatmapTrackerType.Continuous)
            return true;

        switch (TriggerMode)
        {
            case TriggerType.Input:
                return Input.GetKeyDown(TriggerKey);
            case TriggerType.Event:
                return false; // Events are handled externally
        }
        return false;
    }

    public void InvokeEvent()
    {
        if (TriggerMode == TriggerType.Event && OnTriggerEvent != null)
        {
            OnTriggerEvent.Invoke();
        }
    }
}

