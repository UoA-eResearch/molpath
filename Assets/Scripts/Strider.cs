using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strider : MonoBehaviour
{

	public RibbonMaker RibbonMaker;

	public Strider()
	{
	}

	public float errorThreshold = 30f;
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
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

	private void AnalyzePeptide(GameObject peptide)
	{
		// search immediate children in order.

		// each res has 3 bbus.

		// patterns are 6 things long.

		// var cjPhi_NCa = GetAmideForResidue(resid).GetComponent<ConfigurableJoint>();
		// cjPhi_NCa.targetRotation = Quaternion.Euler(180.0f - phi, 0, 0);

		// var cjPsi_CaCO = GetCalphaForResidue(resid).GetComponent<ConfigurableJoint>();
		// cjPsi_CaCO.targetRotation = Quaternion.Euler(180.0f - psi, 0, 0);


		// iterating entire chain
		Residue[] residues = peptide.transform.GetComponentsInChildren<Residue>();
		for (int i = 0; i < residues.Length; i++)
		{
			Residue residue = residues[i];
			Residue residueNext = residues[i+1];
			if (residues[i + 1])
			{
				if (IsHelical(residue) && IsHelical(residueNext))
				{
					// if residue helical, grab amide and add to segmentpoints.
					var controlPoint = Utility.GetFirstChildContainingText(residue.transform, "amide");
					var controlPoint2 = Utility.GetFirstChildContainingText(residueNext.transform, "amide");
					if (controlPoint && controlPoint2)
					{
		List<Transform> ribbonSegmentPoints = new List<Transform>();
		List<List<Transform>> ribbonSegments = new List<List<Transform>>();
					}
				}
			}
		}
	}

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
		if (Utility.VectorInRange(cfjs[0].targetRotation.eulerAngles, new Vector3(phi, 0, 0), errorThreshold / 2, 'x'))
		{
			// Debug.Log("phi within range" + cfjs[0].targetRotation.eulerAngles.x);
		}

		// the psi angle is the second angle. (i'm pretty sure.)
		// Debug.Log("psi x angle" + cfjs[1].targetRotation.eulerAngles.x);
		if (Utility.VectorInRange(cfjs[1].targetRotation.eulerAngles, new Vector3(psi, 0, 0), errorThreshold / 2, 'x'))
		{
			// Debug.Log("psi x angle in range" + cfjs[1].targetRotation.eulerAngles.x);
		}
		else
		{
			isHelical = false;
		}

		if (isHelical)
		{
			// Debug.Log("both bond angles are within correct range for helical pattern" + cfjs[0].targetRotation.eulerAngles.x + "," + cfjs[1].targetRotation.eulerAngles.x);
		}
		return isHelical;
	}
}
