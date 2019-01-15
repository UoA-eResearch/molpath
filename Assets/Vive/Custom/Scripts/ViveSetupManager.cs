using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.EventSystems;
using ViveInputs;

namespace ControllerSelection
{
	public class ViveSetupManager : MonoBehaviour
    {
        [Header("Oculus References")]
        private OVRPlayerController _ovrPlayerController;
        public OVRPlayerController ovrPlayerController
        {

            get
            {
                if (_ovrPlayerController == null)
                {
                    _ovrPlayerController = FindObjectOfType<OVRPlayerController>();
                }
                return _ovrPlayerController;
            }
        }
        private GameObject _ovrPlayerGo;
        public GameObject ovrPlayerGo
        {
            get
            {
                if (!_ovrPlayerGo)
                {
                    _ovrPlayerGo = ovrPlayerController.gameObject;
                }
                return _ovrPlayerGo;
            }
        }

        public Camera _ovrPlayerCamera;
        public Camera ovrPlayerCamera
        {
            get
            {
                if (!_ovrPlayerCamera)
                {
                    _ovrPlayerCamera = FindObjectOfType<OVRCameraRig>().leftEyeCamera;
                }
                return _ovrPlayerCamera;
            }
        }
        public GameObject OvrRightHandAnchor;
        public GameObject OvrLeftHandAnchor;

        [Header("Vive References")]
        private Player _vivePlayer;
        public Player vivePlayer
        {
            get
            {
                if (!_vivePlayer)
                {
                    _vivePlayer = Player.instance;
                }
                return _vivePlayer;
            }
        }
        private Camera _vivePlayerCamera;
        public Camera vivePlayerCamera
        {
            get
            {
                if (!_vivePlayerCamera)
                {
                    _vivePlayerCamera = vivePlayer.hmdTransform.GetComponent<Camera>();
                }
                return _vivePlayerCamera;
            }
        }

        [Header("UI Transform settings in when attached to hand")]
        public GameObject uiContainer;
        private int activeMenuIndex = 0;
        public List<GameObject> menus;

        [Header("Event Systems")]
        private OVRInputModule _ovrInputModule;
        public OVRInputModule ovrInputModule
        {
            get
            {
                if (!_ovrInputModule)
                {
                    _ovrInputModule = FindObjectOfType<OVRInputModule>();
                }
                return _ovrInputModule;
            }
        }
        private ViveInputModule _viveInputModule;
        public ViveInputModule viveInputModule
        {
            get
            {
                if (!_viveInputModule)
                {
                    _viveInputModule = FindObjectOfType<ViveInputModule>();
                }
                return _viveInputModule;
            }
        }

        public bool usingOculus;
        public bool usingVive;

        public bool DebugOculusAsVive = false;

