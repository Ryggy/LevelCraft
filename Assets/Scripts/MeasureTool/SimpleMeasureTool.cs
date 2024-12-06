using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorTool("Simple Measure Tool")]
public class SimpleMeasureTool : EditorTool
{
    // Settings keys
    private static readonly string PrefId = "SimpleMeasureTool_";
    private static bool showSettings;
    private static bool showPositionLabels = true;
    private static bool showDistanceLabels = true;
    private static bool showMeshLabels = true;
    private static bool settingsFoldout = true;
    private static Color textColour = Color.white;
    private static Color lineColour = Color.yellow;
    private static int textSize = 12;
    private static float lineWidth = 2f;
    private static float outlineWidth = 1;

    private static Vector2 settingsWindowPosition = new Vector2(10, 10);
    private static Vector2 dragOffset;
    private static bool isDragging = false;

    private bool isInitialised = false;

    private void OnEnable()
    {
        if (!isInitialised)
        {
            LoadPreferences();
            isInitialised = true;
        }
    }

    public override void OnToolGUI(EditorWindow window)
    {
        Handles.color = lineColour;

        // Draw settings UI if toggled
        if (showSettings)
        {
            DrawSettingsUI();
        }

        // Measure selected objects
        foreach (var obj in Selection.transforms)
        {
            if (obj == null) continue;

            // Display position labels
            if (showPositionLabels)
            {
                DrawOutlinedLabel(obj.position, $"Position: {obj.position}", outlineWidth);
            }

            // Display distance labels
            foreach (var otherObj in Selection.transforms)
            {
                if (otherObj == obj || otherObj == null) continue;

                Vector3 startPosition = obj.position;
                Vector3 endPosition = otherObj.position;
                float distance = Vector3.Distance(startPosition, endPosition);

                Handles.DrawAAPolyLine(lineWidth, startPosition, endPosition);

                if (showDistanceLabels)
                {
                    DrawOutlinedLabel((startPosition + endPosition) / 2, $"Distance: {distance:F2}", outlineWidth);
                }
            }

            // Display mesh size labels
            if (showMeshLabels && obj.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                var bounds = meshFilter.sharedMesh.bounds;
                var size = Vector3.Scale(bounds.size, obj.lossyScale);
                DrawOutlinedLabel(obj.position + Vector3.up * 2, $"Mesh Size: {size}", outlineWidth);
            }
        }
    }

    private void DrawSettingsUI()
    {
        Handles.BeginGUI();

        // Create draggable settings window
        Rect windowRect = new Rect(settingsWindowPosition, new Vector2(160, settingsFoldout ? 300 : 30));
        GUI.Box(windowRect, GUIContent.none, GUI.skin.box);

        // Handle dragging the window
        Rect dragRect = new Rect(settingsWindowPosition, new Vector2(160, 20));
        settingsFoldout = EditorGUI.Foldout(dragRect, settingsFoldout, "Measure Tool Settings", true);
        EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.Pan);

        Event e = Event.current;

        // Handle dragging only in the header area
        if (dragRect.Contains(e.mousePosition))
        {
            if (e.type == EventType.MouseDown)
            {
                isDragging = true;
                dragOffset = e.mousePosition - settingsWindowPosition;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && isDragging)
            {
                settingsWindowPosition = e.mousePosition - dragOffset;
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                isDragging = false;
                e.Use();
            }
        }

        if (settingsFoldout)
        {
            // Define a smaller area for the foldout content, excluding the header
            Rect contentRect = new Rect(settingsWindowPosition.x, settingsWindowPosition.y + 20, 160, 280);
            GUILayout.BeginArea(contentRect);
            GUILayout.Space(5);

            // Text colour
            GUILayout.Label("Text Colour:");
            textColour = EditorGUILayout.ColorField(textColour);

            // Line colour
            GUILayout.Label("Line Colour:");
            lineColour = EditorGUILayout.ColorField(lineColour);

            // Text size
            GUILayout.Label("Text Size:");
            textSize = EditorGUILayout.IntSlider(textSize, 10, 40);

            // Line width
            GUILayout.Label("Line Width:");
            lineWidth = EditorGUILayout.Slider(lineWidth, 1f, 10f);

            // Outline width
            GUILayout.Label("Outline Width:");
            outlineWidth = EditorGUILayout.Slider(outlineWidth, 0f, 2f);

            // Toggles for label visibility
            showPositionLabels = GUILayout.Toggle(showPositionLabels, "Position");
            showDistanceLabels = GUILayout.Toggle(showDistanceLabels, "Distance");
            showMeshLabels = GUILayout.Toggle(showMeshLabels, "Mesh Size");

            if (GUILayout.Button("Save Settings"))
            {
                SavePreferences();
            }

            GUILayout.EndArea();
        }

