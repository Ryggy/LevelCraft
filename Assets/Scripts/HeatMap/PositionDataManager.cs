using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionDataManager
{
    private List<Vector3> cumulativeData = new List<Vector3>();
    private List<List<Vector3>> snapshots = new List<List<Vector3>>();

    /// <summary>
    /// Adds a new set of positions to the cumulative data.
    /// </summary>
    public void AddPositions(List<Vector3> positions)
    {
        cumulativeData.AddRange(positions);
    }

    /// <summary>
    /// Takes a snapshot of the current position data.
    /// </summary>
    public void TakeSnapshot(List<Vector3> positions)
    {
        snapshots.Add(new List<Vector3>(positions)); // Store a copy
    }

    /// <summary>
    /// Gets the cumulative data.
    /// </summary>
    public List<Vector3> GetCumulativeData()
    {
        return new List<Vector3>(cumulativeData); // Return a copy
    }

    /// <summary>
    /// Gets a specific snapshot by index.
    /// </summary>
    public List<Vector3> GetSnapshot(int index)
    {
        if (index >= 0 && index < snapshots.Count)
        {
            return new List<Vector3>(snapshots[index]); // Return a copy
        }
        Debug.LogWarning("Snapshot index out of range.");
        return null;
    }

    /// <summary>
    /// Gets all snapshots.
    /// </summary>
    public List<List<Vector3>> GetAllSnapshots()
    {
        return new List<List<Vector3>>(snapshots); // Return a copy
    }

    /// <summary>
    /// Clears all data (cumulative and snapshots).
    /// </summary>
    public void ClearData()
    {
        cumulativeData.Clear();
        snapshots.Clear();
    }
}
