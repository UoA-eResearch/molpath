using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class Strider : MonoBehaviour
{
	public Material ribbonMaterial;
	public RibbonMaker RibbonMaker;
	public float errorThreshold = 30f;

	private void FixedUpdate()
	{
		Profiler.BeginSample("Ribbon creation");
		GameObject peptide = GameObject.Find("polyPep_0");
		if (peptide)
		{
			// Debug.Log("peptide has spawned");
			AnalyzePeptide(peptide);
		}
		Profiler.EndSample();
	}

	private static int SortByName(GameObject o1, GameObject o2)
	{
		return o1.name.CompareTo(o2.name);
	}

	private string MakeRibbonName(int start, int end) {
		return start + "-" + end;
	}


	/// <summary>
	/// Iterates a peptide child transforms and creates ribbon instances where bond parameters match
	/// </summary>
	/// <param name="peptide"></param>
	private void AnalyzePeptide(GameObject peptide)
	{
		var points = new List<Transform>();
		var ribbonsToAdd = new Dictionary<string, List<Transform>>();
		var ribbonsExisting = new Dictionary<string, List<Transform>>();

		var pointIndexes = new List<int[]>();

		int startResidue = 0;
		int endResidue = 0;
		string ribbonKey = "";
		// iterating entire chain
		Residue[] residues = peptide.transform.GetComponentsInChildren<Residue>();
		for (int i = 0; i < residues.Length - 1; i++)
		{
			Residue residue = residues[i];
			endResidue = i;
			if (IsHelical(residue))
			{
				points.Add(Utility.GetFirstChildContainingText(residue.transform, "amide"));
			}
			else
			{
				// only add if doesn't exist.
				ribbonKey = MakeRibbonName(startResidue, endResidue);
				if (!ribbonsExisting.ContainsKey(ribbonKey)) {
					ribbonsToAdd[ribbonKey] = points;
				}
			}
		}
		// adding to ribbons for final piece if no break in helical pattern
		// only add if doesn't exist.
		ribbonKey = MakeRibbonName(startResidue, endResidue);
		if (!ribbonsExisting.ContainsKey(ribbonKey))
		{
			ribbonsToAdd[ribbonKey] = points;
		}

		ProcessRibbonChanges(peptide.transform, ribbonsToAdd, ribbonsExisting);
		MakeNewRibbons(peptide.transform, ribbonsToAdd);
	}

/// <summary>
/// Removes old ribbons, reduces new ribbons to be added 
/// </summary>
/// <param name="root"></param>
/// <param name="ribbonsToAdd"></param>
/// <param name="ribbonsExisting"></param>
	private void ProcessRibbonChanges(Transform root, Dictionary<string, List<Transform>> ribbonsToAdd, Dictionary<string, List<Transform>> ribbonsExisting)
	{
		// compare old with new, destroy olds not present in using UnityEngine;
		foreach (var ribbonExisting in ribbonsExisting)
		{
			if (!ribbonsToAdd.ContainsKey(ribbonExisting.Key))
			{
				var ribbonToDelete = root.Find(ribbonExisting.Key);
				if (ribbonToDelete)
				{
					Destroy(ribbonToDelete);
				}
			}
		}

		// compare new with olds. remove olds from new as they already exist.
		// in theory shouldn't need to do this part as the entry validation is already done in analyzePeptide.
		foreach (var ribbonToAdd in ribbonsToAdd)
		{
			if (ribbonsExisting.ContainsKey(ribbonToAdd.Key))
			{
				ribbonsToAdd.Remove(ribbonToAdd.Key);
			}
			else
			{
				ribbonsExisting[ribbonToAdd.Key] = ribbonToAdd.Value;
			}
		}
	}

	private void MakeNewRibbons(Transform peptide, Dictionary<string, List<Transform>> ribbons){
		foreach (var ribbonEntry in ribbons)
		{
			// Debug.Log("making ribbon" + ribbonEntry.Key);
			if (peptide.transform.Find(ribbonEntry.Key)) {
				// Debug.Log("ribbon exists, not creating new ribbon.");
			}
			else
			{
				MakeRibbon(peptide, ribbonEntry.Key, ribbonEntry.Value);
			}
		}
	}

	private GameObject MakeRibbon(Transform peptide, string ribbonName, List<Transform> points){
		// make a ribbon
		GameObject ribbon = new GameObject(ribbonName);
		ribbon.transform.parent = peptide.transform;
		ribbon.AddComponent<MeshRenderer>();
		ribbon.AddComponent<MeshFilter>();
		RibbonMaker ribbonMaker = ribbon.AddComponent<RibbonMaker>();
		ribbonMaker.Material = ribbonMaterial;
		foreach (var point in points)
		{
			ribbonMaker.controlPoints.Add(point);
		}
		return ribbon;
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
		else
		{
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