        private static ViveSetupManager _instance;
        public static ViveSetupManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSetupManager>();
                }
                return _instance;
            }
        }

        [Header("Teleporting")]
        private static Teleport _teleport;
        public static Teleport teleport
        {
            get
            {
                if (_teleport == null)
                {
                    _teleport = FindObjectOfType<Teleport>();
                }
                return _teleport;
            }
        }

        private static TeleportArea _teleportArea;
        public static TeleportArea teleportArea
        {
            get
            {
                if (_teleportArea == null)
                {
                    _teleportArea = FindObjectOfType<TeleportArea>();
                }
                return _teleportArea;
            }
        }

        public GameObject teleportAreaTarget;

		private GameObject[] uis;

		void Awake()
        {
            // Handling UI 
            // if (uiContainer == null)
            // {
            //     uiContainer = GameObject.Find("UiContainer");
            // }

			// handling ui outside the ui container.
			uis = GameObject.FindGameObjectsWithTag("UI");

            if (UnityEngine.XR.XRDevice.model.Contains("Vive") && !DebugOculusAsVive)
            {
                Debug.Log("using vive setup");
                ViveSceneSetup();
                usingVive = true;
            }
            else if (UnityEngine.XR.XRDevice.model.Contains("Oculus") || DebugOculusAsVive)
            {
                OculusSceneSetup();
            }
            else
            {
                DesktopSceneSetup();
            }
            // SetUpTeleporting();
        }

		/// <summary>
		/// Turns of OVR input module. Turns on ViveInputModule
		/// </summary>
		private void ViveSceneSetup()
        {
            ovrInputModule.gameObject.SetActive(false);
            viveInputModule.gameObject.SetActive(true);

            ovrPlayerController.gameObject.SetActive(false);
            vivePlayer.gameObject.SetActive(true);

            // SetUIToHandPosition();

            // currently all the canvases are set to oculus transforms/tracking space etc.
            ConfigureCanvasesForVive(vivePlayerCamera);

            // disable multiple audio listeners.
            vivePlayer.audioListener.GetComponent<AudioListener>().enabled = false;

            usingVive = true;
            usingOculus = false;
        }

        private void OculusSceneSetup()
        {
            ovrInputModule.gameObject.SetActive(true);
            viveInputModule.gameObject.SetActive(false);

            vivePlayer.gameObject.SetActive(false);
            ovrPlayerController.gameObject.SetActive(true);

            // Camera stuff

            // SetUIToWorldPosition();

            // accessing ovr camera rig left/right eye camera gets centre eye camera by default.
            ConfigureCanvasesForVive(ovrPlayerCamera);

            ovrPlayerController.GetComponentInChildren<AudioListener>().enabled = false;

            teleport.gameObject.SetActive(false);

            usingVive = false;
            usingOculus = true;
        }

        private void DesktopSceneSetup()
        {
            Debug.Log("Using unassigned device, defaulting to Vive scene to use the Vive 2d debugger.");
            ViveSceneSetup();
        }

        private void SetUpTeleporting()
        {
            // reference to teleporting in case it's used here.
            if (usingVive)
            {
                teleport.gameObject.SetActive(true);
                teleportArea.gameObject.SetActive(true);

                // attempt to position and scale teleport area to match floor size.
                if (!teleportAreaTarget)
                {
                    teleportAreaTarget = GameObject.Find("Floor");
                }
                if (teleportAreaTarget)
                {
                    teleportArea.transform.position = teleportAreaTarget.transform.position + new Vector3(0, 0.002f, 0);
                    teleportArea.transform.localScale = teleportAreaTarget.transform.localScale;
                }
            }
            if (usingOculus)
            {
                teleportArea.gameObject.SetActive(false);
                teleport.gameObject.SetActive(false);
            }
        }

        private void SetUIToWorldPosition()
        {
            uiContainer.transform.parent = null;
            uiContainer.transform.position = new Vector3(0, 1, 0);
            uiContainer.transform.rotation = Quaternion.Euler(Vector3.zero);
            uiContainer.transform.localScale = Vector3.one;
        }

        private void SetUIToHandPosition()
        {
			if (Player.instance)
            {  
                if (Player.instance.rightHand && uiContainer) {
					uiContainer.transform.parent = Player.instance.rightHand.transform;
					uiContainer.transform.localPosition = new Vector3(0, 0.1f, 0.2f);
					uiContainer.transform.localRotation = Quaternion.Euler(60, 0, 0);
					uiContainer.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                }
            }
        }

        private void ConfigureCanvasesForVive(Camera camera)
        {
			foreach (var ui in uis)
            {
                if (ui.GetComponent<Canvas>()) {
					ui.GetComponent<Canvas>().worldCamera = camera;
                }
            }
        }

        // private GameObject CycleMenu()
        // {
        //     // switch off current menu
        //     activeMenuIndex = activeMenuIndex % menus.Count;
        //     if (menus[activeMenuIndex] != null)
        //     {
        //         menus[activeMenuIndex].SetActive(false);
        //     }

        //     // update index and active new menu
        //     activeMenuIndex++;
        //     activeMenuIndex %= menus.Count;
        //     if (menus[activeMenuIndex] != null)
        //     {
        //         menus[activeMenuIndex].SetActive(true);
        //         return menus[activeMenuIndex];
        //     }
        //     return null;
        // }

        // private void SwapMenuHand(GameObject menu, Hand hand)
        // {
        //     if (!menu)
        //     {
        //         return;
        //     }
        //     if (menu.transform.parent && menu.transform.parent != hand.transform)
        //     {
        //         return;
        //     }
        //     var positionOffset = menu.transform.localPosition;
        //     menu.transform.parent = hand.transform;
        //     menu.transform.localPosition = positionOffset;
        // }

        // private void UpdateMenuPosition()
        // {
        //     foreach (Hand hand in vivePlayer.hands)
        //     {
        //         if (hand.controller != null)
        //         {
        //             if (hand.controller.GetPressDown(EVRButtonId.k_EButton_ApplicationMenu))
        //             {
        //                 GameObject newMenu = CycleMenu();
        //                 SwapMenuHand(newMenu, hand);
        //             }
        //         }
        //     }
        // }

        // Update is called once per frame
        void Update()
        {
            // UpdateMenuPosition();

            if (DebugOculusAsVive)
            {
                DebugWithVive();
            }
        }

        private void DebugWithVive()
        {
            if (vivePlayer.gameObject.activeInHierarchy == false)
            {
                vivePlayer.gameObject.SetActive(true);
                vivePlayerCamera.enabled = false;
                var audioListeners = vivePlayer.gameObject.GetComponentsInChildren<AudioListener>();
                foreach (var audioListener in audioListeners)
                {
                    audioListener.enabled = false;
                }
            }
            if (ovrPlayerController.gameObject.activeInHierarchy == false)
            {
                ovrPlayerController.gameObject.SetActive(true);
                // Debug.Log(Player.instance.gameObject.name);
                // Debug.Log(Player.instance.rightHand.gameObject.name);
                // Debug.Log(Player.instance.leftHand.gameObject.name);
                OvrRightHandAnchor.transform.position = Player.instance.rightHand.transform.position;
                OvrLeftHandAnchor.transform.position = Player.instance.leftHand.transform.position;
            }
        }
    }
}


