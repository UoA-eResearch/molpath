﻿/************************************************************************************
Copyright   :   Copyright 2017-Present Oculus VR, LLC. All Rights reserved.
Licensed under the Oculus VR Rift SDK License Version 3.2 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.
You may obtain a copy of the License at
http://www.oculusvr.com/licenses/LICENSE-3.2
Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
************************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ControllerSelection
{
    public class OVRRawRaycaster : MonoBehaviour
    {
        [System.Serializable]
        public class HoverCallback : UnityEvent<Transform> { }
        [System.Serializable]
        public class SelectionCallback : UnityEvent<Transform, Ray> { }
        [System.Serializable]
        public class SelectionCallbackAxis : UnityEvent<Transform, Ray, float> { }

        [Header("(Optional) Tracking space")]
        [Tooltip("Tracking space of the OVRCameraRig.\nIf tracking space is not set, the scene will be searched.\nThis search is expensive.")]
        public Transform trackingSpace = null;


        [Header("Selection")]
        [Tooltip("Primary selection button")]
        public OVRInput.Button primaryButton = OVRInput.Button.PrimaryIndexTrigger;
        [Tooltip("Secondary selection button")]
        public OVRInput.Button secondaryButton = OVRInput.Button.PrimaryTouchpad;
        [Tooltip("A selection button")]
        public OVRInput.Button aButton = OVRInput.Button.One;
        [Tooltip("B selection button")]
        public OVRInput.Button bButton = OVRInput.Button.Two;
        [Tooltip("Layers to exclude from raycast")]
        public LayerMask excludeLayers;
        [Tooltip("Maximum raycast distance")]
        public float raycastDistance = 500;

        public RawInteraction myRawInteraction;

        public OVRPointerVisualizer myOVRPointerVisualizer;

        [Header("Hover Callbacks")]
        public OVRRawRaycaster.HoverCallback onHoverEnter;
        public OVRRawRaycaster.HoverCallback onHoverExit;
        public OVRRawRaycaster.HoverCallback onHover;

        public OVRRawRaycaster.HoverCallback onHoverADown;
        public OVRRawRaycaster.HoverCallback onHoverBDown;

        [Header("Selection Callbacks")]
        public OVRRawRaycaster.SelectionCallback onPrimarySelect;
        public OVRRawRaycaster.SelectionCallback onSecondarySelect;

        public OVRRawRaycaster.SelectionCallbackAxis onPrimarySelectDownAxis;
        public OVRRawRaycaster.SelectionCallbackAxis onSecondarySelectDownAxis;

        //protected Ray pointer;
        public Transform lastHit = null;
        public Transform primaryDown = null;
        public Transform secondaryDown = null;
        public Transform aDown = null;
        public Transform bDown = null;

        public Vector3 myHitPos;

        public Transform remoteGrab = null;
        public float remoteGrabDistance;
        //public Vector3 remoteGrabOffset;

        private Vector3 remoteGrabStartPos = new Vector3(0f, 0f, 0f);
        private Vector3 remoteGrabTargetPos = new Vector3(0f, 0f, 0f);
        private Quaternion remoteGrabObjectStartQ = Quaternion.identity;
        private Quaternion remoteGrabObjectTargetQ = Quaternion.identity;
        private Quaternion remoteGrabControllerStartQ = Quaternion.identity;

        private Transform centreEyeAnchor;

        private bool tractorBeaming = false;
        private int tractorTime = 0;
        private int tractorDelay = 3; // frames before tractorbeam begins
        private float tractorLerp = 0.01f;
        private float tractorAxisInputFiltered = 0.0f;

        private Ray prevPointer;
        private float remoteGrabPoke;
        private int remoteGrabTime = 0;
        private float approxMovingAvgPoke;

        //[HideInInspector]
        public OVRInput.Controller activeController = OVRInput.Controller.None;

        void Awake()
        {
            if (trackingSpace == null)
            {
                Debug.LogWarning("OVRRawRaycaster did not have a tracking space set. Looking for one");
                trackingSpace = OVRInputHelpers.FindTrackingSpace();

            }
            centreEyeAnchor = trackingSpace.transform.Find("CenterEyeAnchor");
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (trackingSpace == null)
            {
                Debug.LogWarning("OVRRawRaycaster did not have a tracking space set. Looking for one");
                trackingSpace = OVRInputHelpers.FindTrackingSpace();
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(remoteGrabStartPos, 0.04f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(remoteGrabTargetPos, 0.04f);
            if (lastHit)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(myHitPos, 0.04f);
            }

        }

        void Update()
        {
            activeController = OVRInputHelpers.GetControllerForButton(OVRInput.Button.PrimaryIndexTrigger, activeController);
            Ray pointer = OVRInputHelpers.GetSelectionRay(activeController, trackingSpace);

            RaycastHit hit; // Was anything hit?
            if (Physics.Raycast(pointer, out hit, raycastDistance, ~excludeLayers))
            {

                myHitPos = hit.point;
                myOVRPointerVisualizer.rayDrawDistance = hit.distance;
                //Debug.Log(hit.distance);


                if (lastHit != null && lastHit != hit.transform)
                {
                    if (onHoverExit != null)
                    {
                        onHoverExit.Invoke(lastHit);
                    }
                    lastHit = null;
                }

                if (lastHit == null)
                {
                    if (onHoverEnter != null)
                    {
                        onHoverEnter.Invoke(hit.transform);
                    }
                }

                if (onHover != null)
                {
                    onHover.Invoke(hit.transform);
                }

                lastHit = hit.transform;

                // Handle selection callbacks. An object is selected if the button selecting it was
                // pressed AND released while hovering over the object.

                if (activeController != OVRInput.Controller.None)
                {
                    if (OVRInput.GetDown(secondaryButton, activeController))
                    {
                        secondaryDown = lastHit;
                        //Debug.Log("1");
                    }
                    else if (OVRInput.GetUp(secondaryButton, activeController))
                    {
                        if (secondaryDown != null && secondaryDown == lastHit)
                        {
                            if (onSecondarySelect != null)
                            {
                                onSecondarySelect.Invoke(secondaryDown, pointer);
                                //Debug.Log("2");
                            }
                        }
                    }
                    if (!OVRInput.Get(secondaryButton, activeController))
                    {
                        secondaryDown = null;
                        //Debug.Log("3");
                    }

                    if (OVRInput.GetDown(primaryButton, activeController))
                    {
                        primaryDown = lastHit;
                        //Debug.Log("4");
                    }
                    else if (OVRInput.GetUp(primaryButton, activeController))
                    {
                        if (primaryDown != null && primaryDown == lastHit)
                        {
                            if (onPrimarySelect != null)
                            {
                                onPrimarySelect.Invoke(primaryDown, pointer);
                                //Debug.Log("5");
                            }
                        }
                    }
                    if (!OVRInput.Get(primaryButton, activeController))
                    {
                        primaryDown = null;
                        //Debug.Log("6");
                    }
                }

                if (lastHit)
                {
                    ///
                    if (OVRInput.Get(aButton, activeController))
                    {
                        aDown = lastHit;
                    }
                    else
                    {
                        aDown = null;
                    }
                    if (OVRInput.Get(bButton, activeController))
                    {
                        bDown = lastHit;
                    }
                    else
                    {
                        bDown = null;
                    }
                }



                if (aDown)
                {
                    //Debug.Log("A---->" + aDown);
                    onHoverADown.Invoke(aDown);
                }

                if (bDown)
                {
                    //Debug.Log("B---->" + bDown);
                    onHoverBDown.Invoke(bDown);
                }

                if (primaryDown && !secondaryDown)
                {
                    //Debug.Log(primaryDown);
                    //Debug.Log(axisValue);
                    if (!tractorBeaming)
                    {
                        tractorBeaming = true;
                        tractorTime = 0;
                        tractorAxisInputFiltered = 0.0f;
                    }
                    else
                    {
                        tractorTime++;
                        if (tractorTime > tractorDelay)
                        {
                            float axisValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, activeController);
                            tractorAxisInputFiltered = Mathf.Lerp(tractorAxisInputFiltered, axisValue, tractorLerp);
                            onPrimarySelectDownAxis.Invoke(primaryDown, pointer, tractorAxisInputFiltered);
                        }
                    }
                }
                else if (secondaryDown && !primaryDown)
                {
                    //Debug.Log(secondaryDown);
                    //Debug.Log(axisValue);
                    if (!tractorBeaming)
                    {
                        tractorBeaming = true;
                        tractorTime = 0;
                        tractorAxisInputFiltered = 0.0f;
                    }
                    else
                    {
                        tractorTime++;
                        if (tractorTime > tractorDelay)
                        {
                            float axisValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, activeController);
                            tractorAxisInputFiltered = Mathf.Lerp(tractorAxisInputFiltered, axisValue, tractorLerp);
                            onSecondarySelectDownAxis.Invoke(secondaryDown, pointer, tractorAxisInputFiltered);
                        }
                    }

                }
                else
                {
                    tractorBeaming = false;
                }

#if UNITY_ANDROID && !UNITY_EDITOR
            // Gaze pointer fallback
            else {
                if (Input.GetMouseButtonDown(0) ) {
                    triggerDown = lastHit;
                }
                else if (Input.GetMouseButtonUp(0) ) {
                    if (triggerDown != null && triggerDown == lastHit) {
                        if (onPrimarySelect != null) {
                            onPrimarySelect.Invoke(triggerDown);
                        }
                    }
                }
                if (!Input.GetMouseButton(0)) {
                    triggerDown = null;
                }
            }
#endif

                //REMOTE GRAB
                if (!remoteGrab)
                {
                    // remoteGrab not set - looking for candidate
                    if (lastHit)
                    {
                        if (primaryDown && secondaryDown)
                        {
                            if (lastHit == primaryDown && lastHit == secondaryDown)
                            {
                                // START remote grabbing
                                //Debug.Log(lastHit + " is candidate for remoteGrab");
                                remoteGrab = lastHit;
                                remoteGrabDistance = hit.distance;
                                //Debug.Log("   --->" + hit.distance);
                                remoteGrabStartPos = hit.point;
                                approxMovingAvgPoke = 0f;
                                remoteGrabTime = 0;

                                remoteGrabObjectStartQ = remoteGrab.gameObject.transform.rotation;
                                remoteGrabControllerStartQ = OVRInput.GetLocalControllerRotation(activeController);

                                BackboneUnit bu = (remoteGrab.gameObject.GetComponent("BackboneUnit") as BackboneUnit);
                                if (bu != null)
                                {
                                    bu.SetRemoteGrabSelect(true);
                                    //bu.remoteGrabSelectOn = true;
                                    //bu.UpdateRenderMode();
                                }

                                //Rigidbody hitRigidBody = lastHit.gameObject.GetComponent<Rigidbody>();
                                //remoteGrabOffset = hitRigidBody.position - hit.point;


                            }
                        }
                    }
                }
                else
                {

                }
            }
            // Nothing was hit, handle exit callback
            else
            {
                myOVRPointerVisualizer.rayDrawDistance = 10.0f;

                if (lastHit != null)
                {
                    if (onHoverExit != null)
                    {
                        onHoverExit.Invoke(lastHit);
                    }
                    lastHit = null;
                }
            }


            //REMOTE GRAB UPDATE (outside of hit test)
            if (remoteGrab)
            {
                if ((OVRInput.Get(primaryButton, activeController)) && (OVRInput.Get(secondaryButton, activeController)))
                {
                    // still remote grabbing


                    // poke (detecting sustained controller movement along pointer axis)
                    remoteGrabTime++;

                    Vector3 deltaPointer = Vector3.Project((pointer.origin - prevPointer.origin), pointer.direction);

                    float poke = Vector3.Dot(deltaPointer, pointer.direction);

                    approxMovingAvgPoke -= approxMovingAvgPoke / 5;
                    approxMovingAvgPoke += poke / 5;

                    //if (Mathf.Abs(poke) > 0.001f)
                    //{
                    //	Debug.Log("poke = " + poke);
                    //}


                    if (remoteGrabTime > 5)
                    {
                        // scale remoteGrabDistance 
                        remoteGrabDistance *= (1.0f + (poke * 5.0f));
                    }

                    prevPointer = pointer;

                    remoteGrabTargetPos = (pointer.origin + (remoteGrabDistance * pointer.direction));

                    // tractor beam to destination (mostly tangential to pointer axis (pitch / yaw movement)
                    myRawInteraction.RemoteGrabInteraction(primaryDown, remoteGrabTargetPos);


                    BackboneUnit bu = (remoteGrab.gameObject.GetComponent("BackboneUnit") as BackboneUnit);
                    if (bu != null)
                    {
                        //add ROLL - torque from wrist twist
                        Quaternion remoteGrabControllerCurrentQ = OVRInput.GetLocalControllerRotation(activeController);
                        Quaternion remoteGrabControllerDeltaQ = remoteGrabControllerCurrentQ * Quaternion.Inverse(remoteGrabControllerStartQ);
                        remoteGrabObjectTargetQ = remoteGrabControllerDeltaQ * remoteGrabObjectStartQ;

                        //remoteGrab.gameObject.transform.rotation = Quaternion.Slerp(remoteGrab.gameObject.transform.rotation, remoteGrabObjectTargetQ, 0.1f);


                        Vector3 vInit = remoteGrabControllerStartQ.eulerAngles;
                        Vector3 vDelta = remoteGrabControllerDeltaQ.eulerAngles;
                        Vector3 vCurrent = remoteGrabControllerCurrentQ.eulerAngles; // Quaternion.ToEulerAngles(q); 

                        //Debug.Log(vInit.z + " -> " + vCurrent.z + " d = " + vDelta.z);

                        float zRot = vDelta.z;
                        if (zRot > 180.0f)
                        {
                            zRot -= 360.0f;
                        }

                        //Debug.Log(zRot);

                        if (Mathf.Abs(zRot) > 15.0f) // threshold 
                        {
                            remoteGrab.gameObject.GetComponent<Rigidbody>().AddTorque(pointer.direction * zRot * 2.5f);
                        }
                    }
                    else
                    {
                        // not bu - UI - make the 'front' face the pointer
                        // flipped because go was initially set up with z facing away

                        //Use pointer position
                        //Vector3 lookAwayPos = remoteGrab.gameObject.transform.position + pointer.direction;

                        //Use HMD (possibly better - maybe a bit queasy)
                        Vector3 lookAwayPos = remoteGrab.gameObject.transform.position + centreEyeAnchor.forward;
                        remoteGrab.gameObject.transform.LookAt(lookAwayPos, Vector3.up);
                    }
                }
                else
                {
                    //END remote grabbing
                    BackboneUnit bu = (remoteGrab.gameObject.GetComponent("BackboneUnit") as BackboneUnit);
                    if (bu != null)
                    {
                        bu.SetRemoteGrabSelect(false);
                        //bu.remoteGrabSelectOn = false;
                        //bu.UpdateRenderMode();
                    }
                    remoteGrab = null;
                }
            }
        }
    }
}
// Useful ref:
// https://developer.oculus.com/documentation/unity/latest/concepts/unity-integration-ovrinput/