        Handles.EndGUI();
    }


    private void DrawOutlinedLabel(Vector3 position, string text, float outlineWidth)
    {
        GUIStyle style = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = textColour },
            fontSize = textSize
        };

        if (outlineWidth > 0 && HandleUtility.WorldToGUIPointWithDepth(position).z > 0)
        {
            var backgroundStyle = new GUIStyle(style) { normal = { textColor = Color.black } };
            Rect rect = HandleUtility.WorldPointToSizedRect(position, new GUIContent(text), style);
            Handles.BeginGUI();

            // Vertical lines (left and right)
            for (float i = -outlineWidth; i <= outlineWidth; i++)
            {
                GUI.Label(new Rect(rect.x - outlineWidth, rect.y + i, rect.width, rect.height), text, backgroundStyle);
                GUI.Label(new Rect(rect.x + outlineWidth, rect.y + i, rect.width, rect.height), text, backgroundStyle);
            }

            // Horizontal lines (top and bottom)
            for (float i = -outlineWidth + 1; i <= outlineWidth - 1; i++)
            {
                GUI.Label(new Rect(rect.x + i, rect.y - outlineWidth, rect.width, rect.height), text, backgroundStyle);
                GUI.Label(new Rect(rect.x + i, rect.y + outlineWidth, rect.width, rect.height), text, backgroundStyle);
            }

            Handles.EndGUI();
        }

        Handles.Label(position, text, style);
    }

    [MenuItem("Tools/Simple Measure Tool/Toggle Settings")]
    private static void ToggleSettings()
    {
        showSettings = !showSettings;
        EditorPrefs.SetBool(PrefId + nameof(showSettings), showSettings);
        SceneView.RepaintAll();
    }

    private static void SavePreferences()
    {
        EditorPrefs.SetBool(PrefId + nameof(showSettings), showSettings);
        EditorPrefs.SetBool(PrefId + nameof(showPositionLabels), showPositionLabels);
        EditorPrefs.SetBool(PrefId + nameof(showDistanceLabels), showDistanceLabels);
        EditorPrefs.SetBool(PrefId + nameof(showMeshLabels), showMeshLabels);
        EditorPrefs.SetBool(PrefId + nameof(settingsFoldout), settingsFoldout);
        EditorPrefs.SetString(PrefId + nameof(textColour), JsonUtility.ToJson(textColour));
        EditorPrefs.SetString(PrefId + nameof(lineColour), JsonUtility.ToJson(lineColour));
        EditorPrefs.SetInt(PrefId + nameof(textSize), textSize);
        EditorPrefs.SetFloat(PrefId + nameof(lineWidth), lineWidth);
        EditorPrefs.SetFloat(PrefId + nameof(outlineWidth), outlineWidth);
        EditorPrefs.SetFloat(PrefId + nameof(settingsWindowPosition) + "x", settingsWindowPosition.x);
        EditorPrefs.SetFloat(PrefId + nameof(settingsWindowPosition) + "y", settingsWindowPosition.y);
    }

    private static void LoadPreferences()
    {
        showSettings = EditorPrefs.GetBool(PrefId + nameof(showSettings), false);
        showPositionLabels = EditorPrefs.GetBool(PrefId + nameof(showPositionLabels), true);
        showDistanceLabels = EditorPrefs.GetBool(PrefId + nameof(showDistanceLabels), true);
        showMeshLabels = EditorPrefs.GetBool(PrefId + nameof(showMeshLabels), true);
        settingsFoldout = EditorPrefs.GetBool(PrefId + nameof(settingsFoldout), true);
        textColour = JsonUtility.FromJson<Color>(EditorPrefs.GetString(PrefId + nameof(textColour), JsonUtility.ToJson(Color.white)));
        lineColour = JsonUtility.FromJson<Color>(EditorPrefs.GetString(PrefId + nameof(lineColour), JsonUtility.ToJson(Color.yellow)));
        textSize = EditorPrefs.GetInt(PrefId + nameof(textSize), 12);
        lineWidth = EditorPrefs.GetFloat(PrefId + nameof(lineWidth), 2f);
        outlineWidth = EditorPrefs.GetInt(PrefId + nameof(outlineWidth), 1);
        settingsWindowPosition.x = EditorPrefs.GetFloat(PrefId + nameof(settingsWindowPosition) + "x", 10);
        settingsWindowPosition.y = EditorPrefs.GetFloat(PrefId + nameof(settingsWindowPosition) + "y", 10);
    }
}