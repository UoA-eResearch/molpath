using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RibbonMaker : MonoBehaviour
{
    //Use the transforms of GameObjects in 3d space as your points or define array with desired points
    public List<Transform> controlPoints = new List<Transform>();
    public List<GameObject> controlPointGos = new List<GameObject>();

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
    public int interpolationSteps = 5;

    private MeshRenderer _meshRenderer;
    public Material Material;
    public int crossSegments = 5;
    public Vector3[] crossPoints;
    public int lastCrossSegments;
    private Vector3[] interpolatedPositions;

	public List<string> controlPointTags;


	public RibbonMaker(Transform root, List<Transform> controlPoints)
	{
		this.controlPoints = controlPoints;
		// expand later for consecutive residues

		GameObject ribbonGo = new GameObject("ribbonGo", typeof(MeshRenderer));
		ribbonGo.transform.parent = root;

		_meshRenderer = ribbonGo.GetComponent<MeshRenderer>();
        _meshRenderer.material = new Material(Utility.GenerateDefaultMaterial().shader);
	}


	private void Start()
    {
		// line.positionCount = controlPoints.Count * numberOfPoints;
		_meshRenderer = gameObject.GetComponent<MeshRenderer>();
        _meshRenderer.material = new Material(Material.shader);
    }

	private static int SortByName(GameObject o1, GameObject o2)
	{
		return o1.name.CompareTo(o2.name);
	}

    private void FixedUpdate()
    {
		// controlPoints.Clear();
		// controlPointGos.Clear();

		// TODO: interpolates better using just singular point per residue.
		// TODO: Alternatively, average the residue vertices and use that as the control point.


		// foreach (var tag in controlPointTags) 
		// {
		// 	foreach (var go in GameObject.FindGameObjectsWithTag(tag))
		// 	{
		// 		controlPointGos.Add(go);
		// 	}
		// }

		// // sort control points by otherwise it grabs them out of order
		// controlPointGos.Sort(SortByName);
		// // add sorted transforms 
        // foreach (var controlPointGo in controlPointGos)
        // {
		// 	// Debug.Log(controlPointGo.name);
		// 	controlPoints.Add(controlPointGo.transform);
		// }
        if (controlPoints.Count < 0) {
			return;
		}
        Vector3[] interpolatedPositions = InterpolateControlPoints(controlPoints);
        TubeVertex[]  tubeVertices =  CreateTubeVertices(interpolatedPositions);
		// Debug.Log(interpolatedPositions[interpolatedPositions.Length -1]);
		// Debug.Log(tubeVertices.Length);
        CreateTubeMesh(tubeVertices);
    }

    private Vector3[] InterpolateControlPoints(List<Transform> controlPoints)
    {
		// C - C - C- C 

		// Vector3[] interpolatedPositions = new Vector3[interpolationSteps * controlPoints.Count];
		// interpolatedPositions[i + j * interpolationSteps] = position;

		Vector3[] interpolatedPositions = new Vector3[((controlPoints.Count - 1) * interpolationSteps)];
		// Debug.Log(interpolatedPositions.Length); // 45

		Vector3 p0, p1, m0, m1;
        for (int j = 0; j < controlPoints.Count - 1; j++)
        {
            // check control points
            if (controlPoints[j] == null || controlPoints[j + 1] == null ||
                (j > 0 && controlPoints[j - 1] == null) ||
                (j < controlPoints.Count - 2 && controlPoints[j + 2] == null))
            {
				Debug.Log("control points wrong index size.");
                return null;
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
            float pointStep = 1.0f / interpolationSteps;

            if (j == controlPoints.Count - 2)
            {
                pointStep = 1.0f / (interpolationSteps - 1.0f);
                // last point of last segment should reach p1
            }
            for (int i = 0; i < interpolationSteps; i++)
            {
                t = i * pointStep;
                position = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * p0
                    + (t * t * t - 2.0f * t * t + t) * m0
                    + (-2.0f * t * t * t + 3.0f * t * t) * p1
                    + (t * t * t - t * t) * m1;
                // line.SetPosition(i + j * numberOfPoints,
                //     position);
                interpolatedPositions[(j  * interpolationSteps) + i] = position;
            }
        }
		return interpolatedPositions;
	}


	private void OnDrawGizmos()
	{
		if (controlPoints != null)
		{
			foreach (var point in controlPoints)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(point.position, 1);
			}
		}

		if (interpolatedPositions != null) 
		{
			for (int i = 0; i < interpolatedPositions.Length; i++)
			{
				var position = interpolatedPositions[i];
				Debug.Log(i + " position: " + position);
				Gizmos.color = Color.blue;
				Gizmos.DrawCube(position, Vector3.one);
			}	
		}
	}

    private TubeVertex[] CreateTubeVertices(Vector3[] interpolatedPositions)
	{
        // Debug.Log(interpolatedPositions);
		TubeVertex[] tubeVertices = new TubeVertex[interpolatedPositions.Length];
		for (int i = 0; i < interpolatedPositions.Length; i++)
        {
            tubeVertices[i] = new TubeVertex(interpolatedPositions[i], tubeRadius);
        }
		return tubeVertices;
	}


    // basically the for calculating negative angles I think. Don't touch this one. Needs to be 2.
	private float thetaDivider = 2.0f;
	[Header("Tube params")]
    [Tooltip("How much flatter the x scale of the tube will be vs the y axis.")]
	public float tubeXScaleDivider = 1.0f;
	public float tubeRadius = 0.25f;

	/// <summary>
	/// iterates through tube vertices and does the vertices calcuations for them.
	/// </summary>
	private void CreateTubeMesh(TubeVertex[] tubeVertices)
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
            float theta = thetaDivider * Mathf.PI / crossSegments;
            for (int c = 0; c < crossSegments; c++)
            {
                // crossPoints[c] = new Vector3(Mathf.Cos(theta * c) / 10.0f, Mathf.Sin(theta * c), 0);
                crossPoints[c] = new Vector3(Mathf.Cos(theta * c) / tubeXScaleDivider, Mathf.Sin(theta * c), 0);
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
            {
                // if there's a next point, set the rotation of the current tube vertex to aim at the next one.
                rotation = Quaternion.FromToRotation(Vector3.forward, tubeVertices[p + 1].point - tubeVertices[p].point);
            }

            for (int c = 0; c < crossSegments; c++)
            {
                // creating a set of cross section vertices
                int vertexIndex = p * crossSegments + c;
                meshVertices[vertexIndex] = tubeVertices[p].point + rotation * crossPoints[c] * tubeVertices[p].radius;
                uvs[vertexIndex] = new Vector2((0.0f + c) / crossSegments, (0.0f + p) / tubeVertices.Length);
                // colors[vertexIndex] = Color.yellow;
                // colors[vertexIndex] = tubeVertices[p].color;
                lastVertices[c] = theseVertices[c];
                theseVertices[c] = p * crossSegments + c;
            }
            if (p > 0)
            {
				//make triangles
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
/* TODO:
Important:
- Need to cap the ends of the generated mesh.
- figure out which points to use as spline control points. 
    -- one of the bb atoms vs. 
    -- residue transform vs. 
    -- average of bbu positions.
- Calculate when a tube or ribbon starts + ends (Strider's algorithm)

Additional:
- Mesh vertices are folding within itself (should join interfacting faces and cull the other side of the face. or somehow lerp the verts).
    -- Maybe sort interpolated cross sections by proximity rather than in order of name/residue as (then again it might not connect properly if that's the case)
    -- Alternatively could use the tube vert point cloud and do delaunay triangulation to get a non-overlapping faces mesh.
*/
