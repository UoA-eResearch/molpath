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
        private ulong touchpad = SteamVR_Controller.ButtonMask.Touchpad;

        [Tooltip("Trigger button")]
        private ulong trigger = SteamVR_Controller.ButtonMask.Trigger;

        private ulong grip = SteamVR_Controller.ButtonMask.Grip;

        [Tooltip("Layers to exclude from raycast")]
        public LayerMask excludeLayers;
        [Tooltip("Maximum raycast distance")]
        public float raycastDistance = 500;

		public bool touchpadOverloads = false;
		public bool keyboardControls = true;
		public ViveRawInteraction viveRawInteration;

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
        public Transform remoteGrabController;

        void Awake()
        {
            if (!vivePlayer)
            {
				vivePlayer = Player.instance;
                if (vivePlayer)
				{
					viveLeftHand = vivePlayer.leftHand;
					viveRightHand = vivePlayer.rightHand;
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
            
            if (!viveSelectionPointer)
            {
				viveSelectionPointer = FindObjectOfType<ViveSelectionPointer>();
			}

            if (!viveRawInteration)
            {
				viveRawInteration = GetComponent<ViveRawInteraction>();
			}
            if (!viveRawInteration)
            {
				viveRawInteration = FindObjectOfType<ViveRawInteraction>();
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

        private void InitializeRemoteGrab(RaycastHit hit, Transform controller)
        {
            remoteGrabController = controller;
            remoteGrab = lastHit;
            remoteGrabDistance = hit.distance;
            //Debug.Log("   --->" + hit.distance);
            remoteGrabStartPos = hit.point;
            approxMovingAvgPoke = 0f;
            remoteGrabTime = 0;

            remoteGrabObjectStartQ = remoteGrab.gameObject.transform.rotation;
            remoteGrabControllerStartQ = controller.localRotation;

            BackboneUnit bu = remoteGrab.gameObject.GetComponent<BackboneUnit>();
            if (bu != null)
            {
                bu.SetRemoteGrabSelect(true);
                remoteGrabBackboneUnit = bu;
                //bu.remoteGrabSelectOn = true;
                //bu.UpdateRenderMode();
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
			// TODO: Make function calls from either all player.instance or viveplayer.
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

			if (keyboardControls)
			{
				// some test keyboard inputs 1,2, 3, 4
				if (Input.GetKeyDown(KeyCode.Alpha1))
				{
					Debug.Log("1");
					primaryDown = lastHit;
				}
				if (Input.GetKeyDown(KeyCode.Alpha2))
				{
					Debug.Log("2");
					secondaryDown = lastHit;
				}
				if (Input.GetKeyDown(KeyCode.Alpha3))
				{
					Debug.Log("3");
					aDown = lastHit;
				}
				if (Input.GetKeyDown(KeyCode.Alpha4))
				{
					Debug.Log("4");
					bDown = lastHit;
				}
				if (Input.GetKeyUp(KeyCode.Alpha1))
				{
					Debug.Log("up");
					primaryDown = null;
				}
				if (Input.GetKeyUp(KeyCode.Alpha2))
				{
					Debug.Log("up");
					secondaryDown = null;
				}
				if (Input.GetKeyUp(KeyCode.Alpha3))
				{
					Debug.Log("up");
					aDown = null;
				}
				if (Input.GetKeyUp(KeyCode.Alpha4))
				{
					Debug.Log("up");
					bDown = null;
				}
				if (aDown)
				{
					Debug.Log("invoking a down");
					onHoverADown.Invoke(aDown);
					viveRawInteration.OnHoverADown(aDown);
				}
				if (bDown)
				{
					Debug.Log("invoking b down");
					onHoverBDown.Invoke(bDown);
					viveRawInteration.OnHoverBDown(bDown);
				}
				if (primaryDown && !secondaryDown)
				{
					// tractor beam stub
				}
				else if (secondaryDown && !primaryDown)
				{
					// tactor time iterates?
				}
			}
            if (touchpadOverloads)
            {
				if (vivePlayer.GetGripIsSqueezing(activeController))
				{
					Debug.Log("pressing grip");
					if (vivePlayer.GetPressDown(activeController, SteamVR_Controller.ButtonMask.Touchpad))
					{
						Debug.Log("pressing touchpad + grip");
						if (vivePlayer.GetPressDownB(activeController, SteamVR_Controller.ButtonMask.Touchpad))
						{
							Debug.Log("right b press (lower half)");
						}
						if (vivePlayer.GetPressDownA(activeController, SteamVR_Controller.ButtonMask.Touchpad))
						{
							Debug.Log("right a press (upper half)");
						}
					}
				}
            }

            // standard vive inputs
			if (vivePlayer.GetHairTriggerDown(activeController))
			{
				InitializeRemoteGrab(hit, activeController.transform);
				// SetRemoteGrab(hit.point, viveRightHand.transform);
			}
			if (vivePlayer.GetHairTriggerUp(activeController))
			{
				ClearRemoteGrab();
			}
        }

        private void ViveRemoteGrab(Ray pointer)
        {
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
            viveRawInteration.RemoteGrabPositionalInteration(remoteGrab, remoteGrabTargetPos);
            BackboneUnit bu = remoteGrab.gameObject.GetComponent<BackboneUnit>();
            if (bu != null)
            {
                //add ROLL - torque from wrist twist
                Quaternion remoteGrabControllerCurrentQ = remoteGrabController.localRotation;
                Quaternion remoteGrabControllerDeltaQ = remoteGrabControllerCurrentQ * Quaternion.Inverse(remoteGrabControllerStartQ);
                remoteGrabObjectTargetQ = remoteGrabControllerDeltaQ * remoteGrabObjectStartQ;
                //remoteGrab.gameObject.transform.rotation = Quaternion.Slerp(remoteGrab.gameObject.transform.rotation, remoteGrabObjectTargetQ, 0.1f);
                Vector3 vInit = remoteGrabControllerStartQ.eulerAngles;
                Vector3 vDelta = remoteGrabControllerDeltaQ.eulerAngles;
                Vector3 vCurrent = remoteGrabControllerCurrentQ.eulerAngles; // Quaternion.ToEulerAngles(q); 
                //Debug.Log(vInit.z + " -> " + vCurrent.z + " d = " + vDelta.z);
                float zRot = vDelta.z;
                viveRawInteration.RemoteGrabRotationalInteration(zRot, remoteGrab, pointer);
            }
            if (remoteGrab.GetComponent<Canvas>() || remoteGrab.gameObject.layer == 11)
			{
				// UI - make the 'front' face the pointer
				// flipped because UI GO was initially set up with z facing away

				//Use pointer position
				//Vector3 lookAwayPos = remoteGrab.gameObject.transform.position + pointer.direction;

				//Use HMD (possibly better - maybe a bit queasy)
				Vector3 lookAwayPos = remoteGrab.gameObject.transform.position + Player.instance.hmdTransform.position;

				// use the transform of the GO this script is attached to (RawInteraction) as proxy for storing target rotation :)
				transform.position = remoteGrab.position;
				transform.LookAt(lookAwayPos, Vector3.up);

				// lerp to target - eases the rotation of the UI - useful when remote grab just initiated
				// remoteGrab.gameObject.transform.rotation = Quaternion.Lerp(remoteGrab.gameObject.transform.rotation, transform.rotation, Time.deltaTime * 10.0f);
				//remoteGrab.gameObject.transform.LookAt(lookAwayPos, Vector3.up);
				remoteGrab.rotation = Quaternion.Euler(pointer.direction);
			}
        }

        void Update()
        {
            activeController = ViveInputHelpers.GetHandForButton(touchpad, activeController);
            if (!activeController)
            {
				return;
			}
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
                    if (vivePlayer.gameObject.activeInHierarchy)
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
                if (!vivePlayer.GetHairTrigger(activeController))
                {
                    ClearLastHit();
                }
            }
            //REMOTE GRAB UPDATE (outside of hit test)
            if (remoteGrab)
            {
                if (vivePlayer.GetHairTrigger(activeController))
                {
                    ViveRemoteGrab(pointer);
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