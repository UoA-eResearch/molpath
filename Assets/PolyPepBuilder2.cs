using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolyPepBuilder2 : MonoBehaviour {

	public int size;

	public Material hydrogenMaterial;
	public Material carbonMaterial;

	private List<GameObject> backbones = new List<GameObject>();

	private void Start() {
		Vector3 startPos = this.transform.position;
		BuildMolecule(startPos);
	}

	private void BuildMolecule(Vector3 startPos) {
		//offset
		Vector3 offset = new Vector3(0, 0, 3.0f);

		// first iteration (create all the backbones.)
		for (int i = 0; i < size; i++)
		{
			Vector3 spawnPos = startPos + offset;
			backbones.Add(MakeAtom(spawnPos));
		}

		// second iteration - join the backbones
		GameObject prevBackbone = null;
		foreach (var backbone in backbones)
		{
			if (prevBackbone == null) {
				prevBackbone = backbone;
				continue;
			} else {
				MakeJoint(prevBackbone, backbone, new Vector3(0, 0, 3.0f));
				MakeSpring(prevBackbone, backbone);
				prevBackbone = backbone;
			}
		}

		// third iteration - add child molecules
		foreach (var backbone in backbones)
		{
			for (int i = 0; i < 2; i++)
			{
				
				if (i%2==0) {
					Vector3 spawnPos = backbone.transform.position;					
				} else {
					Vector3 spawnPos = backbone.transform.position;					
				}
				
			}
		}
	}

	private GameObject MakeAtom(Vector3 spawnPos) {
		GameObject atom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		float scale = 0.1f;
		atom.transform.localScale = new Vector3(scale, scale, scale);
		atom.transform.position = spawnPos;
		var rb = atom.AddComponent<Rigidbody>();
		rb.useGravity = false;
		rb.drag = 0.5f;
		atom.GetComponent<Renderer>().sharedMaterial = carbonMaterial;
		return atom;
	}

	private ConfigurableJoint MakeJoint(GameObject go1, GameObject go2, Vector3 anchor) {
		var cj = go1.AddComponent<ConfigurableJoint>();
		cj.connectedBody = go2.GetComponent<Rigidbody>();

		cj.autoConfigureConnectedAnchor = false;
		cj.xMotion = ConfigurableJointMotion.Locked;
		cj.yMotion = ConfigurableJointMotion.Locked;
		cj.zMotion = ConfigurableJointMotion.Locked;

		cj.angularXMotion = ConfigurableJointMotion.Free;
		cj.angularYMotion = ConfigurableJointMotion.Locked;
		cj.angularZMotion = ConfigurableJointMotion.Locked;

		cj.anchor = anchor;
		return cj;
	}

	private SpringJoint MakeSpring(GameObject go1, GameObject go2) {
		var sj = go1.AddComponent<SpringJoint>();
		sj.connectedBody = go2.GetComponent<Rigidbody>();
		sj.anchor = new Vector3(0, 0, 3.0f);
		sj.spring = 10.0f;
		return sj;
	}

	private void FixedUpdate() {
		if (backbones.Count == size) {
		}
	}
}