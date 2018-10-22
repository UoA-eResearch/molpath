using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolyPepBuilder2 : MonoBehaviour {

	public int spineSize;

	public List<string> patterns = new List<string>();

	// Use this for initialization
	void Start () {
		BuildChain();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void BuildChain() {

		if (patterns.Count == 0) {
			patterns.Add("Carbonyl");
			patterns.Add("Calpha");
			patterns.Add("Amide");
		}

		foreach (var pattern in patterns) {
				Debug.Log(pattern);
				switch (pattern) {
					case "Carbonyl":
						BuildCarbonyl();
						break;
					case "Calpha":
						BuildCalpha();
						break;
					case "Amide":
						BuildAmide();
						break;
				}
		}	
	}

	private GameObject Atom(){
		GameObject atom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		// starts at 1.
		atom.transform.localScale *= 0.1f;
		Rigidbody atomRb = atom.AddComponent<Rigidbody>();
		return atom;
	}

	private GameObject SetBondAngle(GameObject atom1, GameObject atom2){
		GameObject residue = new GameObject();
		atom1.transform.parent = residue.transform;
		atom2.transform.parent = residue.transform;
		atom1.transform.position = residue.transform.position;
		var joint = atom1.AddComponent<ConfigurableJoint>();
		joint.connectedBody = atom2.GetComponent<Rigidbody>();
		joint.xMotion = ConfigurableJointMotion.Limited;
		joint.yMotion = ConfigurableJointMotion.Limited;
		joint.zMotion = ConfigurableJointMotion.Limited;
		joint.angularXMotion = ConfigurableJointMotion.Limited;
		joint.angularYMotion = ConfigurableJointMotion.Limited;
		joint.angularZMotion = ConfigurableJointMotion.Limited;
		return residue;
	}

	private void BuildCarbonyl() {
		GameObject carbon = Atom();
		// starts at 1.
		GameObject oxygen = Atom();
		SetBondAngle(carbon, oxygen);

	}

	private void BuildCalpha() {

	}

	private void BuildAmide() {

	}
}
