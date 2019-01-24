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
	public static bool VectorInRange(Vector3 curRotation, Vector3 targetRotation, float deviation, char axis)
	{
		switch (axis)
		{
			// Debug.Log(vector);
			// Debug.Log(target.x + deviation);
			// Debug.Log(target.x - deviation);
			case 'x':
				// Debug.Log(curRotation);
				// Debug.Log(targetRotation);
				// Debug.Log(curRotation.x <= (targetRotation.x + deviation) && curRotation.x >= (targetRotation.x - deviation));
				return (curRotation.x <= (targetRotation.x + deviation) && curRotation.x >= (targetRotation.x - deviation));
			case 'y':
				return (curRotation.y <= (targetRotation.y + deviation) && curRotation.y >= (targetRotation.y - deviation));
			case 'z':
				return (curRotation.z <= (targetRotation.z + deviation) && curRotation.z >= (targetRotation.z - deviation));
			default:
				return false;
		}
	}
}
