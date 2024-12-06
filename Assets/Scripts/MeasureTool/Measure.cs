using UnityEditor;
using UnityEngine;
using UnityEditor.EditorTools;

[EditorTool("Measure")]
public class Measure : EditorTool
{
    public override void OnToolGUI(EditorWindow window)
    {
        // Measure the selected object
        foreach (var obj in Selection.transforms)
        {
            if (obj == null) continue;

            // Display the position
            Handles.Label(obj.position, $"Position: {obj.position}", EditorStyles.boldLabel);

            // Calculate and display the distance to other selected objects
            foreach (var otherObj in Selection.transforms)
            {
                if (otherObj == obj || otherObj == null) continue;

                Vector3 startPosition = obj.position;
                Vector3 endPosition = otherObj.position;
                float distance = Vector3.Distance(startPosition, endPosition);

                Handles.DrawLine(startPosition, endPosition);
                Handles.Label((startPosition + endPosition) / 2, $"Distance: {distance:F2}", EditorStyles.boldLabel);
            }

            // Measure and display the mesh size
            if (obj.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                var bounds = meshFilter.sharedMesh.bounds;
                var size = Vector3.Scale(bounds.size, obj.lossyScale);
                Handles.Label(obj.position + Vector3.up * 2, $"Mesh Size: {size}", EditorStyles.boldLabel);
            }
        }
    }
}
