using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour{

	public static Material GenerateDefaultMaterial() {
		GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
		primitive.SetActive(false);
		Material diffuse = primitive.GetComponent<MeshRenderer>().sharedMaterial;
		DestroyImmediate(primitive);
		return diffuse;
	}
	public static Transform GetFirstChildContainingText(Transform parent, string name)
	{
		for (int i = 0; i < parent.childCount; i++)
		{
			var child = parent.GetChild(i);
			if (child.name.ToLower().Contains("amide"))
			{
				return child.transform;
			}
		}
		return null;
	}
	public static bool VectorInRange(Vector3 vector, Vector3 target, float deviation, char axis)
	{
		bool inRange = false;
		switch (axis)
		{
			case 'x':
				// Debug.Log(vector);
				// Debug.Log(target.x + deviation);
				// Debug.Log(target.x - deviation);
				if (vector.x <= (target.x + deviation) && vector.x >= (target.x - deviation))
				{
					// Debug.Log("target base range: " + target.x);
					inRange = true;
				}
				else
				{
					inRange = false;
				}
				break;
			case 'y':
				if (vector.y <= (target.y + deviation) && vector.y >= (target.y - deviation))
				{
					inRange = true;
				}
				else
				{
					inRange = false;
				}
				break;
			case 'z':
				if (vector.z <= (target.z + deviation) && vector.z >= (target.z - deviation))
				{
					inRange = true;
				}
				else
				{
					inRange = false;
				}
				break;
			default:
				// Debug.Log("no axis specified.");
				break;
		}
		return inRange;
	}
}
