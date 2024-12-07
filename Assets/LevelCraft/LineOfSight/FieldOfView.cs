using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfView : MonoBehaviour
{
    public float detectionRadius = 10f;
    [Range(1, 360)]
    public float detectionAngle = 90f;

    public LayerMask obstacleLayer;

    public float meshDetailLevel = 1;
    public int edgeResolutionSteps = 4;
    public float edgeDistanceThreshold = 0.5f;

    public MeshFilter FOVConeMeshFilter;
    private Mesh FOVConeMesh;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        FOVConeMesh = new Mesh { name = "Field Of View" };
        FOVConeMeshFilter.mesh = FOVConeMesh;
    }

    private void LateUpdate()
    {
        RenderDetectionCone();
    }

    private void RenderDetectionCone()
    {
        int stepCount = Mathf.RoundToInt(detectionAngle * meshDetailLevel);
        float stepAngleSize = detectionAngle / stepCount;
        List<Vector3> conePoints = new List<Vector3>();
        RayInfo previousRay = new RayInfo();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - detectionAngle / 2 + stepAngleSize * i;
            RayInfo currentRay = CastRay(angle);

            if (i > 0)
            {
                bool significantEdge = Mathf.Abs(previousRay.distance - currentRay.distance) > edgeDistanceThreshold;

                if (previousRay.hit != currentRay.hit || (previousRay.hit && currentRay.hit && significantEdge))
                {
                    BoundaryInfo boundary = DetermineBoundary(previousRay, currentRay);

                    if (boundary.pointA != Vector3.zero) conePoints.Add(boundary.pointA);
                    if (boundary.pointB != Vector3.zero) conePoints.Add(boundary.pointB);
                }
            }

            conePoints.Add(currentRay.point);
            previousRay = currentRay;
        }

        BuildConeMesh(conePoints);
    }

    private void BuildConeMesh(List<Vector3> points)
    {
        int vertexCount = points.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i < points.Count; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(points[i]);

            if (i < points.Count - 1)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        FOVConeMesh.Clear();
        FOVConeMesh.vertices = vertices;
        FOVConeMesh.triangles = triangles;
        FOVConeMesh.RecalculateNormals();
    }

    private BoundaryInfo DetermineBoundary(RayInfo minRay, RayInfo maxRay)
    {
        float minAngle = minRay.angle;
        float maxAngle = maxRay.angle;
        Vector3 pointA = Vector3.zero, pointB = Vector3.zero;

        for (int i = 0; i < edgeResolutionSteps; i++)
        {
            float middleAngle = (minAngle + maxAngle) / 2;
            RayInfo midRay = CastRay(middleAngle);

            if (midRay.hit == minRay.hit && Mathf.Abs(minRay.distance - midRay.distance) <= edgeDistanceThreshold)
            {
                minAngle = middleAngle;
                pointA = midRay.point;
            }
            else
            {
                maxAngle = middleAngle;
                pointB = midRay.point;
            }
        }

        return new BoundaryInfo(pointA, pointB);
    }

    private RayInfo CastRay(float globalAngle)
    {
        Vector3 direction = AngleToDirection(globalAngle);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, direction, out hit, detectionRadius, obstacleLayer))
        {
            return new RayInfo(true, hit.point, hit.distance, globalAngle);
        }

        return new RayInfo(false, transform.position + direction * detectionRadius, detectionRadius, globalAngle);
        
    }

    private Vector3 AngleToDirection(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians));
    }

    public struct RayInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public RayInfo(bool hit, Vector3 point, float distance, float angle)
        {
            this.hit = hit;
            this.point = point;
            this.distance = distance;
            this.angle = angle;
        }
    }

    public struct BoundaryInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public BoundaryInfo(Vector3 pointA, Vector3 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }
}
