using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace ViveInputs
{
    public class ViveInputHelpers
    {
        // Given a controller and tracking spcae, return the ray that controller uses.
        // Will fall back to center eye or camera on Gear if no controller is present.
        public static Ray GetSelectionRay(Transform originHand)
        {
            if (originHand)
            {
                return new Ray(originHand.position, originHand.forward);
            }
            else
            {
                Transform viveCamera = Player.instance.hmdTransform;
                return new Ray(viveCamera.position, viveCamera.forward);
            }
        }

        public static Hand GetHandForButton(ulong button, Hand oldHand)
        {
            if (Player.instance.leftHand)
            {
                if (Player.instance.leftHand.controller != null)
                {
                    if (Player.instance.leftHand.controller.GetPress(button) || oldHand == null)
                    {
                        return Player.instance.leftHand;
                    }
                }
            }
            if (Player.instance.leftHand)
            {
                if (Player.instance.rightHand.controller != null)
                {
                    if (Player.instance.rightHand.controller.GetPress(button) || oldHand == null)
                    {
                        return Player.instance.rightHand;
                    }
                }
            }
            return oldHand;
        }
    }
}
