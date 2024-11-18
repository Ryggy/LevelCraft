using System;
using UnityEngine;

namespace HeatMap
{
    public class HeatMapManager : MonoBehaviour
    {
        // Grid dimensions and cell size
        private int width;
        private int height;
        private float cellSize;
        
        // 2D array to store grid nodes
        private GridNode[,] grid;
        // Reference to the heatmap renderer
        private HeatMapRenderer renderer;
        private void Start()
        {
            renderer = GetComponent<HeatMapRenderer>();
            
            InitialiseGrid(256, 256, 1f);
            renderer.InitialiseRenderTexture(width, height);
            AddEvent(new Vector3(0,0,0),"test", 1f);
            AddEvent(new Vector3(1,0,1),"test", 1f);
            AddEvent(new Vector3(1,0,1),"test", 1f);
            AddEvent(new Vector3(2,0,2),"test", 1f);
            
            UpdateVisualisation();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                // Convert mouse position to world position
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Vector3 worldPosition = hit.point;

                    // Add an event at the hit position
                    AddEvent(worldPosition, "test", 1f);
                }
            }
        }

        /// <summary>
        /// Initialise the grid system with specified dimensions and cell size
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="cellSize"></param>
        public void InitialiseGrid(int width, int height, float cellSize)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;

            grid = new GridNode[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 worldPosition = new Vector3(x * cellSize, 0, y * cellSize);
                    grid[x, y] = new GridNode(worldPosition);
                }
            }
        }

        /// <summary>
        /// Add an event to the heat map at the specified position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="eventType"></param>
        /// <param name="weight"></param>
        public void AddEvent(Vector3 position, string eventType, float weight)
        {
            GridNode node = FindNode(position);
            if (node != null)
            {
                node.AddEvent(eventType, weight);
            }
        }

        /// <summary>
        /// Find the grid node corresponding to a given position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private GridNode FindNode(Vector3 position)
        {
            int x = Mathf.FloorToInt(position.x / cellSize);
            int y = Mathf.FloorToInt(position.z / cellSize);

            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                return grid[x, y];
            }

            return null;
        }

        /// <summary>
        ///  Update the visualisation of the heat map
        /// </summary>
        private void UpdateVisualisation()
        {
            if (renderer != null)
            {
                renderer.RenderHeatMap(grid, null); // Replace `null` with the actual gradient settings
            }
        }
    }
}