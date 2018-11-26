﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.EventSystems;

namespace ControllerSelection
{
    public class XRDeviceManager : MonoBehaviour
    {
        [Header("Oculus References")]
        public OVRPlayerController ovrPlayerController;
        public GameObject OvrPlayerGo;
        public Camera OvrPlayerCamera;

        [Header("Vive References")]
        public GameObject vivePlayerGo;
        public Player vivePlayer;
        public Camera vivePlayerCamera;
        public GameObject teleporting;
        public GameObject teleportArea;

        [Header("UI Transform settings in when attached to hand")]
        public GameObject uiContainer;
        private int activeMenuIndex = 0;
        public List<GameObject> menus;

        [Header("Event Systems")]
        public GameObject OvrEventSystem;
        public GameObject ViveEventSystem;

        public bool usingOculus;
        public bool usingVive;

        public bool DebugOculusAsVive = false;

        public static XRDeviceManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<XRDeviceManager>();
                }
                return _instance;
            }
        }
        private static XRDeviceManager _instance;

        void Awake()
        {
            // programmatic fallback for if object references not set in editor.
            // oculus and vive players.
            if (!ovrPlayerController)
            {
                // oculusPlayer = GameObject.Find("OVRPlayerController");
                ovrPlayerController = FindObjectOfType<OVRPlayerController>();
            }

            // setting up vive references
            if (Player.instance)
            {
                vivePlayer = Player.instance;
                vivePlayerGo = Player.instance.gameObject;
                vivePlayerCamera = vivePlayer.hmdTransform.GetComponent<Camera>();
            }

            // Handling UI 
            if (uiContainer == null)
            {
                uiContainer = GameObject.Find("UI_container");
            }

            if (UnityEngine.XR.XRDevice.model.Contains("Vive") && !DebugOculusAsVive)
            {
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

            // hacking way of adding a no-menu option for menu cycling.
            menus.Add(uiContainer);
            menus.Add(null);

            SetUpTeleporting();
        }

        private void ViveSceneSetup()
        {
            OvrEventSystem.SetActive(false);
            ViveEventSystem.SetActive(true);

            ovrPlayerController.gameObject.SetActive(false);
            vivePlayerGo.SetActive(true);

            SetUIToHandPosition();

            // currently all the canvases are set to oculus transforms/tracking space etc.
            SetAllCanvasEventCameras(vivePlayerCamera);

            usingVive = true;
            usingOculus = false;
        }

        private void OculusSceneSetup()
        {
            OvrEventSystem.SetActive(true);
            ViveEventSystem.SetActive(false);

            vivePlayerGo.SetActive(false);
            ovrPlayerController.gameObject.SetActive(true);

            SetUIToWorldPosition();
            // accessing ovr camera rig left/right eye camera gets centre eye camera by default.
            SetAllCanvasEventCameras(OvrPlayerCamera);

            teleporting.SetActive(false);

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
                teleporting.SetActive(true);
                teleportArea.SetActive(true);
            }

            if (usingOculus)
            {
                teleportArea.SetActive(false);
                teleporting.SetActive(false);
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
            if (vivePlayerGo)
            {
                Player player = vivePlayerGo.GetComponent<Player>();
                uiContainer.transform.parent = player.hands[1].transform;
                uiContainer.transform.localPosition = new Vector3(0, 0.1f, 0.2f);
                uiContainer.transform.localRotation = Quaternion.Euler(60, 0, 0);
                uiContainer.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            }
        }

        private void SetAllCanvasEventCameras(Camera camera)
        {
            Canvas[] canvases = uiContainer.GetComponentsInChildren<Canvas>();
            foreach (var canvas in canvases)
            {
                canvas.worldCamera = camera;
            }
        }

        private GameObject CycleMenu()
        {
            // switch off current menu
            activeMenuIndex = activeMenuIndex % menus.Count;
            if (menus[activeMenuIndex] != null)
            {
                menus[activeMenuIndex].SetActive(false);
            }

            // update index and active new menu
            activeMenuIndex++;
            activeMenuIndex %= menus.Count;
            if (menus[activeMenuIndex] != null)
            {
                menus[activeMenuIndex].SetActive(true);
                return menus[activeMenuIndex];
            }
            return null;
        }

        private void SwapMenuHand(GameObject menu, Hand hand)
        {
            if (!menu)
            {
                return;
            }
            if (menu.transform.parent && menu.transform.parent != hand.transform)
            {
                return;
            }
            var positionOffset = menu.transform.localPosition;
            menu.transform.parent = hand.transform;
            menu.transform.localPosition = positionOffset;
        }

        private void UpdateMenuPosition()
        {
            foreach (Hand hand in vivePlayer.hands)
            {
                if (hand.controller != null)
                {
                    if (hand.controller.GetPressDown(EVRButtonId.k_EButton_ApplicationMenu))
                    {
                        GameObject newMenu = CycleMenu();
                        SwapMenuHand(newMenu, hand);
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            UpdateMenuPosition();
            Debug.Log(EventSystem.current.gameObject.name);
            if (DebugOculusAsVive)
            {
                DebugWithVive();
            }
        }

        public GameObject OvrRightHandAnchor;
        public GameObject OvrLeftHandAnchor;

        private void DebugWithVive()
        {
            if (vivePlayerGo.activeInHierarchy == false)
            {
                vivePlayerGo.SetActive(true);
                vivePlayerCamera.enabled = false;
                var audioListeners = vivePlayerGo.GetComponentsInChildren<AudioListener>();
                foreach (var audioListener in audioListeners)
                {
                    audioListener.enabled = false;
                }
            }
            if (ovrPlayerController.gameObject.activeInHierarchy == false)
            {
                ovrPlayerController.gameObject.SetActive(true);
                Debug.Log(Player.instance.gameObject.name);
                Debug.Log(Player.instance.rightHand.gameObject.name);
                Debug.Log(Player.instance.leftHand.gameObject.name);
                OvrRightHandAnchor.transform.position = Player.instance.rightHand.transform.position;
                OvrLeftHandAnchor.transform.position = Player.instance.leftHand.transform.position;
            }
        }
    }
}


