using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Valve.VR.InteractionSystem;

namespace ViveInputs
{
    public class ViveRawRaycaster : MonoBehaviour
    {
        [System.Serializable]
        public class HoverCallback : UnityEvent<Transform> { }
        [System.Serializable]
        public class SelectionCallback : UnityEvent<Transform, Ray> { }
        [System.Serializable]
        public class SelectionCallbackAxis : UnityEvent<Transform, Ray, float> { }

        [Header("Vive Selection")]
        [Tooltip("Primary selection button")]
        public ulong touchpad = SteamVR_Controller.ButtonMask.Touchpad;

        [Tooltip("Trigger button")]
        public ulong trigger = SteamVR_Controller.ButtonMask.Trigger;

        public ulong grip = SteamVR_Controller.ButtonMask.Grip;

        [Tooltip("Layers to exclude from raycast")]
        public LayerMask excludeLayers;
        [Tooltip("Maximum raycast distance")]
        public float raycastDistance = 500;

        public RawInteraction myRawInteraction;


        public ViveSelectionPointer viveSelectionPointer;


        [Header("Hover Callbacks")]
        public ViveRawRaycaster.HoverCallback onHoverEnter;
        public ViveRawRaycaster.HoverCallback onHoverExit;
        public ViveRawRaycaster.HoverCallback onHover;

        public ViveRawRaycaster.HoverCallback onHoverADown;
        public ViveRawRaycaster.HoverCallback onHoverBDown;

        [Header("Selection Callbacks")]
        public ViveRawRaycaster.SelectionCallback onPrimarySelect;
        public ViveRawRaycaster.SelectionCallback onSecondarySelect;

        public ViveRawRaycaster.SelectionCallbackAxis onPrimarySelectDownAxis;
        public ViveRawRaycaster.SelectionCallbackAxis onSecondarySelectDownAxis;

        //protected Ray pointer;
        public Transform lastHit = null;
        public Transform primaryDown = null;
        public Transform secondaryDown = null;
        public Transform aDown = null;
        public Transform bDown = null;

        public Vector3 myHitPos;

        public Transform remoteGrab = null;
        private Rigidbody remoteGrabRigidBody = null;
        private BackboneUnit remoteGrabBackboneUnit = null;
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

        // Vive variables to track
        private GameObject vivePlayerGo;
        private Player vivePlayer;
        private Hand viveLeftHand;
        private Hand viveRightHand;
        private Vector3 lastControllerPos;
        private Vector3 lastControllerRot;
        public float remoteGrabStrength = 5.0f;
        private GameObject remoteGrabDestinationGo;

        //[HideInInspector]
        public Hand activeController;

