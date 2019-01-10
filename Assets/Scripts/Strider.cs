using System;
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
		var ribbons = new Dictionary<string, List<Transform>>();
		var oldRibbons = new Dictionary<string, List<Transform>>();

		var pointIndexes = new List<int[]>();

		int startResidue = 0;
		int endResidue = 0;

		// iterating entire chain
		Residue[] residues = peptide.transform.GetComponentsInChildren<Residue>();
		for (int i = 0; i < residues.Length - 1; i++)
		{
			Residue residue = residues[i];
			endResidue = i;
			if (IsHelical(residue))
			{
				// // if residue helical, grab amide and add to segmentpoints.
				// var controlPoint = Utility.GetFirstChildContainingText(residue.transform, "amide");
				// var controlPoint2 = Utility.GetFirstChildContainingText(residueNext.transform, "amide");
				// if (controlPoint && controlPoint2)
				// {
				// 	// Debug.Log("make a ribbon " + residue.name);
				// 	if (!residue.transform.Find("ribbon"))
				// 	{
				// 		MakeRibbon(residue.transform, controlPoint, controlPoint2);
				// 	}
				// }
				points.Add(Utility.GetFirstChildContainingText(residue.transform, "amide"));
			}
			else
			{
				// adding to ribbons duplicate code.
				ribbons[MakeRibbonName(startResidue, endResidue)] = points;
			}
		}
		// adding to ribbons for final piece if no break in helical pattern
		ribbons[MakeRibbonName(startResidue, endResidue)] = points;
		// Debug.Log("end of ribbon. " + points.Count);

		// garbage collection for ribbons.
		ComparePrevRibbons(peptide.transform, oldRibbons, ribbons);
		MakeNewRibbons(peptide.transform, ribbons);
		oldRibbons = ribbons;
	}

	/// <summary>
	/// Compares new ribbon keys with old ones, for any old ribbons that arent found in current iteration destroy the old ribbons.
	/// </summary>
	/// <param name="oldRibbons"></param>
	/// <param name="ribbons"></param>
	private void ComparePrevRibbons(Transform peptide, Dictionary<string, List<Transform>> oldRibbons, Dictionary<string, List<Transform>> ribbons)
	{
		foreach (var oldRibbon in oldRibbons)
		{
			if (!ribbons.ContainsKey(oldRibbon.Key))
			{
				Debug.Log("destorying old ribbon: " + oldRibbon.Key);
				// var x = oldRibbons[oldRibbon.Key];
				oldRibbons.Remove(oldRibbon.Key);
				Destroy(peptide.transform.Find(oldRibbon.Key).gameObject);
			}
			else {
				ribbons.Remove(oldRibbon.Key);
			}
		}
	}

	private void MakeNewRibbons(Transform peptide, Dictionary<string, List<Transform>> ribbons){
		foreach (var ribbonEntry in ribbons)
		{
			Debug.Log("making ribbon" + ribbonEntry.Key);
			MakeRibbon(peptide, ribbonEntry.Value);
		}
	}

	private GameObject MakeRibbon(Transform peptide, List<Transform> points){
		// make a ribbon
		GameObject ribbon = new GameObject("ribbon");
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
