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

		// Use this for initialization
		void Start () {

		}
		
		// Update is called once per frame
		void Update () {
			
		}
	}
}