        void Awake()
        {
            if (vivePlayer == null)
            {
                vivePlayerGo = GameObject.Find("VivePlayer");

                if (vivePlayerGo)
                {
                    vivePlayer = vivePlayerGo.GetComponent<Player>();
                    if (vivePlayer)
                    {
                        viveLeftHand = vivePlayer.leftHand;
                        viveRightHand = vivePlayer.rightHand;
                    }
                }
            }
            if (remoteGrabDestinationGo == null)
            {
                // simple object acting as a target destination transform when using remote grab
                remoteGrabDestinationGo = new GameObject();
                remoteGrabDestinationGo.name = "remoteGrabDestination";
            }

            if (!activeController)
            {
                activeController = Player.instance.leftHand;
            }
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

        private void SetRemoteGrab(Vector3 newPosition, Transform newParent)
        {
            remoteGrabDestinationGo.transform.position = newPosition;
            remoteGrabDestinationGo.transform.parent = newParent.transform;

            remoteGrab = lastHit;
            remoteGrabBackboneUnit = remoteGrab.GetComponent<BackboneUnit>();
            remoteGrabRigidBody = remoteGrab.GetComponent<Rigidbody>();
            if (remoteGrabBackboneUnit)
            {
                remoteGrabBackboneUnit.SetRemoteGrabSelect(true);
            }
        }

        private void ClearRemoteGrab()
        {
            if (remoteGrab == null)
            {
                return;
            }
            if (remoteGrabBackboneUnit)
            {
                remoteGrabBackboneUnit.SetRemoteGrabSelect(false);
            }
            remoteGrab = null;
            remoteGrabRigidBody = null;
            remoteGrabBackboneUnit = null;
        }


        private void ClearLastHit()
        {
            if (!lastHit)
            {
                return;
            }
            if (onHoverExit != null)
            {
                onHoverExit.Invoke(lastHit);
            }
            lastHit = null;
        }

        private void ProcessViveInputOnTarget(RaycastHit hit)
        {
            // Vive handling
            if (remoteGrab == null)
            {
                onHoverEnter.Invoke(hit.transform);
            }

            if (lastHit != null && lastHit != hit.transform)
            {
                // dont show selection highlight if remote grabbing.
                onHoverExit.Invoke(lastHit);
            }
            lastHit = hit.transform;

            // Test remote grab stuff
            //left
            if (vivePlayer.GetHairTriggerDown(viveLeftHand))
            {
                SetRemoteGrab(hit.point, viveLeftHand.transform);
            }
            if (vivePlayer.GetHairTriggerUp(viveLeftHand))
            {
                ClearRemoteGrab();
            }

            // right
            if (Player.instance.GetHairTriggerDown(viveRightHand))
            {
                SetRemoteGrab(hit.point, viveRightHand.transform);
            }
            if (vivePlayer.GetHairTriggerUp(viveRightHand))
            {
                ClearRemoteGrab();
            }
        }


        private void OculusRemoteGrab(Ray pointer)
        {
            // // still remote grabbing
            // // poke (detecting sustained controller movement along pointer axis)
            // remoteGrabTime++;
            // Vector3 deltaPointer = Vector3.Project((pointer.origin - prevPointer.origin), pointer.direction);
            // float poke = Vector3.Dot(deltaPointer, pointer.direction);
            // approxMovingAvgPoke -= approxMovingAvgPoke / 5;
            // approxMovingAvgPoke += poke / 5;
            // //if (Mathf.Abs(poke) > 0.001f)
            // //{
            // //	Debug.Log("poke = " + poke);
            // //}
            // if (remoteGrabTime > 5)
            // {
            //     // scale remoteGrabDistance 
            //     remoteGrabDistance *= (1.0f + (poke * 5.0f));
            // }
            // prevPointer = pointer;
            // remoteGrabTargetPos = (pointer.origin + (remoteGrabDistance * pointer.direction));
            // // tractor beam to destination (mostly tangential to pointer axis (pitch / yaw movement)
            // myRawInteraction.RemoteGrabInteraction(primaryDown, remoteGrabTargetPos);
            // BackboneUnit bu = remoteGrab.GetComponent<BackboneUnit>();
            // if (bu != null)
            // {
            //     //add ROLL - torque from wrist twist
            //     Quaternion remoteGrabControllerCurrentQ = OVRInput.GetLocalControllerRotation(activeController);
            //     Quaternion remoteGrabControllerDeltaQ = remoteGrabControllerCurrentQ * Quaternion.Inverse(remoteGrabControllerStartQ);
            //     remoteGrabObjectTargetQ = remoteGrabControllerDeltaQ * remoteGrabObjectStartQ;
            //     //remoteGrab.gameObject.transform.rotation = Quaternion.Slerp(remoteGrab.gameObject.transform.rotation, remoteGrabObjectTargetQ, 0.1f);
            //     Vector3 vInit = remoteGrabControllerStartQ.eulerAngles;
            //     Vector3 vDelta = remoteGrabControllerDeltaQ.eulerAngles;
            //     Vector3 vCurrent = remoteGrabControllerCurrentQ.eulerAngles; // Quaternion.ToEulerAngles(q); 
            //     //Debug.Log(vInit.z + " -> " + vCurrent.z + " d = " + vDelta.z);
            //     float zRot = vDelta.z;
            //     if (zRot > 180.0f)
            //     {
            //         zRot -= 360.0f;
            //     }
            //     //Debug.Log(zRot);
            //     if (Mathf.Abs(zRot) > 15.0f) // threshold 
            //     {
            //         remoteGrab.gameObject.GetComponent<Rigidbody>().AddTorque(pointer.direction * zRot * 2.5f);
            //     }
            // }
            // else
            // {
            //     // not bu - UI - make the 'front' face the pointer
            //     // flipped because go was initially set up with z facing away

            //     //Use pointer position
            //     //Vector3 lookAwayPos = remoteGrab.gameObject.transform.position + pointer.direction;

            //     //Use HMD (possibly better - maybe a bit queasy)
            //     Vector3 lookAwayPos = remoteGrab.gameObject.transform.position + centreEyeAnchor.forward;
            //     remoteGrab.gameObject.transform.LookAt(lookAwayPos, Vector3.up);
            // }
        }

        private void ViveRemoteGrab()
        {
            if (vivePlayerGo == null)
            {
                return;
            }
            if (remoteGrabRigidBody)
            {
                Vector3 forceDirection = remoteGrabDestinationGo.transform.position - remoteGrab.position;
                remoteGrabRigidBody.AddForce(forceDirection * remoteGrabStrength, ForceMode.VelocityChange);
            }
        }

        private void SetRemoteGrabDestinationAnchor(Vector3 newPosition, Transform newParent)
        {
            // sets an anchor at the hit position and sets hand as parent so offsets are automatically calculated without code.
            remoteGrabDestinationGo.transform.position = newPosition;
            remoteGrabDestinationGo.transform.parent = viveLeftHand.transform;
        }

        private bool IsHandUI(Transform target)
        {
            // checks if transform is on VR UI layer and if it's a child of a hand.
            return (target.gameObject.layer == 11 && target.transform.parent == viveLeftHand.transform || target.transform.transform.parent == viveRightHand.transform);
        }

        void Update()
        {
            activeController = ViveInputHelpers.GetHandForButton(touchpad, activeController);

            Ray pointer;
            pointer = ViveInputHelpers.GetSelectionRay(activeController.transform);
            RaycastHit hit; // Was anything hit?
            if (Physics.Raycast(pointer, out hit, raycastDistance, ~excludeLayers))
            {
                myHitPos = hit.point;
                viveSelectionPointer.distance = hit.distance;

                // assumes vivePlayer script always attached to a gameObject.
                if (vivePlayer.gameObject != null)
                {
                    if (vivePlayer.gameObject.activeInHierarchy && !IsHandUI(hit.transform))
                    {
                        ProcessViveInputOnTarget(hit);
                    }
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
                    if (lastHit)
                    {

                    }
                }
            }
            // Nothing was hit, handle exit callback
            else
            {
                viveSelectionPointer.distance = 10.0f;
                // if aiming at nothing and trigger is not held down: clear the last hit/remote grabbed object.
                if (!vivePlayer.GetHairTrigger(viveLeftHand) && !vivePlayer.GetHairTrigger(viveRightHand))
                {
                    ClearLastHit();
                }
            }
            //REMOTE GRAB UPDATE (outside of hit test)
            if (remoteGrab)
            {
                // else if (viveLeftHand.controller.GetHairTrigger() || viveRightHand.controller.GetHairTrigger())
                if (vivePlayer.GetHairTrigger(viveLeftHand) || vivePlayer.GetHairTrigger(viveRightHand))
                {
                    ViveRemoteGrab();
                }
                else
                {
                    ClearRemoteGrab();
                }
            }
        }
    }
}
// Useful ref:
// https://developer.oculus.com/documentation/unity/latest/concepts/unity-integration-ovrinput/