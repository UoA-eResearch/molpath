using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace ControllerSelection {
	public class XRDeviceManager : MonoBehaviour {
		public GameObject oculusPlayer;
		public GameObject vivePlayer;

		public GameObject UI;

		// [Header("UI Transform settings in world")]
		// public Vector3 uiWorldPosition;
		// public Vector3 uiWorldRotation;
		// public Vector3 uiWorldScale;

		// if UI is supposed to be some place other than the origin, uncomment the settings above and edit them in editor.

		[Header("UI Transform settings in when attached to hand")]
		public Vector3 uiHandPosition;
		public Vector3 uiHandRotation;
		public Vector3 uiHandScale;

		void Awake() {
			// programmatic fallback for if object references not set in editor.
			// oculus and vive players.
			if (oculusPlayer != null) {
				oculusPlayer = GameObject.Find("OVRPlayerController");
			}
			if (vivePlayer != null) {
				vivePlayer = GameObject.Find("VivePlayer");
			}

			// Handling UI 
			if (UI == null) {
				UI = GameObject.Find("UI");
			}
			if (vivePlayer == null) {
				SetUIToWorldPosition();
			} else {
				SetUIToHandPosition();
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

			if (UI == null) {
				UI = GameObject.Find("UI");
			}
		}

		private void SetUIToWorldPosition() {
			UI.transform.parent = null;
			UI.transform.position = Vector3.zero;
			UI.transform.rotation = Quaternion.Euler(Vector3.zero);
			UI.transform.localScale = Vector3.one;
		}

		private void SetUIToHandPosition() {
			if (vivePlayer) {
				Player player = vivePlayer.GetComponent<Player>();
				UI.transform.parent = player.hands[0].transform;
				UI.transform.localPosition = uiHandPosition;
				UI.transform.localRotation = Quaternion.Euler(uiHandRotation);
				UI.transform.localScale = uiHandScale;
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


