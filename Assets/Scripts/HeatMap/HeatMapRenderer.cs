using UnityEngine;
using System.Collections.Generic;

namespace HeatMap
{
    public class HeatMapRenderer : MonoBehaviour
    {
        public int textureWidth = 256; // Width of the heat map texture
        public int textureHeight = 256; // Height of the heat map texture
        public Gradient defaultColourGradient; // Gradient for mapping intensity to color
        public Renderer targetRenderer; // Renderer to display the heat map
        public float transparency = 0.8f; // Transparency of the heat map (0 to 1)

        private CustomRenderTexture heatMapTexture;
        
        // Initialise the heat map texture
        public void InitialiseRenderTexture(int gridWidth, int gridHeight)
        {
            textureWidth = gridWidth;
            textureHeight = gridHeight;

            // Create a CustomRenderTexture
            heatMapTexture = new CustomRenderTexture(textureWidth, textureHeight, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Point, // Prevent blurring
                wrapMode = TextureWrapMode.Clamp,
                autoGenerateMips = false // No mipmaps needed
            };

            heatMapTexture.Create();

            // Assign the render texture to the material of the target renderer
            if (targetRenderer != null)
            {
                targetRenderer.material.mainTexture = heatMapTexture;
            }

            // Clear the texture initially
            ClearRenderTexture();
        }

        
        /// <summary>
        /// Renders the heat map based on the grid nodes and the provided colour gradient
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="gradient"></param>
        public void RenderHeatMap(GridNode[,] grid, Gradient gradient)
        {
            // Use the provided gradient or fallback to the default
            Gradient gradientToUse = gradient ?? defaultColourGradient;

            // Create a temporary Texture2D to update the render texture
            Texture2D tempTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            tempTexture.filterMode = FilterMode.Point;

            // Loop through the grid and update the temporary texture
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    GridNode node = grid[x, y];
                    float intensity = CalculateIntensity(node);

                    // Map intensity to a color using the gradient
                    Color color = gradientToUse.Evaluate(intensity);
                    color.a = transparency; // Apply transparency

                    if (intensity == 0)
                    {
                        color.a = 0;
                    }
                    
                    // Set the pixel color on the temporary texture
                    tempTexture.SetPixel(x, y, color);
                }
            }

            // Apply the changes to the temporary texture
            tempTexture.Apply();

            // Copy the temporary texture to the CustomRenderTexture
            Graphics.Blit(tempTexture, heatMapTexture);

            // Clean up the temporary texture
            Destroy(tempTexture);
        }
        
        // Clears the heat map render texture by setting all pixels to transparent
        public void ClearRenderTexture()
        {
            // Use a command buffer to clear the CustomRenderTexture
            Graphics.SetRenderTarget(heatMapTexture);
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.SetRenderTarget(null);
        }
        
        /// <summary>
        /// Calculates the intensity of events for a grid node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private float CalculateIntensity(GridNode node)
        {
            float totalEvents = 0;

            // Sum all event weights in the node
            foreach (var weight in node.EventData.Values)
            {
                totalEvents += weight;
            }

            // Normalize the total weight
            float maxEvents = 10f; // Replace with a dynamic max value if needed
            return Mathf.Clamp01(totalEvents / maxEvents);
        }
    }
}
