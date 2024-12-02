using UnityEngine;

public class Sensor : MonoBehaviour, ISensor
{
   public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void UpdateSensor()
    {
        
    }
}
