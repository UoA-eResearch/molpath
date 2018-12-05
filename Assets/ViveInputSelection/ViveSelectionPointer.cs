using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace ViveInputs
{
    public class ViveSelectionPointer : MonoBehaviour
    {

        public LineRenderer myLineRenderer;

        public float distance;

        public Hand activeHand;

        // Use this for initialization
        void Start()
        {

        }

        private void SetPointerVisibility()
        {
            if (activeHand)
            {
                myLineRenderer.enabled = true;
            }
            else
            {
                myLineRenderer.enabled = false;
            }
        }

        private void SetPointerPosition()
        {
            if (activeHand)
            {
                Ray ray = ViveInputHelpers.GetSelectionRay(activeHand.transform);
                myLineRenderer.SetPosition(0, ray.origin);
                myLineRenderer.SetPosition(1, ray.origin + ray.direction * distance);
            }
        }

        // Update is called once per frame
        void Update()
        {
            activeHand = ViveInputHelpers.GetHandForButton(SteamVR_Controller.ButtonMask.Touchpad, activeHand);
            SetPointerPosition();
            SetPointerVisibility();
        }
    }
}

