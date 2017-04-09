using UnityEngine;
using Valve.VR;
using System.Collections;
using System.Collections.Generic;

public class ViveInput : MonoBehaviour
{
	public bool testMode = false;

	public ViveWand left = new ViveWand();
	public ViveWand right = new ViveWand();
	
	public GameObject leftControllerObject;
	public GameObject rightControllerObject;
		
	void Start() {
		InvokeRepeating("DiscoverControllers", 0, 0.5f);
	}

    public void Update()
    {
		if (!testMode) {
			var vr = SteamVR.instance;
			ViveWand[] viveWands = {left, right};
			foreach (ViveWand viveWand in viveWands) {
				viveWand.touchPad.x = GetTouchPadX (vr, viveWand.index);
				viveWand.touchPad.y = GetTouchPadY (vr, viveWand.index);
				viveWand.touchPad.SetPressed (GetTouchPadIsPressed(vr, viveWand.index));

				viveWand.trigger.SetPressed (GetTriggerIsPressed (vr, viveWand.index));
				viveWand.trigger.SetTouched (GetTriggerIsTouched (vr, viveWand.index));
				viveWand.topButton.SetPressed (GetTopButtonIsPressed (vr, viveWand.index));
				viveWand.grip.SetPressed (GetGripIsPressed(vr, viveWand.index));
				viveWand.grip.SetTouched (GetGripIsTouched(vr, viveWand.index));
			}
		}

		if (testMode) {
			ViveWand[] viveWands = {left, right};
			foreach (ViveWand viveWand in viveWands) {
				viveWand.touchPad.SetPressed(viveWand.touchPad.pressed);
				viveWand.trigger.SetPressed (viveWand.trigger.pressed);
				viveWand.topButton.SetPressed (viveWand.topButton.pressed);
				viveWand.grip.SetPressed (viveWand.grip.pressed);

			}
		}

        HapticPulse();
        TouchQuadrantEvents();
		TouchOctantEvents();
	}

	void DiscoverControllers() {
		//if (left.gameObject == null) {
			for (int i = 0; i < 16; i++) {
				if (leftControllerObject.GetComponent<SteamVR_TrackedObject> ().index.ToString () == "Device" + i) {
					left.index = (uint)i;
					left.gameObject = leftControllerObject;
				}
			}
		//}
		
		//if (right.gameObject == null) {
			for (int i = 0; i < 16; i++) {
				if (rightControllerObject.GetComponent<SteamVR_TrackedObject>().index.ToString() == "Device" + i) {
					right.index = (uint)i;
					right.gameObject = rightControllerObject;
				}
			}
		//}
	}

	bool GetTriggerIsTouched(SteamVR vr, uint controller)
	{
		if (controller == 0)
			return false;
		return SteamVR_Controller.Input((int)controller).GetTouch(SteamVR_Controller.ButtonMask.Trigger);
	}
	
	bool GetTriggerIsPressed(SteamVR vr, uint controller)
	{
		if (controller == 0)
			return false;
		return SteamVR_Controller.Input((int)controller).GetPress(SteamVR_Controller.ButtonMask.Trigger);
	}

	bool GetTopButtonIsPressed(SteamVR vr, uint controller)
	{
		if (controller == 0)
			return false;
		return SteamVR_Controller.Input((int)controller).GetPress(SteamVR_Controller.ButtonMask.ApplicationMenu);	}

	bool GetTouchPadIsPressed(SteamVR vr, uint controller)
	{
		if (controller == 0)
			return false;
		return SteamVR_Controller.Input((int)controller).GetPress(SteamVR_Controller.ButtonMask.Touchpad);	}

	bool GetGripIsTouched(SteamVR vr, uint controller)
	{
		if (controller == 0)
			return false;
		return SteamVR_Controller.Input((int)controller).GetTouch(SteamVR_Controller.ButtonMask.Grip);
	}

	bool GetGripIsPressed(SteamVR vr, uint controller)
	{
		if (controller == 0)
			return false;
		return SteamVR_Controller.Input((int)controller).GetPress(SteamVR_Controller.ButtonMask.Grip);	}


	float GetTouchPadX(SteamVR vr, uint controller)
	{
		if (controller == 0)
			return 0;
		var state = new VRControllerState_t();
		var success = vr.hmd.GetControllerState(controller, ref state, 10);
		return state.rAxis0.x;
	}

	float GetTouchPadY(SteamVR vr, uint controller)
	{
		if (controller == 0)
			return 0;
		var state = new VRControllerState_t();
		var success = vr.hmd.GetControllerState(controller, ref state, 10);
		return state.rAxis0.y;
	}

