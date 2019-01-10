using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strider : MonoBehaviour
{
	public Material ribbonMaterial;
	public RibbonMaker RibbonMaker;
	public float errorThreshold = 30f;

	void Update()
	{
		GameObject peptide = GameObject.Find("polyPep_0");
		if (peptide)
		{
			// Debug.Log("peptide has spawned");
			AnalyzePeptide(peptide);
		}
	}

	private static int SortByName(GameObject o1, GameObject o2)
	{
		return o1.name.CompareTo(o2.name);
	}

	private string MakeRibbonKey(int a, int b) {
		return a + "-" + b;
	}


	/// <summary>
	/// Iterates a peptide child transforms and creates ribbon instances where bond parameters match
	/// </summary>
	/// <param name="peptide"></param>
	private void AnalyzePeptide(GameObject peptide)
	{
		var points = new List<Transform>();
		var ribbons = new Dictionary<string, List<Transform>>();

		int startResidue = 0;
		int endResidue = 0;

		 // Debug.Log(typeof(string).Assembly.ImageRuntimeVersion);

		Residue[] residues = peptide.transform.GetComponentsInChildren<Residue>();
		for (int i = 0; i < residues.Length - 1; i++)
		{
			var residue = residues[i];
			if (IsHelical(residue)) {
				
			}
			else {
				endResidue = i;
				// do some things
				


				startResidue = i;
			}
		}
		// if (false)
		// {
		// 	// iterating entire chain
		// 	Residue[] residues = peptide.transform.GetComponentsInChildren<Residue>();
		// 	for (int i = 0; i < residues.Length - 1; i++)
		// 	{
		// 		Residue residue = residues[i];
		// 		Residue residueNext = residues[i + 1];
		// 		if (residues[i + 1])
		// 		{
		// 			if (IsHelical(residue) && IsHelical(residueNext))
		// 			{
		// 				// if residue helical, grab amide and add to segmentpoints.
		// 				var controlPoint = Utility.GetFirstChildContainingText(residue.transform, "amide");
		// 				var controlPoint2 = Utility.GetFirstChildContainingText(residueNext.transform, "amide");
		// 				if (controlPoint && controlPoint2)
		// 				{
		// 					// Debug.Log("make a ribbon " + residue.name);

		// 					// List<Transform> ribbonSegmentPoints = new List<Transform>();
		// 					// List<List<Transform>> ribbonSegments = new List<List<Transform>>();
		// 					if (!residue.transform.Find("ribbon"))
		// 					{
		// 						// make a ribbon
		// 						GameObject ribbon = new GameObject("ribbon");
		// 						ribbon.transform.parent = residue.transform;
		// 						ribbon.AddComponent<MeshRenderer>();
		// 						ribbon.AddComponent<MeshFilter>();
		// 						RibbonMaker ribbonMaker = ribbon.AddComponent<RibbonMaker>();
		// 						ribbonMaker.Material = ribbonMaterial;
		// 						ribbonMaker.controlPoints.Add(controlPoint);
		// 						ribbonMaker.controlPoints.Add(controlPoint2);
		// 					}
		// 				}
		// 			}
		// 			else
		// 			{
		// 				Destroy(residue.transform.Find("ribbon").gameObject);
		// 			}
		// 		}
		// 	}

		// }
	}

	/// <summary>
	///  Determines if a residue has helical bonds by looking at joint values within a value range returns true if helical.
	/// </summary>
	/// <param name="residue"></param>
	/// <returns>boolean</returns>
	private bool IsHelical(Residue residue)
	{
		//error threshold.
		// alpha helical = psi 50, phi 60.
		// Debug.Log(residue.transform.name);
		BackboneUnit[] bbus = residue.transform.GetComponentsInChildren<BackboneUnit>();
		ConfigurableJoint[] cfjs = residue.transform.GetComponentsInChildren<ConfigurableJoint>();

		// Debug.Log(cfjs[0].targetRotation);

		// this is so it fits to the plot being 180 degrees I suppose?
		// if (cfjs[0].targetRotation.eulerAngles.x <= Quaternion.Euler(180.0f - 50, 0, 0)) 

		// if (Utility.VectorInRange(cfjs[0].targetRotation.eulerAngles, new Vector3(180.0f - 60, 0, 0), 5f, 'x')) 

		bool isHelical = true;
		float phi = 60f;
		float psi = 50f;
		if (!Utility.VectorInRange(cfjs[0].targetRotation.eulerAngles, new Vector3(phi, 0, 0), errorThreshold / 2, 'x'))
		{
			isHelical = false;
		}
		else {
			// Debug.Log("phi within range" + cfjs[0].targetRotation.eulerAngles.x);
		}

		// the psi angle is the second angle. (i'm pretty sure.)
		// Debug.Log("psi x angle" + cfjs[1].targetRotation.eulerAngles.x);
		if (!Utility.VectorInRange(cfjs[1].targetRotation.eulerAngles, new Vector3(psi, 0, 0), errorThreshold / 2, 'x'))
		{
			isHelical = false;
		}
		else
		{
			// Debug.Log("psi x angle in range" + cfjs[1].targetRotation.eulerAngles.x);
		}

		if (isHelical)
		{
			// Debug.Log("both bond angles are within correct range for helical pattern" + cfjs[0].targetRotation.eulerAngles.x + "," + cfjs[1].targetRotation.eulerAngles.x);
		}
		return isHelical;
	}
}
