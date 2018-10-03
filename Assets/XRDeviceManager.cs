using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ControllerSelection {
	public class XRDeviceManager : MonoBehaviour {
		public GameObject oculusPlayer;
		public GameObject vivePlayer;
		public Transform viveRayTransform;

		public OVRInputModule inputModule;
		public OVRGazePointer gazePointer;

		void Awake() {
			if (oculusPlayer != null) {
				oculusPlayer = GameObject.Find("OVRPlayerController");
			}
			if (vivePlayer != null) {
				vivePlayer = GameObject.Find("VivePlayer");
				UpdateUIReferences();
			}

			if (UnityEngine.XR.XRDevice.model.Contains("Vive")) {
				oculusPlayer.SetActive(false);
				vivePlayer.SetActive(true);
			}
			else if (UnityEngine.XR.XRDevice.model.Contains("Oculus")) {
				vivePlayer.SetActive(false);
				oculusPlayer.SetActive(true);
			} else {
				Debug.Log("Using unhandled XR device.");
			}
		}

		// TEST:
		public Canvas ui1;
		public Canvas ui2;
		void UpdateUIReferences() {
			// switch from oculus righthand references as pointer to vive controller.
			gazePointer.rayTransform = viveRayTransform;

			// update canvas world cameras.
			ui1.worldCamera = vivePlayer.GetComponentInChildren<Camera>();
			ui2.worldCamera = vivePlayer.GetComponentInChildren<Camera>();
		}
		// TODO: Also add button references so clicks work

		// Use this for initialization
		void Start () {

		}
		
		// Update is called once per frame
		void Update () {
			
		}
	}
}


