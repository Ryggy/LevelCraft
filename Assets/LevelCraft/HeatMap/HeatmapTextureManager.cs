using UnityEngine;

public class HeatmapTextureManager : MonoBehaviour
{
    public enum HeatmapMode
    {
        Histogram,
        KDE
    }

    [Header("Heatmap Settings")]
    public Gradient colourGradient; // Gradient for coloring the heatmap
    public Vector2Int textureResolution = new Vector2Int(64, 64); // Resolution of the heatmap texture
    public HeatmapMode mode = HeatmapMode.Histogram; // Toggle between histogram and KDE

    [Header("Display Settings")]
    public MeshRenderer heatmapRenderer; // Renderer to display the heatmap texture
    private Texture2D heatmapTexture;

    [Header("Kernel properties")]
    public float bandwidth = 3f; 
    public float kernelMaxDistance = 3f;

    private void OnEnable()
    {
        InitialiseTexture();
    }

    private void OnValidate()
    {
        // Reinitialize texture if settings change in the inspector
        InitialiseTexture();
    }
    
    private void InitialiseTexture()
    {
        if (heatmapTexture == null || heatmapTexture.width != textureResolution.x || heatmapTexture.height != textureResolution.y)
        {
            // Create or recreate the texture
            heatmapTexture = new Texture2D(textureResolution.x, textureResolution.y, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            Debug.Log($"Heatmap texture initialized with resolution: {textureResolution.x}x{textureResolution.y}");
        }

        // Assign the texture to the renderer's material
        if (heatmapRenderer != null)
        {
            //heatmapRenderer.material.mainTexture = heatmapTexture;
            heatmapRenderer.sharedMaterial.mainTexture = heatmapTexture;
        }
        else
        {
            Debug.LogWarning("Heatmap Renderer is not assigned.");
        }
    }
    
    /// <summary>
    /// Updates the heatmap texture based on the provided data grid.
    /// </summary>
    /// <param name="dataGrid">2D array of intensity values.</param>
    public void UpdateTexture(float[,] dataGrid)
    {
        if (mode == HeatmapMode.Histogram)
        {
            UpdateHistogramTexture(dataGrid);
        }
        else if (mode == HeatmapMode.KDE)
        {
            UpdateKDETexture(dataGrid);
        }
    }

    /// <summary>
    /// Updates the texture using a histogram approach.
    /// </summary>
    private void UpdateHistogramTexture(float[,] dataGrid)
    {
        int width = dataGrid.GetLength(0);
        int height = dataGrid.GetLength(1);

        // Find the maximum value in the grid for normalization
        float maxValue = 0f;
        foreach (var value in dataGrid)
        {
            if (value > maxValue)
                maxValue = value;
        }

        // Update the texture pixels
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float normalizedValue = maxValue > 0 ? dataGrid[x, y] / maxValue : 0f;
                Color color = colourGradient.Evaluate(normalizedValue);
                heatmapTexture.SetPixel(x, y, color);
            }
        }

        heatmapTexture.Apply();
    }

    /// <summary>
    /// Updates the texture using a Kernel Density Estimation approach.
    /// </summary>
    private void UpdateKDETexture(float[,] dataGrid)
    {
        int width = dataGrid.GetLength(0);
        int height = dataGrid.GetLength(1);

        // Create a temporary KDE grid
        float[,] kdeGrid = new float[width, height];

        // Apply kernel to every grid point
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Skip empty cells
                if (dataGrid[x, y] <= 0f) continue;

                // Apply the kernel
                for (int dx = -Mathf.CeilToInt(kernelMaxDistance); dx <= Mathf.CeilToInt(kernelMaxDistance); dx++)
                {
                    for (int dy = -Mathf.CeilToInt(kernelMaxDistance); dy <= Mathf.CeilToInt(kernelMaxDistance); dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        // Ensure we stay within grid bounds
                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            float distance = Mathf.Sqrt(dx * dx + dy * dy);
                            if (distance <= kernelMaxDistance)
                            {
                                float weight = Mathf.Exp(-0.5f * (distance * distance) / (bandwidth * bandwidth));
                                kdeGrid[nx, ny] += dataGrid[x, y] * weight;
                            }
                        }
                    }
                }
            }
        }

        // Find the maximum value in the KDE grid for normalisation
        float maxValue = 0f;
        foreach (var value in kdeGrid)
        {
            if (value > maxValue)
                maxValue = value;
        }

        // Update the texture pixels
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float normalizedValue = maxValue > 0 ? kdeGrid[x, y] / maxValue : 0f;
                Color color = colourGradient.Evaluate(normalizedValue);
                heatmapTexture.SetPixel(x, y, color);
            }
        }

        heatmapTexture.Apply();
    }

    /// <summary>
    /// Clears the texture by setting all pixels to transparent.
    /// </summary>
    public void ClearTexture()
    {
        int width = heatmapTexture.width;
        int height = heatmapTexture.height;

        Color clearColor = new Color(0, 0, 0, 0); // Transparent
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heatmapTexture.SetPixel(x, y, clearColor);
            }
        }

        heatmapTexture.Apply();
    }
}
