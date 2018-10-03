﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class PolyPepManager : MonoBehaviour {


	public GameObject polyPepBuilder_pf;
	public List<PolyPepBuilder> allPolyPepBuilders = new List<PolyPepBuilder>();

	public bool collidersOn = false;

	public int UIDefinedSecondaryStructure { get; set; }

	public float phiTarget = 0f;
	public float psiTarget = 0f;

	public Slider phiSliderUI;
	public Slider psiSliderUI;
	public Slider vdwSliderUI;
	public Slider scaleSliderUI;
	public Slider hbondSliderUI;
	public Slider phiPsiDriveSliderUI;
	public Slider spawnLengthSliderUI;


	void Awake()
	{
			GameObject temp = GameObject.Find("Slider_Phi");
			phiSliderUI = temp.GetComponent<Slider>();

			temp = GameObject.Find("Slider_Psi");
			psiSliderUI = temp.GetComponent<Slider>();

			temp = GameObject.Find("Slider_Vdw");
			vdwSliderUI = temp.GetComponent<Slider>();

			temp = GameObject.Find("Slider_HbondStrength");
			hbondSliderUI = temp.GetComponent<Slider>();

			temp = GameObject.Find("Slider_PhiPsiDrive");
			phiPsiDriveSliderUI = temp.GetComponent<Slider>();

			temp = GameObject.Find("Slider_SpawnLength");
			spawnLengthSliderUI = temp.GetComponent<Slider>();

	}

	void Start()
	{

		{
			//UI
			// initialise phi psi slider values (hacky?)

			phiSliderUI.GetComponent<Slider>().value = 0;
			psiSliderUI.GetComponent<Slider>().value = 0;
			vdwSliderUI.GetComponent<Slider>().value = 10;
			hbondSliderUI.GetComponent<Slider>().value = 200;
			phiPsiDriveSliderUI.GetComponent<Slider>().value = 200;
			spawnLengthSliderUI.GetComponent<Slider>().value = 12;

			//temp = GameObject.Find("Slider_ResStart");

			//resStartSliderUI = temp.GetComponent<Slider>();
			//resStartSliderUI.maxValue = numResidues;
			//resStartSliderUI.value = 1;

			//temp = GameObject.Find("Slider_ResEnd");

			//Assert.IsNotNull(temp);

			//resEndSliderUI = temp.GetComponent<Slider>();
			//resEndSliderUI.maxValue = numResidues;
			//resEndSliderUI.value = 3; // numResidues; // initial value (+1)

			//temp = GameObject.Find("Slider_Scale");

			//scaleSliderUI = temp.GetComponent<Slider>();
			//scaleSliderUI.value = 10;

		}

		//{
		//	// create chains - hard coded for the moment

		//	for (int i = 0; i < 6; i++)
		//	{
		//		GameObject pp = Instantiate(polyPepBuilder_pf, new Vector3(0f, 0f + (i*1), 0f), Quaternion.identity);
		//		PolyPepBuilder pp_cs = pp.GetComponent<PolyPepBuilder>();
		//		pp_cs.numResidues = 14;
		//		pp.name = "polyPep_" + (i).ToString();
		//	}
		//}

		//foreach (PolyPepBuilder polyPep in FindObjectsOfType<PolyPepBuilder>() )
		//{
		//	//Debug.Log("------------------->" + polyPep);
		//	allPolyPepBuilders.Add(polyPep);

		//}



	}

	public void SpawnPolypeptide(Transform spawnTransform)
	{
		// create chains - hard coded for the moment

		Debug.Log(spawnLengthSliderUI.GetComponent<Slider>().value);

		for (int i = 0; i< 1; i++)
		{
			//GameObject pp = Instantiate(polyPepBuilder_pf, new Vector3(0f, 0f + (i * 1), 0f), Quaternion.identity);
			Debug.Log(spawnTransform.position);
			GameObject pp = Instantiate(polyPepBuilder_pf, spawnTransform.position, Quaternion.identity);
			PolyPepBuilder pp_cs = pp.GetComponent<PolyPepBuilder>();
			pp_cs.numResidues = (int)spawnLengthSliderUI.GetComponent<Slider>().value;
			pp_cs.buildTransform = spawnTransform;
			//pp.name = "polyPep_" + (i).ToString();
			pp.name = "polyPep_" + allPolyPepBuilders.Count;
			allPolyPepBuilders.Add(pp_cs);
			pp_cs.ScaleVDW(vdwSliderUI.GetComponent<Slider>().value / 10.0f);
		}
	}



	public void UpdateVDWScalesFromUI(float scaleVDWx10)
	{

		//Debug.Log("hello from the manager! ---> " + scaleVDWx10);
		foreach (PolyPepBuilder _ppb in allPolyPepBuilders)
		{
			_ppb.ScaleVDW(scaleVDWx10 / 10.0f);
		}
	}

	public void UpdateCollidersFromUI(bool value)
	{
		//Debug.Log("hello from the manager! ---> " + scaleVDWx10);
		foreach (PolyPepBuilder _ppb in allPolyPepBuilders)
		{
			_ppb.SetAllColliderIsTrigger(!value);
		}
	}

	public void UpdateHbondOnFromUI(bool value)
	{
		//Debug.Log("hello from the manager! ---> " + scaleVDWx10);
		foreach (PolyPepBuilder _ppb in allPolyPepBuilders)
		{
			_ppb.ActiveHbondSpringConstraints = value;
			_ppb.UpdateHBondSprings();
		}
	}

	public void UpdateHbondStrengthFromUI(float hbondStrength)
	{

		//Debug.Log("hello Hbond Strength from the manager! ---> " + hbondStrength);
		foreach (PolyPepBuilder _ppb in allPolyPepBuilders)
		{
			_ppb.hbondStrength = hbondStrength;
			_ppb.UpdateHBondSprings();
		}
	}

	public void UpdatePhiPsiDriveFromUI(float phiPsiDrive)
	{

		//Debug.Log("hello PhiPsi Drive from the manager! ---> " + phiPsiDrive);
		foreach (PolyPepBuilder _ppb in allPolyPepBuilders)
		{
			_ppb.drivePhiPsiMaxForce = phiPsiDrive;
			_ppb.drivePhiPsiPosSpring = phiPsiDrive;
			_ppb.UpdatePhiPsiDrives();

			// drivePhiPsiPosDamper ?
		}
	}

	public void SelectAllFromUI(bool value)
	{
		//Debug.Log("Select All from the manager! ---> " +  value);
		foreach (PolyPepBuilder _ppb in allPolyPepBuilders)
		{
			_ppb.SetGlobalSelect(value);
		}
	}

	public void UpdateDefinedSecondaryStructureFromUI()
	{
		float phi = phiTarget;
		float psi = psiTarget;
		switch (UIDefinedSecondaryStructure)
		{
			case 0:     
				// not defined
				phi = phiTarget;
				psi = psiTarget;
				break;

			case 1:     
				//alpha helix (right handed) (phi + ps ~ -105)
				phi = -57.0f;
				psi = -47.0f;
				break;

			case 2:     
				//310 helix (phi + psi ~ -75)
				phi = -49.0f;// -74.0f;
				psi = -26.0f;// -4.0f;
				break;

			case 3:     
				//anti beta sheet
				phi = -139.0f;
				psi = 135.0f;
				break;

			case 4:     
				//parallel beta sheet
				phi = -119.0f;
				psi = 113.0f;
				break;

			case 5:     
				//pi helix (phi + ps ~ -125)
				phi = -55.0f;
				psi = -70.0f;
				break;

			case 6:     
				//alpha helix (left handed)
				phi = 47.0f;
				psi = 57.0f;
				break;
		}

		phiTarget = phi;
		psiTarget = psi;
		
		phiSliderUI.value = phi;
		psiSliderUI.value = psi;

		UpdatePhiPsiForPolyPeptides();
	}

	public void UpdatePhiFromUI(float phi)
	{
		//Debug.Log("hello from the manager! ---> " + phi);
		phiTarget = phi;
		UpdatePhiPsiForPolyPeptides();
	}

	public void UpdatePsiFromUI(float psi)
	{
		//Debug.Log("hello from the manager! ---> " + psi);
		psiTarget = psi;
		UpdatePhiPsiForPolyPeptides();
	}

	private void UpdatePhiPsiForPolyPeptides()
	{
		foreach (PolyPepBuilder _ppb in allPolyPepBuilders)
		{
			_ppb.SetPhiPsiForSelection(phiTarget, psiTarget);
		}
	}

	public void ResetLevel()
	{
		Scene m_Scene = SceneManager.GetActiveScene();
		Debug.Log("Loading... " + m_Scene.name);
		SceneManager.LoadScene(m_Scene.name);
	}

	// Update is called once per frame
	void Update () {
		
	}
}
