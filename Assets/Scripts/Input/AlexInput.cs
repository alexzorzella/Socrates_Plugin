using UnityEngine;
using UnityEngine.InputSystem;

/**
 * Used by code at large to read input from a controller.
 */
public class AlexInput : MonoBehaviour {
	/**
	 * Unity component that keeps track of a controller input.
	 */
	PlayerInput input;

	/**
	 * Unity-assigned identifier to a controller. 
	 */
	int deviceId;

	// Input overriding
	bool overrideInput = false;

	public void SetOverrideFlag() {
		overrideInput = true;
	}
	
	public bool GetOverrideFlag() {
		return overrideInput;
	}
    
	Vector2 leftStickOverride;

	public void SetLeftStickOverride(Vector2 leftStickOverride) { this.leftStickOverride = leftStickOverride; }
	
	void Start() {
		if (GetComponent<PlayerInput>() == null) { // Is this okay?
			deviceId = -2;
		
			#if UNITY_EDITOR
				JConsole.i.LogSystemMessage("CPU input device added");
			#endif
			
			return;
		}
		
		input = GetComponent<PlayerInput>();
		
		InputManager inputManager = InputManager.instance;

		InputDevice inputDevice = GetDevice();
		deviceId = inputDevice.deviceId;

		gameObject.name = $"(Device {deviceId}) {GetDevice().displayName} Input";

		bool connectionSucessful = inputManager.AddSource(this);

		if (connectionSucessful) {
			// JConsole.i.DisplaySystemMessage($"{gameObject.name} connected.");
			JConsole.i.LogSystemMessage($"{GetDevice().displayName} connected");
		}

		transform.SetParent(inputManager.transform);
		transform.localPosition = Vector2.zero;
	}

	public bool DeviceIsKeyboard() {
		return input.GetDevice<Keyboard>() != null;
	}

	public InputDevice GetDevice() {
		return input.devices.Count > 0 ? input.devices[0] : null;
	}

	public int GetDeviceId() {
		return deviceId;
	}

	private InputActionMap GetActionMap() {
		return input.actions.FindActionMap("Player");
	}

	public Vector2 LeftStickRaw() {
		return !overrideInput ? GetActionMap().FindAction("LeftStick").ReadValue<Vector2>() : leftStickOverride;
	}

	const float stickDeadzoneX = 0.1F;
	const float stickDeadzoneY = 0.25F;

	public Vector2 LeftStickForgiving() {
		if (overrideInput) {
			return leftStickOverride;
		}
		
		Vector2 cardinal = LeftStickRaw();
		// return new Vector2(Mathf.CeilToInt(cardinal.x), Mathf.CeilToInt(cardinal.y));
		float resultX = 0;
		float resultY = 0;

		if(cardinal.x > stickDeadzoneX) {
			resultX = 1;
		} else if(cardinal.x < -stickDeadzoneX) {
			resultX = -1;
		}

		if(cardinal.y > stickDeadzoneY) {
			resultY = 1;
		} else if(cardinal.y < -stickDeadzoneY) {
			resultY = -1;
		}

		return new Vector2(resultX, resultY);
	}

	public Vector2 LeftStick() {
		if (overrideInput) {
			return leftStickOverride;
		}
		
		Vector2 cardinal = LeftStickRaw();
		return new Vector2(Mathf.RoundToInt(cardinal.x), Mathf.RoundToInt(cardinal.y));
		// return GetActionMap().FindAction("LeftStick").ReadValue<Vector2>();
	}

	public Vector2 RightStick() {
		Vector2 cardinal = GetActionMap().FindAction("RightStick").ReadValue<Vector2>();
		return new Vector2(Mathf.RoundToInt(cardinal.x), Mathf.RoundToInt(cardinal.y));
		// return GetActionMap().FindAction("LeftStick").ReadValue<Vector2>();
	}

	public class InputFlag {
		public string actionName;
		InputActionMap actionMap;

		public bool lastPressed;

		public InputFlag(string actionName, InputActionMap actionMap, bool lastPressed) {
			this.actionName = actionName;
			this.actionMap = actionMap;

			this.lastPressed = lastPressed;
		}

		public bool InputUpdated() {
			if (actionMap.FindAction(actionName).IsPressed() != lastPressed) {
				lastPressed = actionMap.FindAction(actionName).IsPressed();
			}

			return true;
		}

		public bool IsPressed() {
			return actionMap.FindAction(actionName).IsPressed();
		}
	}

	public bool LightPunchPressed() {
		return GetActionMap().FindAction("LPunch").IsPressed();
	}

	public bool MediumPunchPressed() {
		return GetActionMap().FindAction("MPunch").IsPressed();
	}

	public bool HeavyPunchPressed() {
		return GetActionMap().FindAction("HPunch").IsPressed();
	}

	public bool LightKickPressed() {
		return GetActionMap().FindAction("LKick").IsPressed();
	}	

	public bool MediumKickPressed() {
		return GetActionMap().FindAction("MKick").IsPressed();
	}

	public bool HeavyKickPressed() {
		return GetActionMap().FindAction("HKick").IsPressed();
	}

	public bool PlusPressed() {
		return GetActionMap().FindAction("Plus").IsPressed();
	}

	public bool ScreenshotPressed() {
		return GetActionMap().FindAction("Screenshot").IsPressed();
	}

	public bool HomePressed() {
		return GetActionMap().FindAction("Home").IsPressed();
	}

	public bool TauntPressed() {
		return GetActionMap().FindAction("Taunt").IsPressed();
	}

	public bool RespectPressed() {
		return GetActionMap().FindAction("Respect").IsPressed();
	}

	public void Destroy() {
		Destroy(gameObject);
	}

	public override string ToString() {
		return gameObject.name;
	}
}