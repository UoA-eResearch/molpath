using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strider : MonoBehaviour
{

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


		//error threshold.
		// alpha helical = psi 50, phi 60.
		foreach (var residue in peptide.transform.GetComponentsInChildren<Residue>())
		{
			Debug.Log(residue.transform.name);
			BackboneUnit[] bbus = residue.transform.GetComponentsInChildren<BackboneUnit>();

			bool isHelical = true;
			// psi
			if (bbus[0].GetComponent<ConfigurableJoint>().targetRotation != Quaternion.Euler(180.0f - 50, 0, 0))
			{
				isHelical = false;
			}
			// phi
			if (bbus[1].GetComponent<ConfigurableJoint>().targetRotation != Quaternion.Euler(180.0f - 60, 0, 0))
			{
				isHelical = false;
			}

			if (isHelical)
			{
				Debug.Log(residue.transform.name + "is helical.");
			}
		}
	}
}
