using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

namespace ControllerSelection
{
    public class XRDeviceManager : MonoBehaviour
    {
        public GameObject oculusPlayer;
        public GameObject vivePlayerGo;
        public Player vivePlayer;
        public Camera vivePlayerCamera;

        public GameObject UI;

        [Header("UI Transform settings in when attached to hand")]
        public Vector3 uiHandPosition;
        public Vector3 uiHandRotation;
        public Vector3 uiHandScale;

        void Awake()
        {
            // programmatic fallback for if object references not set in editor.
            // oculus and vive players.
            if (!oculusPlayer)
            {
                oculusPlayer = GameObject.Find("OVRPlayerController");
            }

            // setting up vive references
            if (!vivePlayerGo)
            {
                vivePlayerGo = GameObject.Find("VivePlayer");
            }
            if (!vivePlayer)
            {
                vivePlayer = vivePlayerGo.GetComponent<Player>();
            }
            if (!vivePlayerCamera)
            {
                vivePlayerCamera = vivePlayer.hmdTransforms[0].GetComponent<Camera>();
            }

            // Handling UI 
            if (UI == null)
            {
                UI = GameObject.Find("UI");
                // TODO: Add the additional canvases
            }

            if (UnityEngine.XR.XRDevice.model.Contains("Vive"))
            {
                ViveSceneSetup();
            }
            else if (UnityEngine.XR.XRDevice.model.Contains("Oculus"))
            {
                OculusSceneSetup();
            }
            else
            {
                Debug.Log("Using unhandled XR device.");
            }
        }

        private void ViveSceneSetup()
        {
            oculusPlayer.SetActive(false);
            vivePlayerGo.SetActive(true);

            SetUIToHandPosition();

            // currently all the canvases are set to oculus transforms/tracking space etc.
            SetAllCanvasEventCameras(vivePlayerCamera);
        }

        private void OculusSceneSetup()
        {
            vivePlayerGo.SetActive(false);
            oculusPlayer.SetActive(true);

            SetUIToWorldPosition();
        }

        private void SetUIToWorldPosition()
        {
            UI.transform.parent = null;
            UI.transform.position = new Vector3(0, 1, 0);
            UI.transform.rotation = Quaternion.Euler(Vector3.zero);
            UI.transform.localScale = Vector3.one;
        }

        private void SetUIToHandPosition()
        {
            if (vivePlayerGo)
            {
                Player player = vivePlayerGo.GetComponent<Player>();
                UI.transform.parent = player.hands[1].transform;
                UI.transform.localPosition = uiHandPosition;
                UI.transform.localRotation = Quaternion.Euler(uiHandRotation);
                UI.transform.localScale = uiHandScale;
            }
        }

        private void SetAllCanvasEventCameras(Camera camera)
        {
            Canvas[] canvases = UI.GetComponentsInChildren<Canvas>();
            foreach (var canvas in canvases)
            {
                canvas.worldCamera = camera;
            }
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


