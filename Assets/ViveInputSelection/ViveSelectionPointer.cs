using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace ViveInputs
{
    public class ViveSelectionPointer : MonoBehaviour
    {

        public LineRenderer myLineRenderer;

        public float pointerDistance;

        public Hand activeHand;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Ray r = ViveInputHelpers.GetSelectionRay(activeHand.transform);
            myLineRenderer.SetPosition(0, activeHand.transform.position);
            myLineRenderer.SetPosition(1, activeHand.transform.forward * pointerDistance);
        }
    }
}

