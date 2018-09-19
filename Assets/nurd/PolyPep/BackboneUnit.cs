﻿


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackboneUnit : MonoBehaviour {

	public int residue = -1;

	Shader shaderStandard;
	Shader shaderToonOutline;

	public bool activeSequenceSelect = false;
	private bool activeSequenceSelectLast = false;
	public bool controllerHoverOn = false;
	public bool controllerSelectOn = false;

	public Residue myResidue;

	// 
	private Renderer rendererPhi;	// amide only
	private Renderer rendererPsi;   // calpha only
	private Renderer rendererPeptide; // carbonyl only

	//private Renderer[] rendererAtoms;
	private List<Renderer> renderersAtoms = new List<Renderer>();

	// Use this for initialization
	void Start () {

		// init references to renderers
		{
			Renderer[] allChildRenderers = gameObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer childRenderer in allChildRenderers)
			{
				//Debug.Log(childRenderer.transform.gameObject.name);
				//Debug.Log(childRenderer.transform.gameObject.layer);

				//bonds
				if (childRenderer.transform.gameObject.name == "bond_N_CA")
				{
					rendererPhi = childRenderer;
				}
				if (childRenderer.transform.gameObject.name == "bond_CA_CO")
				{
					rendererPsi = childRenderer;
				}
				if (childRenderer.transform.gameObject.name == "bond_CO_N")
				{
					rendererPeptide = childRenderer;
				}

				//atoms
				if (childRenderer.transform.gameObject.layer == LayerMask.NameToLayer("Atom"))
				{
					//Debug.Log("got one!");
					renderersAtoms.Add(childRenderer);
				}
			}

		}

		myResidue = (gameObject.transform.parent.gameObject.GetComponent("Residue") as Residue);
		shaderStandard = Shader.Find("Standard");
		shaderToonOutline = Shader.Find("Toon/Basic Outline");
		UpdateRenderMode();
	}

	public void SetBackboneUnitControllerHover(bool flag)
	{
		controllerHoverOn = flag;
		UpdateRenderMode();
	}

	public void SetMyResidueSelect(bool flag)
	{
		//Residue res = (gameObject.transform.parent.gameObject.GetComponent("Residue") as Residue);
		if (myResidue)
		{
			//Debug.Log("             " + res);
			BackboneUnit buAmide = myResidue.amide_pf.GetComponent("BackboneUnit") as BackboneUnit;
			BackboneUnit buCalpha = myResidue.calpha_pf.GetComponent("BackboneUnit") as BackboneUnit;
			BackboneUnit buCarbonyl = myResidue.carbonyl_pf.GetComponent("BackboneUnit") as BackboneUnit;

			buAmide.SetBackboneUnitSelect(flag);
			buCalpha.SetBackboneUnitSelect(flag);
			buCarbonyl.SetBackboneUnitSelect(flag);
		}
	}

	public void SetBackboneUnitSelect (bool flag)
	{
		controllerSelectOn = flag;
		UpdateRenderMode();
	}

	public void SetActiveSequenceSelect(bool flag)
	{
		activeSequenceSelect = flag;
		if (activeSequenceSelect != activeSequenceSelectLast)
		{
			UpdateRenderMode();
			activeSequenceSelectLast = activeSequenceSelect;	
		}
	}

	public void TractorBeam(Ray pointer, bool attract)
	{
		Debug.Log("tractor beam me!");

		float tractorBeamAttractionFactor = 50.0f;

		Vector3 tractorBeam = pointer.origin - gameObject.transform.position;
		if (!attract)
		{
			// repel
			tractorBeam = gameObject.transform.position - pointer.origin;
		}
		float tractorBeamScale = Mathf.Max(100.0f, tractorBeamAttractionFactor * (Vector3.Magnitude(tractorBeam) / 500.0f));
		gameObject.GetComponent<Rigidbody>().AddForce((tractorBeam * tractorBeamScale), ForceMode.Acceleration);

	}

	private void SetRenderingModeOld(GameObject go, string shaderName)
	{
		Renderer[] allChildRenderers = go.GetComponentsInChildren<Renderer>();
		foreach (Renderer childRenderer in allChildRenderers)
		{
			//Debug.Log(child.GetType());

			var _type = childRenderer.GetType();
			if (_type.ToString() != "UnityEngine.ParticleSystemRenderer")

			{
				switch (shaderName)

				{
					case "ToonOutlineGreen":
						{
							childRenderer.GetComponent<Renderer>().material.shader = shaderStandard;
						}
						//{
						//	Renderer _renderer = childRenderer.GetComponent<Renderer>();
						//	_renderer.material.shader = shaderToonOutline;
						//	_renderer.material.SetColor("_OutlineColor", Color.green);
						//	_renderer.material.SetFloat("_Outline", 0.005f);
						//}
						break;

					case "ToonOutlineRed":
						{
							Renderer _renderer = childRenderer.GetComponent<Renderer>();
							_renderer.material.shader = shaderToonOutline;
							_renderer.material.SetColor("_OutlineColor", Color.red);
							_renderer.material.SetFloat("_Outline", 0.005f);
						}
						break;

					case "ToonOutlineYellow":
						{
							Renderer _renderer = childRenderer.GetComponent<Renderer>();
							_renderer.material.shader = shaderToonOutline;
							_renderer.material.SetColor("_OutlineColor", Color.yellow);
							_renderer.material.SetFloat("_Outline", 0.005f);
						}
						break;

					case "Standard":
						{
							childRenderer.GetComponent<Renderer>().material.shader = shaderStandard;
						}
						break;

				}

				if (childRenderer == rendererPhi)
				{
					if (true)//myResidue.drivePhiPsiOn)
					{
						Renderer _renderer = childRenderer.GetComponent<Renderer>();
						_renderer.material.shader = shaderToonOutline;
						_renderer.material.SetColor("_OutlineColor", Color.cyan);
						_renderer.material.SetFloat("_Outline", 0.005f);
					}
					else
					{
						childRenderer.GetComponent<Renderer>().material.shader = shaderStandard;
					}
				}
				if (childRenderer == rendererPsi)
				{
					if (true)//myResidue.drivePhiPsiOn)
					{
						//Renderer _renderer = childRenderer.GetComponent<Renderer>();
						//_renderer.material.shader = shaderToonOutline;
						//_renderer.material.SetColor("_OutlineColor", Color.magenta);
						//_renderer.material.SetFloat("_Outline", 0.005f);

						//Renderer _renderer = childRenderer.GetComponent<Renderer>();
						rendererPsi.material.shader = shaderToonOutline;
						rendererPsi.material.SetColor("_OutlineColor", Color.yellow);
						rendererPsi.material.SetFloat("_Outline", 0.005f);
					}
					else
					{
						childRenderer.GetComponent<Renderer>().material.shader = shaderStandard;
					}
				}
				if (childRenderer == rendererPeptide)
				{
					if (true)
					{
						Renderer _renderer = childRenderer.GetComponent<Renderer>();
						_renderer.material.shader = shaderToonOutline;
						_renderer.material.SetColor("_OutlineColor", Color.black);
						_renderer.material.SetFloat("_Outline", 0.005f);
					}
					else
					{

					}
				}
			}
			else
			{
				// particle system
			}
		}
	}

	private void SetRenderingMode(GameObject go, string shaderName)
	{

		//Renderer[] allChildRenderers = go.GetComponentsInChildren<Renderer>();
		//if (_type.ToString() != "UnityEngine.ParticleSystemRenderer")

		foreach (Renderer _rendererAtom in renderersAtoms)
		{
			{
				switch (shaderName)

				{
					case "ToonOutlineGreen":
						{
							_rendererAtom.material.shader = shaderStandard;
						}
						{
							_rendererAtom.material.shader = shaderToonOutline;
							_rendererAtom.material.SetColor("_OutlineColor", Color.green);
							_rendererAtom.material.SetFloat("_Outline", 0.005f);
						}
						break;

					case "ToonOutlineRed":
						{
							_rendererAtom.material.shader = shaderToonOutline;
							_rendererAtom.material.SetColor("_OutlineColor", Color.red);
							_rendererAtom.material.SetFloat("_Outline", 0.005f);
						}
						break;

					case "ToonOutlineYellow":
						{
							_rendererAtom.material.shader = shaderToonOutline;
							_rendererAtom.material.SetColor("_OutlineColor", Color.yellow);
							_rendererAtom.material.SetFloat("_Outline", 0.005f);
						}
						break;

					case "Standard":
						{
							_rendererAtom.material.shader = shaderStandard;
						}
						break;
				}
			}
		}


		if (rendererPhi)
		{
			if (true)//myResidue.drivePhiPsiOn)
			{
				rendererPhi.material.shader = shaderToonOutline;
				rendererPhi.material.SetColor("_OutlineColor", Color.cyan);
				rendererPhi.material.SetFloat("_Outline", 0.005f);
			}
			else
			{
				rendererPhi.material.shader = shaderStandard;
			}
		}

		if (rendererPsi)
		{
			if (true)//myResidue.drivePhiPsiOn)
			{
				rendererPsi.material.shader = shaderToonOutline;
				rendererPsi.material.SetColor("_OutlineColor", Color.magenta);
				rendererPsi.material.SetFloat("_Outline", 0.005f);
			}
			else
			{
				rendererPsi.material.shader = shaderStandard;
			}
		}

		if (rendererPeptide)
		{
			if (true)
			{
				rendererPeptide.material.shader = shaderToonOutline;
				rendererPeptide.material.SetColor("_OutlineColor", Color.black);
				rendererPeptide.material.SetFloat("_Outline", 0.005f);
			}
			else
			{
				rendererPeptide.material.shader = shaderStandard;
			}
		}

	}


	public void UpdateRenderMode()
	{
		if (controllerHoverOn)
		{
			SetRenderingMode(gameObject, "ToonOutlineRed");
		}
		else if (activeSequenceSelect)
		{
			SetRenderingMode(gameObject, "ToonOutlineGreen");
		}
		else if (controllerSelectOn)
		{
			SetRenderingMode(gameObject, "ToonOutlineYellow");
		}
		else
		{
			SetRenderingMode(gameObject, "Standard");
		}
	}

	// Update is called once per frame
	void Update () {
		//UpdateRenderMode();
	}
}
