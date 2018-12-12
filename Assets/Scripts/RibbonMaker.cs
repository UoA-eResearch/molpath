using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RibbonMaker : MonoBehaviour
{
    //Use the transforms of GameObjects in 3d space as your points or define array with desired points
    public List<Transform> controlPoints = new List<Transform>();

    //Store points on the Catmull curve so we can visualize them
    List<Vector3> newPoints = new List<Vector3>();

    //How many points you want on the curve
    float amountOfPoints = 10.0f;

    //set from 0-1
    public float alpha = 0.5f;

    /////////////////////////////

    public LineRenderer line;
    private List<Vector3> debugPositions = new List<Vector3>();

    // the number of points in between control points
    [Tooltip("Number of interpolation points between control points")]
    public int numberOfPoints = 5;

    private MeshRenderer _meshRenderer;
    public Material Material;
    public int crossSegments = 5;
    public Vector3[] crossPoints;
    public int lastCrossSegments;
    private Vector3[] interpolatedPositions;

    public class TubeVertex
    {
        public Vector3 point = Vector3.zero;
        public float radius = 4f;

        public TubeVertex(Vector3 pt, float r)
        {
            point = pt;
            radius = r;
        }
    }

    private TubeVertex[] tubeVertices;
    private void Start()
    {
		line.positionCount = controlPoints.Count * numberOfPoints;
		_meshRenderer = gameObject.GetComponent<MeshRenderer>();
        _meshRenderer.material = new Material(Material.shader);
    }

    void Update()
    {
    }

	private static int SortByName(GameObject o1, GameObject o2)
	{
		return o1.name.CompareTo(o2.name);
	}

    private void FixedUpdate()
    {
		controlPoints.Clear();
		foreach (var amide in GameObject.FindGameObjectsWithTag("amide"))
        {
            controlPoints.Add(amide.transform);
        }
        foreach (var calpha in GameObject.FindGameObjectsWithTag("calpha"))
        {
            controlPoints.Add(calpha.transform);
        }
        foreach (var carbonyl in GameObject.FindGameObjectsWithTag("carbonyl"))
        {
            controlPoints.Add(carbonyl.transform);
        }

        // TODO: Need to sort by name here for an array of the game objects rather. Not possible at the moment as theyre transforms.


		interpolatedPositions = new Vector3[numberOfPoints * controlPoints.Count];
        tubeVertices = new TubeVertex[numberOfPoints * controlPoints.Count];
        DynamicSpline(controlPoints);
        CreateTubeVertices();
        Show();
    }
    void DynamicSpline(List<Transform> controlPoints)
    {
        Vector3 p0, p1, m0, m1;

        for (int j = 0; j < controlPoints.Count - 1; j++)
        {
            // check control points
            if (controlPoints[j] == null || controlPoints[j + 1] == null ||
                (j > 0 && controlPoints[j - 1] == null) ||
                (j < controlPoints.Count - 2 && controlPoints[j + 2] == null))
            {
                return;
            }
            // determine control points of segment
            p0 = controlPoints[j].transform.position;
            p1 = controlPoints[j + 1].transform.position;

            if (j > 0)
            {
                m0 = 0.5f * (controlPoints[j + 1].transform.position - controlPoints[j - 1].transform.position);
            }
            else
            {
                m0 = controlPoints[j + 1].transform.position - controlPoints[j].transform.position;
            }
            if (j < controlPoints.Count - 2)
            {
                m1 = 0.5f * (controlPoints[j + 2].transform.position - controlPoints[j].transform.position);
            }
            else
            {
                m1 = controlPoints[j + 1].transform.position - controlPoints[j].transform.position;
            }

            // set points of Hermite curve
            Vector3 position;
            float t;
            float pointStep = 1.0f / numberOfPoints;

            if (j == controlPoints.Count - 2)
            {
                pointStep = 1.0f / (numberOfPoints - 1.0f);
                // last point of last segment should reach p1
            }
            for (int i = 0; i < numberOfPoints; i++)
            {
                t = i * pointStep;
                position = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * p0
                    + (t * t * t - 2.0f * t * t + t) * m0
                    + (-2.0f * t * t * t + 3.0f * t * t) * p1
                    + (t * t * t - t * t) * m1;
                // line.SetPosition(i + j * numberOfPoints,
                //     position);
                interpolatedPositions[i + j * numberOfPoints] = position;
            }
        }
    }


    private void OnDrawGizmos()
    {
        Debug.Log(tubeVertices.Length);
        if (tubeVertices.Length > 0)
        {
            // foreach (var vertex in tubeVertices)
            // {
            //     // Debug.Log(vertex.point);
            //     Gizmos.color = Color.red;
            //     Gizmos.DrawSphere(vertex.point, 0.1f);
            // }
            for (int i = 0; i < interpolatedPositions.Length - 1; i++)
            {
				var vertex1 = interpolatedPositions[i];
				var vertex2 = interpolatedPositions[i+1];
				// Debug.Log(vertex.point);
				Gizmos.color = Color.red;
				// Gizmos.DrawSphere(vertex, 0.1f);
				Gizmos.DrawLine(vertex1, vertex2);
			}
        }
    }

    private int CreateTubeVertices()
    {
        for (int i = 0; i < interpolatedPositions.Length; i++)
        {
            // Debug.Log(interpolatedPositions[i]);
            tubeVertices[i] = new TubeVertex(interpolatedPositions[i], 1.0f);
        }
        return interpolatedPositions.Length;
    }

    /// <summary>
    /// iterates through tube vertices and does the vertices calcuations for them.
    /// </summary>
    private void Show()
    {
        if (null == tubeVertices ||
            tubeVertices.Length <= 1)
        {
            GetComponent<Renderer>().enabled = false;
            return;
        }
        GetComponent<Renderer>().enabled = true;

        //draw tube
        if (crossSegments != lastCrossSegments)
        {
            crossPoints = new Vector3[crossSegments];
            float theta = 2.0f * Mathf.PI / crossSegments;
            for (int c = 0; c < crossSegments; c++)
            {
                crossPoints[c] = new Vector3(Mathf.Cos(theta * c) / 10.0f, Mathf.Sin(theta * c), 0);
            }
            lastCrossSegments = crossSegments;
        }

        Vector3[] meshVertices = new Vector3[tubeVertices.Length * crossSegments];
        Vector2[] uvs = new Vector2[tubeVertices.Length * crossSegments];
        Color[] colors = new Color[tubeVertices.Length * crossSegments];
        int[] tris = new int[tubeVertices.Length * crossSegments * 6];
        int[] lastVertices = new int[crossSegments];
        int[] theseVertices = new int[crossSegments];
        Quaternion rotation = Quaternion.identity;
        for (int p = 0; p < tubeVertices.Length; p++)
        {
            if (p < tubeVertices.Length - 1)
                rotation = Quaternion.FromToRotation(Vector3.forward, tubeVertices[p + 1].point - tubeVertices[p].point);

            for (int c = 0; c < crossSegments; c++)
            {
                int vertexIndex = p * crossSegments + c;
                meshVertices[vertexIndex] = tubeVertices[p].point + rotation * crossPoints[c] * tubeVertices[p].radius;
                uvs[vertexIndex] = new Vector2((0.0f + c) / crossSegments, (0.0f + p) / tubeVertices.Length);
                colors[vertexIndex] = Color.yellow;
                // colors[vertexIndex] = tubeVertices[p].color;

                lastVertices[c] = theseVertices[c];
                theseVertices[c] = p * crossSegments + c;
            }
            //make triangles
            if (p > 0)
            {
                for (int c = 0; c < crossSegments; c++)
                {
                    int start = (p * crossSegments + c) * 6;
                    tris[start] = lastVertices[c];
                    tris[start + 1] = lastVertices[(c + 1) % crossSegments];
                    tris[start + 2] = theseVertices[c];
                    tris[start + 3] = tris[start + 2];
                    tris[start + 4] = tris[start + 1];
                    tris[start + 5] = theseVertices[(c + 1) % crossSegments];

                }
            }
        }

        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        if (!mesh)
        {
            mesh = new Mesh();
        }
        mesh.vertices = meshVertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.uv = uvs;
        mesh.colors = colors;
        GetComponent<MeshFilter>().sharedMesh = mesh;

    }
}
