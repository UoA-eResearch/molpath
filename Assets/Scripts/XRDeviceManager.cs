using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace ControllerSelection
{
    public class XRDeviceManager : MonoBehaviour
    {
        public GameObject oculusPlayer;
        public GameObject vivePlayerGo;
        public Player vivePlayer;
        public Camera vivePlayerCamera;


        public GameObject uiContainer;

        [Header("UI Transform settings in when attached to hand")]

        private int activeMenuIndex = 0;
        public List<GameObject> menus;

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
            if (uiContainer == null)
            {
                uiContainer = GameObject.Find("UI_container");
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

            // hacking way of adding a no-menu option for menu cycling.
            menus.Add(uiContainer);
            menus.Add(null);
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
            var offset = menu.transform.localPosition;
            menu.transform.parent = hand.transform;
            menu.transform.localPosition = offset;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // poll for inputs from either hand
            // TODO: switch from polling to event triggers.
            foreach (Hand hand in vivePlayer.hands)
            {
                if (hand.controller != null)
                {
                    if (hand.controller.GetPressDown(EVRButtonId.k_EButton_ApplicationMenu))
                    {
                        GameObject newMenu = CycleMenu();
                        if (newMenu.transform.parent && newMenu.transform.parent != hand.transform)
                        {
                            SwapMenuHand(newMenu, hand);
                        }
                    }
                }
            }
        }
    }
}


