using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlacementTool : EditorWindow
{
    // Modes for object placement
    private enum PlacementMode { Line, Circle, Grid, Curve }
    private PlacementMode currentMode;
    private PlacementMode lastMode;

    // Prefab selection
    private GameObject prefabToPlace;
    private int objectCount = 10;

    // Placement settings
    private Vector3 startPoint = Vector3.zero;
    private Vector3 endPoint = Vector3.right * 10;
    private float radius = 5f;
    private int rows = 3, columns = 5;
    private float spacing = 2f;

    // Curve settings
    private List<Vector3> curvePoints = new List<Vector3> { Vector3.zero, Vector3.right * 10 };
    private int curveObjectCount = 10; // Number of objects along the curve

    // Preview settings
    private bool showPreview = true;
    private List<Vector3> previewPoints = new List<Vector3>();

    [MenuItem("Tools/Placement Tool")]
    public static void ShowWindow()
    {
        GetWindow<PlacementTool>("Placement Tool");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        lastMode = currentMode;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Placement Tool", EditorStyles.boldLabel);

        // Prefab selection
        prefabToPlace = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabToPlace, typeof(GameObject), false);

        if (prefabToPlace == null)
        {
            EditorGUILayout.HelpBox("Please assign a prefab to place.", MessageType.Warning);
            return;
        }

        // Mode selection
        currentMode = (PlacementMode)EditorGUILayout.EnumPopup("Placement Mode", currentMode);

        if (currentMode != lastMode)
        {
            lastMode = currentMode;
            previewPoints.Clear(); // Clear previous mode's preview
            GeneratePreview(); // Generate new preview for the current mode
        }

        // Placement controls based on the selected mode
        switch (currentMode)
        {
            case PlacementMode.Line:
                DrawLineControls();
                break;
            case PlacementMode.Circle:
                DrawCircleControls();
                break;
            case PlacementMode.Grid:
                DrawGridControls();
                break;
            case PlacementMode.Curve:
                DrawCurveControls();
                break;
        }

        // Preview toggle and action buttons
        showPreview = EditorGUILayout.Toggle("Show Preview", showPreview);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply Placement"))
        {
            ApplyPlacement();
        }

        if (GUILayout.Button("Reset Settings"))
        {
            ResetSettings();
        }
        GUILayout.EndHorizontal();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        // Always show handles for interactive editing
        if (currentMode == PlacementMode.Line || currentMode == PlacementMode.Grid)
        {
            Handles.color = Color.yellow;

            Vector3 newStart = Handles.PositionHandle(startPoint, Quaternion.identity);
            if (newStart != startPoint)
            {
                startPoint = newStart;
                GeneratePreview();
            }

            Vector3 newEnd = Handles.PositionHandle(endPoint, Quaternion.identity);
            if (newEnd != endPoint)
            {
                endPoint = newEnd;
                GeneratePreview();
            }
        }

        if (currentMode == PlacementMode.Circle)
        {
            Handles.color = Color.yellow;

            Vector3 newStart = Handles.PositionHandle(startPoint, Quaternion.identity);
            if (newStart != startPoint)
            {
                startPoint = newStart;
                GeneratePreview();
            }
        }

        if (currentMode == PlacementMode.Curve)
        {
            Handles.color = Color.green;

            for (int i = 0; i < curvePoints.Count; i++)
            {
                Vector3 newPoint = Handles.PositionHandle(curvePoints[i], Quaternion.identity);
                if (newPoint != curvePoints[i])
                {
                    curvePoints[i] = newPoint;
                    GeneratePreview();
                }
            }

            // Draw curve line
            for (int i = 0; i < curvePoints.Count - 1; i++)
            {
                Handles.DrawLine(curvePoints[i], curvePoints[i + 1]);
            }
        }

        // Redraw the preview if enabled
        if (showPreview)
        {
            DrawPreview();
        }

        // Force the Scene View to repaint
        SceneView.RepaintAll();
    }

    private void ApplyPlacement()
    {
        if (prefabToPlace == null || previewPoints.Count == 0)
        {
            Debug.LogWarning("No prefab assigned or no preview points available.");
            return;
        }

        // Create a parent object to group instantiated objects
        GameObject parent = new GameObject($"{prefabToPlace.name}_Placement");
        Undo.RegisterCreatedObjectUndo(parent, "Create Parent for Placement");

        foreach (Vector3 point in previewPoints)
        {
            // Instantiate the prefab at the specified position
            GameObject instance = PrefabUtility.InstantiatePrefab(prefabToPlace) as GameObject;

            if (instance != null)
            {
                instance.transform.position = point;
                instance.transform.rotation = Quaternion.identity;

                // Set the parent of the instance (optional)
                instance.transform.SetParent(parent.transform);

                // Register the created object with Undo
                Undo.RegisterCreatedObjectUndo(instance, $"Place {prefabToPlace.name}");
            }
        }

        // Clear the preview points after applying placement
        previewPoints.Clear();
        SceneView.RepaintAll();
    }

    private void DrawPreview()
    {
        if (previewPoints.Count == 0) return;

        Handles.color = Color.cyan;
        foreach (var point in previewPoints)
        {
            Handles.SphereHandleCap(0, point, Quaternion.identity, 0.5f, EventType.Repaint);
        }
    }

    private void ResetSettings()
    {
        // Reset general settings
        objectCount = 10;
        curveObjectCount = 10;
        showPreview = true;

        // Reset Line settings
        startPoint = Vector3.zero;
        endPoint = Vector3.right * 10;

        // Reset Circle settings
        radius = 5f;

        // Reset Grid settings
        rows = 3;
        columns = 5;
        spacing = 2f;

        // Reset Curve settings
        curvePoints = new List<Vector3> { Vector3.zero, Vector3.right * 10 };

        previewPoints.Clear();
        SceneView.RepaintAll();
    }

    private void GeneratePreview()
    {
        previewPoints.Clear();

        switch (currentMode)
        {
            case PlacementMode.Line:
                GenerateLinePreview();
                break;
            case PlacementMode.Circle:
                GenerateCirclePreview();
                break;
            case PlacementMode.Grid:
                GenerateGridPreview();
                break;
            case PlacementMode.Curve:
                GenerateCurvePreview();
                break;
        }
    }

    #region Line
    private void DrawLineControls()
    {
        GUILayout.Label("Line Placement Settings", EditorStyles.boldLabel);

        startPoint = EditorGUILayout.Vector3Field("Start Point", startPoint);
        endPoint = EditorGUILayout.Vector3Field("End Point", endPoint);
        objectCount = EditorGUILayout.IntSlider("Object Count", objectCount, 2, 100);

        GenerateLinePreview();
    }

    private void GenerateLinePreview()
    {
        previewPoints.Clear();
        for (int i = 0; i < objectCount; i++)
        {
            float t = i / (float)(objectCount - 1);
            previewPoints.Add(Vector3.Lerp(startPoint, endPoint, t));
        }
    }
    #endregion

    #region Circle
    private bool fillCircle = false;

    private void DrawCircleControls()
    {
        GUILayout.Label("Circle Placement Settings", EditorStyles.boldLabel);

        startPoint = EditorGUILayout.Vector3Field("Centre Point", startPoint);
        radius = EditorGUILayout.Slider("Radius", radius, 0.1f, 100);
        objectCount = EditorGUILayout.IntSlider("Object Count", objectCount, 1, 100);
        fillCircle = EditorGUILayout.Toggle("Fill Circle", fillCircle);

        GenerateCirclePreview();
    }

    private void GenerateCirclePreview()
    {
        previewPoints.Clear();

        if (fillCircle)
        {
            int rings = Mathf.CeilToInt(Mathf.Sqrt(objectCount));
            int remainingObjects = objectCount;

            for (int ring = 0; ring < rings; ring++)
            {
                float currentRadius = radius * (ring + 1) / rings;
                int pointsInRing = Mathf.Min(remainingObjects, Mathf.CeilToInt(2 * Mathf.PI * currentRadius / (radius / rings)));

                for (int i = 0; i < pointsInRing; i++)
                {
                    float angle = i * Mathf.PI * 2f / pointsInRing;
                    Vector3 position = startPoint + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * currentRadius;
                    previewPoints.Add(position);
                }

                remainingObjects -= pointsInRing;
                if (remainingObjects <= 0) break;
            }
        }
        else
        {
            for (int i = 0; i < objectCount; i++)
            {
                float angle = i * Mathf.PI * 2f / objectCount;
                Vector3 position = startPoint + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                previewPoints.Add(position);
            }
        }
    }
    #endregion

    #region Grid
    private void DrawGridControls()
    {
        GUILayout.Label("Grid Placement Settings", EditorStyles.boldLabel);

        startPoint = EditorGUILayout.Vector3Field("Start Point", startPoint);
        rows = EditorGUILayout.IntSlider("Rows", rows, 1, 100);
        columns = EditorGUILayout.IntSlider("Columns", columns, 1, 100);
        spacing = EditorGUILayout.Slider("Spacing", spacing, 0.1f, 10f);

        GenerateGridPreview();
    }

    private void GenerateGridPreview()
    {
        previewPoints.Clear();
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 position = startPoint + new Vector3(col * spacing, 0, row * spacing);
                previewPoints.Add(position);
            }
        }
    }
    #endregion

    #region Curve
    private void DrawCurveControls()
    {
        GUILayout.Label("Curve Placement Settings", EditorStyles.boldLabel);
        
        curveObjectCount = EditorGUILayout.IntSlider("Object Count", curveObjectCount, 2, 100);
        
        if (GUILayout.Button("Add Point"))
        {
            curvePoints.Add(curvePoints[curvePoints.Count - 1] + Vector3.right);
        }

        GenerateCurvePreview();
    }

    private void GenerateCurvePreview()
    {
        previewPoints.Clear();

        if (curvePoints.Count < 2) return;

        for (int i = 0; i < curveObjectCount; i++)
        {
            float t = i / (float)(curveObjectCount - 1);
            Vector3 position = BezierCurve(curvePoints, t);
            previewPoints.Add(position);
        }
    }

    private Vector3 BezierCurve(List<Vector3> points, float t)
    {
        while (points.Count > 1)
        {
            List<Vector3> nextPoints = new List<Vector3>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                nextPoints.Add(Vector3.Lerp(points[i], points[i + 1], t));
            }
            points = nextPoints;
        }
        return points[0];
    }
    #endregion
}