	void HapticPulse()
	{	
		float leftHaptic = left.haptic.Count > 0 ? left.haptic[0] : 0;
		float rightHaptic = right.haptic.Count > 0 ? right.haptic[0] : 0;

		if (testMode)
		{
			leftHaptic = left.testHaptic;
			rightHaptic = right.testHaptic;
		}

		float leftEffectiveHaptic = Mathf.Min(leftHaptic, 1);
		float rightEffectiveHaptic = Mathf.Min(rightHaptic, 1);

        if (left.index != 0) {
            SteamVR_Controller.Input((int)left.index).TriggerHapticPulse((ushort)(3999 * leftEffectiveHaptic), EVRButtonId.k_EButton_Axis0);
        }

        if (right.index != 0) {
            SteamVR_Controller.Input((int)right.index).TriggerHapticPulse((ushort)(3999 * rightEffectiveHaptic), EVRButtonId.k_EButton_Axis0);
        }

		if (left.haptic.Count > 0) {
			left.haptic.RemoveAt(0);
		}

		if (right.haptic.Count > 0) {
			right.haptic.RemoveAt(0);
		}
	}

	void TouchQuadrantEvents() {
		TouchPad[] touchPads = new TouchPad[] {left.touchPad, right.touchPad};
		
		foreach (TouchPad touchPad in touchPads) {
			bool topTouched = false;
			bool rightTouched = false;
			bool bottomTouched = false;
			bool leftTouched = false;

			if (touchPad.x != 0 || touchPad.y != 0) {
				if (touchPad.y > 0 && Mathf.Abs (touchPad.y) > Mathf.Abs (touchPad.x)) {
					topTouched = true;
				} else if (touchPad.y < 0 && Mathf.Abs (touchPad.y) > Mathf.Abs (touchPad.x)) {
					bottomTouched = true;
				} else if (touchPad.x > 0 && Mathf.Abs (touchPad.x) > Mathf.Abs (touchPad.y)) {
					rightTouched = true;
				} else if (touchPad.x < 0 && Mathf.Abs (touchPad.x) > Mathf.Abs (touchPad.y)) {
					leftTouched = true;
				}
			} 

			SetTouchEventState(touchPad.quadrant.top, topTouched, touchPad); 
			SetTouchEventState(touchPad.quadrant.bottom, bottomTouched, touchPad); 
			SetTouchEventState(touchPad.quadrant.right, rightTouched, touchPad); 
			SetTouchEventState(touchPad.quadrant.left, leftTouched, touchPad); 
		}
	}

	void SetTouchEventState(TouchEvent touchEvent, bool isTouched, TouchPad touchPad) {
		touchEvent.SetTouched (isTouched);
		touchEvent.SetPressed (isTouched && touchPad.pressed);
	}

	void TouchOctantEvents() {
		TouchPad[] touchPads = new TouchPad[] {left.touchPad, right.touchPad};

		foreach (TouchPad touchPad in touchPads) {
			bool touchingTop = false;
			bool touchingTopRight = false;
			bool touchingRight = false;
			bool touchingBottomRight = false;
			bool touchingBottom = false;
			bool touchingBottomLeft = false;
			bool touchingLeft = false;
			bool touchingTopLeft = false;

			if (touchPad.x != 0 || touchPad.y != 0) {
				// Detects if touch pad is on top octant

				// Detects if touch pad is in one of the corners:
				if (Mathf.Abs(touchPad.x) * 2 > Mathf.Abs(touchPad.y) &&
				Mathf.Abs(touchPad.y) * 2 > Mathf.Abs(touchPad.x)) {
					if (touchPad.y > 0 && touchPad.x > 0) {
						touchingTopRight = true;
					} else if (touchPad.y < 0 && touchPad.x > 0) {
						touchingBottomRight = true;
					} else if (touchPad.y < 0 && touchPad.x < 0) {
						touchingBottomLeft = true;
					} else if (touchPad.y > 0 && touchPad.x < 0) {
						touchingTopLeft = true;
					}
				// the touch pad is in one of the four cardinal directions.
				} else {
					if (touchPad.y > 0 && Mathf.Abs (touchPad.y) > Mathf.Abs (touchPad.x) * 2) {
						touchingTop = true;
					} else if (touchPad.y < 0 && Mathf.Abs (touchPad.y) > Mathf.Abs (touchPad.x) * 2) {
						touchingBottom = true;
					} else if (touchPad.x > 0 && Mathf.Abs (touchPad.x) > Mathf.Abs (touchPad.y) * 2) {
						touchingRight = true;
					} else if (touchPad.x < 0 && Mathf.Abs (touchPad.x) > Mathf.Abs (touchPad.y) * 2) {
						touchingLeft = true;
					}
				}
			}

			SetTouchEventState(touchPad.octant.top, touchingTop, touchPad); 
			SetTouchEventState(touchPad.octant.bottom, touchingBottom, touchPad); 
			SetTouchEventState(touchPad.octant.right, touchingRight, touchPad); 
			SetTouchEventState(touchPad.octant.left, touchingLeft, touchPad); 
			SetTouchEventState(touchPad.octant.topLeft, touchingTopLeft, touchPad); 
			SetTouchEventState(touchPad.octant.bottomLeft, touchingBottomLeft, touchPad); 
			SetTouchEventState(touchPad.octant.topRight, touchingTopRight, touchPad); 
			SetTouchEventState(touchPad.octant.topLeft, touchingTopLeft, touchPad); 
		}
	}

