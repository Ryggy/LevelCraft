using UnityEngine;
using System.Collections.Generic;

namespace HeatMap
{
    public class GridNode
    {
        public Vector3 Position { get; private set; }
        public Dictionary<string, float> EventData;

        public GridNode(Vector3 position)
        {
            Position = position;
            EventData = new Dictionary<string, float>();
        }

        public void AddEvent(string eventType, float weight)
        {
            if (!EventData.ContainsKey(eventType))
            {
                EventData[eventType] = 0;
            }

            EventData[eventType] += weight;
        }

        public float GetEventFrequency(string eventType)
        {
            return EventData.ContainsKey(eventType) ? EventData[eventType] : 0;
        }
    }
}
