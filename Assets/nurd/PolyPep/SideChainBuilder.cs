﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideChainBuilder : MonoBehaviour {


	public GameObject Csp3_pf;
	//public List<GameObject> residue_cssideChainList = new List<GameObject>();

	int sideChainLength = 0;

	// debug vectors for gizmos
	public Vector3 posA = new Vector3(0f, 0f, 0f);
	public Vector3 posB = new Vector3(0f, 0f, 0f);
	public Vector3 posC = new Vector3(0f, 0f, 0f);

	// Use this for initialization
	void Start () {

	}

	public void BuildSideChain(GameObject ppb_go, int resid, string type)
	{
		PolyPepBuilder ppb_cs = ppb_go.GetComponent<PolyPepBuilder>();
		Residue residue_cs = ppb_cs.chainArr[resid].GetComponent<Residue>();

		// ought to do this in residue.cs ?
		residue_cs.sidechain = new GameObject(resid + "_" + type);
		residue_cs.sidechain.transform.parent = residue_cs.transform;

		// set default atom type to sp3 - used in Awake() when instantiated
		Csp3_pf.GetComponent<Csp3>().atomType = "sp3";

		switch (type)
		{
			case "ALA":
				build_ALA(residue_cs);
				break;
			case "VAL":
				build_VAL(residue_cs);
				break;
			case "LEU":
				build_LEU(residue_cs);
				break;
			case "ILE":
				build_ILE(residue_cs);
				break;
			case "MET":
				build_MET(residue_cs);
				break;
			case "CYS":
				build_CYS(residue_cs);
				break;
			case "SER":
				build_SER(residue_cs);
				break;
			case "THR":
				build_THR(residue_cs);
				break;
			case "LYS":
				build_LYS(residue_cs);
				break;
			case "ASP":
				build_ASP(residue_cs);
				break;
			case "GLU":
				build_GLU(residue_cs);
				break;
			case "TEST":
				build_TEST(residue_cs);
				break;
			case "DEV":
				build_DEV(residue_cs);
				break;
			default:
				break;
		}

		residue_cs.DisableProxySideChain();
	}

	void build_ALA(Residue residue_cs)
	{
		sideChainLength = 1;
		for (int i = 0; i < sideChainLength; i++)
		{
			residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
		}
		GameObject _CB = residue_cs.sideChainList[0];
		_CB.name = "CB";

		{
			// Get CBeta position => R group
			Transform CB_tf = residue_cs.calpha_pf.transform.Find("tf_sidechain/R_sidechain");
			// Get CAlpha position
			Transform CA_tf = residue_cs.calpha_pf.transform;

			// Place and orient CBeta
			_CB.transform.position = CB_tf.position;
			_CB.transform.LookAt(CA_tf.position);
			AddConfigJointBond(_CB, residue_cs.calpha_pf);
		}
		_CB.GetComponent<Csp3>().ConvertToCH3();
	}

	void build_VAL(Residue residue_cs)
	{
		sideChainLength = 3;
		for (int i = 0; i < sideChainLength; i++)
		{
			residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
		}
		GameObject _CB = residue_cs.sideChainList[0];
		GameObject _CG1 = residue_cs.sideChainList[1];
		GameObject _CG2 = residue_cs.sideChainList[2];

		_CB.name = "CB";
		_CG1.name = "CG1";
		_CG2.name = "CG2";

		{
			// Get CBeta position => R group
			Transform CB_tf = residue_cs.calpha_pf.transform.Find("tf_sidechain/R_sidechain");
			// Get CAlpha position
			Transform CA_tf = residue_cs.calpha_pf.transform;

			// Place and orient CBeta
			_CB.transform.position = CB_tf.position;
			_CB.transform.LookAt(CA_tf.position);
			AddConfigJointBond(_CB, residue_cs.calpha_pf);
		}
		{
			_CG1.transform.position = _CB.transform.Find("H_3").position;
			_CG1.transform.LookAt(_CB.transform.position);
			AddConfigJointBond(_CG1, _CB);
		}
		{
			_CG2.transform.position = _CB.transform.Find("H_2").position;
			_CG2.transform.LookAt(_CB.transform.position);
			AddConfigJointBond(_CG2, _CB);
		}

		_CB.GetComponent<Csp3>().ConvertToCH1();
		_CG1.GetComponent<Csp3>().ConvertToCH3();
		_CG2.GetComponent<Csp3>().ConvertToCH3();
	}

	void build_LEU(Residue residue_cs)
	{
		sideChainLength = 4;
		for (int i = 0; i < sideChainLength; i++)
		{
			residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
		}
		GameObject _CB = residue_cs.sideChainList[0];
		GameObject _CG = residue_cs.sideChainList[1];
		GameObject _CD1 = residue_cs.sideChainList[2];
		GameObject _CD2 = residue_cs.sideChainList[3];

		_CB.name = "CB";
		_CG.name = "CG";
		_CD1.name = "CD1";
		_CD2.name = "CD2";

		{
			// Get CBeta position => R group
			Transform CB_tf = residue_cs.calpha_pf.transform.Find("tf_sidechain/R_sidechain");
			// Get CAlpha position
			Transform CA_tf = residue_cs.calpha_pf.transform;

			// Place and orient CBeta
			_CB.transform.position = CB_tf.position;
			_CB.transform.LookAt(CA_tf.position);
			AddConfigJointBond(_CB, residue_cs.calpha_pf);
		}
		{
			_CG.transform.position = _CB.transform.Find("H_3").position;
			_CG.transform.LookAt(_CB.transform.position);
			AddConfigJointBond(_CG, _CB);
		}
		{
			_CD1.transform.position = _CG.transform.Find("H_3").position;
			_CD1.transform.LookAt(_CG.transform.position);
			AddConfigJointBond(_CD1, _CG);
		}
		{
			_CD2.transform.position = _CG.transform.Find("H_2").position;
			_CD2.transform.LookAt(_CG.transform.position);
			AddConfigJointBond(_CD2, _CG);
		}

		_CB.GetComponent<Csp3>().ConvertToCH2();

		_CG.GetComponent<Csp3>().ConvertToCH1();

		_CD1.GetComponent<Csp3>().ConvertToCH3();
		_CD2.GetComponent<Csp3>().ConvertToCH3();

	}

	void build_ILE(Residue residue_cs)
	{
		sideChainLength = 4;
		for (int i = 0; i < sideChainLength; i++)
		{
			residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
		}

		// ILE has counterintuitive atom nomenclature - need to check DGD
		// CB is chiral - need to check DGD - checked!

		GameObject _CB = residue_cs.sideChainList[0];
		GameObject _CG1 = residue_cs.sideChainList[1];
		GameObject _CG2 = residue_cs.sideChainList[2];
		GameObject _CD1 = residue_cs.sideChainList[3];

		_CB.name = "CB";
		_CG1.name = "CG1";
		_CG2.name = "CG2";
		_CD1.name = "CD1";

		{
			// Get CBeta position => R group
			Transform CB_tf = residue_cs.calpha_pf.transform.Find("tf_sidechain/R_sidechain");
			// Get CAlpha position
			Transform CA_tf = residue_cs.calpha_pf.transform;

			// Place and orient CBeta
			_CB.transform.position = CB_tf.position;
			_CB.transform.LookAt(CA_tf.position);
			AddConfigJointBond(_CB, residue_cs.calpha_pf);
		}
		{
			_CG1.transform.position = _CB.transform.Find("H_3").position;
			_CG1.transform.LookAt(_CB.transform.position);
			AddConfigJointBond(_CG1, _CB);
		}
		{
			_CG2.transform.position = _CB.transform.Find("H_2").position;
			_CG2.transform.LookAt(_CB.transform.position);
			AddConfigJointBond(_CG2, _CB);
		}
		{
			_CD1.transform.position = _CG1.transform.Find("H_3").position;
			_CD1.transform.LookAt(_CG1.transform.position);
			AddConfigJointBond(_CD1, _CG1);
		}

		_CB.GetComponent<Csp3>().ConvertToCH1();

		_CG1.GetComponent<Csp3>().ConvertToCH2();

		_CG2.GetComponent<Csp3>().ConvertToCH3();
		_CD1.GetComponent<Csp3>().ConvertToCH3();

	}

	void build_MET(Residue residue_cs)
	{
		sideChainLength = 4;
		for (int i = 0; i < sideChainLength; i++)
		{
			residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
		}

		GameObject _CB = residue_cs.sideChainList[0];
		GameObject _CG = residue_cs.sideChainList[1];
		GameObject _SD = residue_cs.sideChainList[2];
		GameObject _CE = residue_cs.sideChainList[3];

		_CB.name = "CB";
		_CG.name = "CG";
		_SD.name = "SD";
		_CE.name = "CE";

		{
			// Get CBeta position => R group
			Transform CB_tf = residue_cs.calpha_pf.transform.Find("tf_sidechain/R_sidechain");
			// Get CAlpha position
			Transform CA_tf = residue_cs.calpha_pf.transform;

			// Place and orient CBeta
			_CB.transform.position = CB_tf.position;
			_CB.transform.LookAt(CA_tf.position);
			AddConfigJointBond(_CB, residue_cs.calpha_pf);
		}
		{
			_CG.transform.position = _CB.transform.Find("H_3").position;
			_CG.transform.LookAt(_CB.transform.position);
			AddConfigJointBond(_CG, _CB);
		}
		{
			_SD.transform.position = _CG.transform.Find("H_3").position;
			_SD.transform.LookAt(_CG.transform.position);
			AddConfigJointBond(_SD, _CG);
		}
		{
			_CE.transform.position = _SD.transform.Find("H_3").position;
			_CE.transform.LookAt(_SD.transform.position);
			AddConfigJointBond(_CE, _SD);
		}

		_CB.GetComponent<Csp3>().ConvertToCH2();
		_CG.GetComponent<Csp3>().ConvertToCH2();
		_SD.GetComponent<Csp3>().ConvertToS();
		_CE.GetComponent<Csp3>().ConvertToCH3();
	}

	void build_CYS(Residue residue_cs)
	{
		sideChainLength = 2;
		for (int i = 0; i < sideChainLength; i++)
		{
			residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
		}
		GameObject _CB = residue_cs.sideChainList[0];
		GameObject _SG = residue_cs.sideChainList[1];

		_CB.name = "CB";
		_SG.name = "SG";

		{
			// Get CBeta position => R group
			Transform CB_tf = residue_cs.calpha_pf.transform.Find("tf_sidechain/R_sidechain");
			// Get CAlpha position
			Transform CA_tf = residue_cs.calpha_pf.transform;

			// Place and orient CBeta
			_CB.transform.position = CB_tf.position;
			_CB.transform.LookAt(CA_tf.position);
			AddConfigJointBond(_CB, residue_cs.calpha_pf);
		}
		{
			_SG.transform.position = _CB.transform.Find("H_3").position;
			_SG.transform.LookAt(_CB.transform.position);
			AddConfigJointBond(_SG, _CB);
		}

		_CB.GetComponent<Csp3>().ConvertToCH2();
		_SG.GetComponent<Csp3>().ConvertToSH();

	}

	void build_SER(Residue residue_cs)
	{
		sideChainLength = 2;
		for (int i = 0; i < sideChainLength; i++)
		{
			residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
		}
		GameObject _CB = residue_cs.sideChainList[0];
		GameObject _OG = residue_cs.sideChainList[1];

		_CB.name = "CB";
		_OG.name = "OG";

		{
			// Get CBeta position => R group
			Transform CB_tf = residue_cs.calpha_pf.transform.Find("tf_sidechain/R_sidechain");
			// Get CAlpha position
			Transform CA_tf = residue_cs.calpha_pf.transform;

			// Place and orient CBeta
			_CB.transform.position = CB_tf.position;
			_CB.transform.LookAt(CA_tf.position);
			AddConfigJointBond(_CB, residue_cs.calpha_pf);
		}
		{
			_OG.transform.position = _CB.transform.Find("H_3").position;
			_OG.transform.LookAt(_CB.transform.position);
			AddConfigJointBond(_OG, _CB);
		}

		_CB.GetComponent<Csp3>().ConvertToCH2();
		_OG.GetComponent<Csp3>().ConvertToOH();

	}

	void build_THR(Residue residue_cs)
	{
		sideChainLength = 3;
		for (int i = 0; i < sideChainLength; i++)
		{
			residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
		}

		// CB is chiral - need to check DGD - checked!

		GameObject _CB = residue_cs.sideChainList[0];
		GameObject _CG = residue_cs.sideChainList[1];
		GameObject _OG = residue_cs.sideChainList[2];

		_CB.name = "CB";
		_CG.name = "CG";
		_OG.name = "OG";

		{
			// Get CBeta position => R group
			Transform CB_tf = residue_cs.calpha_pf.transform.Find("tf_sidechain/R_sidechain");
			// Get CAlpha position
			Transform CA_tf = residue_cs.calpha_pf.transform;

			// Place and orient CBeta
			_CB.transform.position = CB_tf.position;
			_CB.transform.LookAt(CA_tf.position);
			AddConfigJointBond(_CB, residue_cs.calpha_pf);
		}
		{
			_CG.transform.position = _CB.transform.Find("H_2").position;
			_CG.transform.LookAt(_CB.transform.position);
			AddConfigJointBond(_CG, _CB);
		}
		{
			_OG.transform.position = _CB.transform.Find("H_3").position;
			_OG.transform.LookAt(_CB.transform.position);
			AddConfigJointBond(_OG, _CB);
		}

		_CB.GetComponent<Csp3>().ConvertToCH1();
		_CG.GetComponent<Csp3>().ConvertToCH3();
		_OG.GetComponent<Csp3>().ConvertToOH();
	}

	void build_LYS(Residue residue_cs)
	{
		sideChainLength = 5;
		for (int i = 0; i < sideChainLength; i++)
		{
			residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
		}

		GameObject _CB = residue_cs.sideChainList[0];
		GameObject _CG = residue_cs.sideChainList[1];
		GameObject _CD = residue_cs.sideChainList[2];
		GameObject _CE = residue_cs.sideChainList[3];
		GameObject _NF = residue_cs.sideChainList[4];

		_CB.name = "CB";
		_CG.name = "CG";
		_CD.name = "CD";
		_CE.name = "CE";
		_NF.name = "NF";

		{
			// Get CBeta position => R group
			Transform CB_tf = residue_cs.calpha_pf.transform.Find("tf_sidechain/R_sidechain");
			// Get CAlpha position
			Transform CA_tf = residue_cs.calpha_pf.transform;

			// Place and orient CBeta
			_CB.transform.position = CB_tf.position;
			_CB.transform.LookAt(CA_tf.position);
			AddConfigJointBond(_CB, residue_cs.calpha_pf);
		}
		{
			_CG.transform.position = _CB.transform.Find("H_3").position;
			_CG.transform.LookAt(_CB.transform.position);
			AddConfigJointBond(_CG, _CB);
		}
		{
			_CD.transform.position = _CG.transform.Find("H_3").position;
			_CD.transform.LookAt(_CG.transform.position);
			AddConfigJointBond(_CD, _CG);
		}
		{
			_CE.transform.position = _CD.transform.Find("H_3").position;
			_CE.transform.LookAt(_CD.transform.position);
			AddConfigJointBond(_CE, _CD);
		}
		{
			_NF.transform.position = _CE.transform.Find("H_3").position;
			_NF.transform.LookAt(_CE.transform.position);
			AddConfigJointBond(_NF, _CE);
		}

		_CB.GetComponent<Csp3>().ConvertToCH2();
		_CG.GetComponent<Csp3>().ConvertToCH2();
		_CD.GetComponent<Csp3>().ConvertToCH2();
		_CE.GetComponent<Csp3>().ConvertToCH2();
		_NF.GetComponent<Csp3>().ConvertToNH3();
	}

	void build_ASP(Residue residue_cs)
	{
		sideChainLength = 2;
		for (int i = 0; i < sideChainLength; i++)
		{
			if (i == 0)
			{
				residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
			}
			if (i == 1)
			{
				Csp3_pf.GetComponent<Csp3>().atomType = "sp2";
				residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
			}
		}
		GameObject _CB = residue_cs.sideChainList[0];
		GameObject _CG = residue_cs.sideChainList[1];

		_CB.name = "CB";
		_CG.name = "CG";

		{
			// Get CBeta position => R group
			Transform CB_tf = residue_cs.calpha_pf.transform.Find("tf_sidechain/R_sidechain");
			// Get CAlpha position
			Transform CA_tf = residue_cs.calpha_pf.transform;

			// Place and orient CBeta
			_CB.transform.position = CB_tf.position;
			_CB.transform.LookAt(CA_tf.position);
			AddConfigJointBond(_CB, residue_cs.calpha_pf);
		}
		{
			_CG.transform.position = _CB.transform.Find("H_3").position;
			_CG.transform.LookAt(_CB.transform.position);
			AddConfigJointBond(_CG, _CB);
		}

		_CB.GetComponent<Csp3>().ConvertToCH2();
		_CG.GetComponent<Csp3>().ConvertSp2ToCOO();

	}

	void build_GLU(Residue residue_cs)
	{
		sideChainLength = 3;
		for (int i = 0; i < sideChainLength; i++)
		{
			if (i == 0 || i == 1)
			{
				residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
			}
			if (i == 2)
			{
				Csp3_pf.GetComponent<Csp3>().atomType = "sp2";
				residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
			}
		}
		GameObject _CB = residue_cs.sideChainList[0];
		GameObject _CG = residue_cs.sideChainList[1];
		GameObject _CD = residue_cs.sideChainList[2];

		_CB.name = "CB";
		_CG.name = "CG";
		_CD.name = "CD";

		{
			// Get CBeta position => R group
			Transform CB_tf = residue_cs.calpha_pf.transform.Find("tf_sidechain/R_sidechain");
			// Get CAlpha position
			Transform CA_tf = residue_cs.calpha_pf.transform;

			// Place and orient CBeta
			_CB.transform.position = CB_tf.position;
			_CB.transform.LookAt(CA_tf.position);
			AddConfigJointBond(_CB, residue_cs.calpha_pf);
		}
		{
			_CG.transform.position = _CB.transform.Find("H_3").position;
			_CG.transform.LookAt(_CB.transform.position);
			AddConfigJointBond(_CG, _CB);
		}
		{
			_CD.transform.position = _CG.transform.Find("H_3").position;
			_CD.transform.LookAt(_CG.transform.position);
			AddConfigJointBond(_CD, _CG);
		}

		_CB.GetComponent<Csp3>().ConvertToCH2();
		_CG.GetComponent<Csp3>().ConvertToCH2();
		_CD.GetComponent<Csp3>().ConvertSp2ToCOO();

	}

	void build_TEST(Residue residue_cs)
	{
		sideChainLength = 1;
		for (int i = 0; i < sideChainLength; i++)
		{
			Csp3_pf.GetComponent<Csp3>().atomType = "none"; // dev:
			residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
		}
		GameObject _CB = residue_cs.sideChainList[0];
		_CB.name = "CB";

		{
			// Get CBeta position => R group
			Transform CB_tf = residue_cs.calpha_pf.transform.Find("tf_sidechain/R_sidechain");
			// Get CAlpha position
			Transform CA_tf = residue_cs.calpha_pf.transform;

			// Place and orient CBeta
			_CB.transform.position = CB_tf.position;
			_CB.transform.LookAt(CA_tf.position);
			AddConfigJointBond(_CB, residue_cs.calpha_pf);
		}
		_CB.GetComponent<Csp3>().ConvertToCH3();
	}

	void build_DEV(Residue residue_cs)
	{
		sideChainLength = 1;
		for (int i = 0; i < sideChainLength; i++)
		{
			Csp3_pf.GetComponent<Csp3>().atomType = "sp2";
			residue_cs.sideChainList.Add(Instantiate(Csp3_pf, transform.position + (transform.right * i * 0.6f), Quaternion.identity, residue_cs.sidechain.transform));
		}
		GameObject _CB = residue_cs.sideChainList[0];
		_CB.name = "CB";

		//{
		//	// Get CBeta position => R group
		//	Transform CB_tf = residue_cs.calpha_pf.transform.Find("tf_sidechain/R_sidechain");
		//	// Get CAlpha position
		//	Transform CA_tf = residue_cs.calpha_pf.transform;

		//	// Place and orient CBeta
		//	_CB.transform.position = CB_tf.position;
		//	_CB.transform.LookAt(CA_tf.position);
		//	AddConfigJointBond(_CB, residue_cs.calpha_pf);
		//}
		//_CB.GetComponent<Csp3>().ConvertToCH3();
	}

	void AddConfigJointBond(GameObject go1, GameObject g02)
	{
		ConfigurableJoint cj = go1.AddComponent(typeof(ConfigurableJoint)) as ConfigurableJoint;
		cj.connectedBody = g02.GetComponent<Rigidbody>();

		// NOTE
		// in PolyPepBuilder.cs anchor and connected anchor are inverted
		// incorrect - but works because cj is used only for rotation
		// and rot direction is accounted for in code


		// orient config joint along bond axis (z = forward)
		// => Xrot for joint is along this axis
		cj.axis = Vector3.forward;

		// can use autoconfigure because geometry has been set up correctly ?
		cj.autoConfigureConnectedAnchor = true;

		cj.xMotion = ConfigurableJointMotion.Locked;
		cj.yMotion = ConfigurableJointMotion.Locked;
		cj.zMotion = ConfigurableJointMotion.Locked;

		cj.angularXMotion = ConfigurableJointMotion.Free;
		cj.angularYMotion = ConfigurableJointMotion.Locked;
		cj.angularZMotion = ConfigurableJointMotion.Locked;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(posA, 0.04f);
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(posB, 0.04f);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(posC, 0.2f);
		//if (lastHit)
		//{
		//	Gizmos.color = Color.black;
		//	Gizmos.DrawWireSphere(myHitPos, 0.04f);
		//}

	}

	// Update is called once per frame
	void Update () {

	}
}