	[System.Serializable]
	public class ViveWand {
		public TouchPad touchPad;
		public uint index;
		//public float haptic = 0;
		public List<float> haptic = new List<float>();
		public ButtonEvent trigger;
		public ButtonEvent topButton;
		public ButtonEvent grip;
		public GameObject gameObject;		
		public float testHaptic;

		public void AddHapticPulse(float strength, float duration) {
			for (int i = 0; i < duration; i++) {
				if (haptic.Count < i+1) {
					haptic.Add(strength);
				} else {
					haptic[i] += strength;
				}
			}
		}
	}

	[System.Serializable]
	public class TouchPad {
		public TouchPadOctant octant = new TouchPadOctant();
		public TouchPadQuadrant quadrant = new TouchPadQuadrant ();
		public float x = 0;
		public float y = 0;
		public bool pressed = false;
		public bool pressedDown = false;
		public bool pressedUp = false;
		private bool pressedLast = false;

		public void SetPressed(bool pressedState) {
			pressed = pressedState;
			if (pressed && !pressedLast) {
				pressedDown = true;
			} else {
				pressedDown = false;
			}

			if (!pressed && pressedLast) {
				pressedUp = true;
			} else {
				pressedUp = false;
			}

			pressedLast = pressed;
		}
	}

	[System.Serializable]
	public class TouchPadOctant {
		public TouchEvent top;
		public TouchEvent topRight;
		public TouchEvent right;
		public TouchEvent bottomRight;
		public TouchEvent bottom;
		public TouchEvent bottomLeft;
		public TouchEvent left;
		public TouchEvent topLeft;
	}

	[System.Serializable]
	public class TouchPadQuadrant {
		public TouchEvent top;
		public TouchEvent right;
		public TouchEvent bottom;
		public TouchEvent left;
	}

	[System.Serializable]
	public class TouchEvent {
		public bool pressed;
		public bool pressedDown;
		public bool pressedUp;
		private bool pressedLast;
		public bool touched;
		public bool touchedDown;
		public bool touchedUp;
		private bool touchedLast;

		public void SetPressed(bool pressedState) {
			pressed = pressedState;
			if (pressed && !pressedLast) {
				pressedDown = true;
			} else {
				pressedDown = false;
			}
			
			if (!pressed && pressedLast) {
				pressedUp = true;
			} else {
				pressedUp = false;
			}
			
			pressedLast = pressed;
		}

		public void SetTouched(bool touchedState) {
			touched = touchedState;
			if (touched && !touchedLast) {
				touchedDown = true;
			} else {
				touchedDown = false;
			}
			
			if (!touched && touchedLast) {
				touchedUp = true;
			} else {
				touchedUp = false;
			}
			
			touchedLast = touched;
		}
	}

	[System.Serializable]
	public class ButtonEvent {
		public bool pressed;
		public bool pressedDown;
		public bool pressedUp;
		private bool pressedLast;
		public bool touched;
		public bool touchedDown;
		public bool touchedUp;
		private bool touchedLast;

		public void SetPressed(bool pressedState) {
			pressed = pressedState;
			if (pressed && !pressedLast) {
				pressedDown = true;
			} else {
				pressedDown = false;
			}
			
			if (!pressed && pressedLast) {
				pressedUp = true;
			} else {
				pressedUp = false;
			}
			
			pressedLast = pressed;
		}

		public void SetTouched(bool touchedState) {
			touched = touchedState;
			if (touched && !touchedLast) {
				touchedDown = true;
			} else {
				touchedDown = false;
			}
			
			if (!touched && touchedLast) {
				touchedUp = true;
			} else {
				touchedUp = false;
			}
			
			touchedLast = touched;
		}

	}
}


