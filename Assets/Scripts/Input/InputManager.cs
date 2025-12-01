using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameControllerSpec {
	public readonly float registeredTimestamp;

	// TODO: I think it would be useful to store Input here as well, not sure
	public GameControllerSpec(float registeredTimestamp) {
		this.registeredTimestamp = registeredTimestamp;
	}
}

public class InputManager : MonoBehaviour {
	EventManager eventManager;
	// Order doesn't matter here. All this script cares about is calling OnUpdate(...) on all the inputs that these
	// InputHandlers want
	List<InputHandler> inputHandlers = new List<InputHandler>();

	// This should keep track of ALL ports
	// Ports should sleep when its associated controller is disconnected
	Dictionary<int, AlexInput> ports = new Dictionary<int, AlexInput>();
	// List<RegisteredInput> ports = new List<RegisteredInput>();
	Dictionary<int, GameControllerSpec> registeredGameControllers = new Dictionary<int, GameControllerSpec>();
	// This is a Dictionary<deviceId, timestamp>

	const float registerBuffer = 0.2F;

	public static InputManager instance;

	public void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(gameObject);
		} else if (instance != this) {
			Destroy(gameObject);
			return;
		}

		eventManager = GameManager.Instance().eventManager;
	}

	public bool AddSource(AlexInput newInput) {
		if (instance != this) {
			throw new InvalidOperationException("Wrong InputManager instance");
		}

		InputDevice newDevice = newInput.GetDevice();
		// TODO: rename to newDeviceId
		int deviceId = newDevice.deviceId;

		// // newDevice.
		// Type type = newDevice.GetType();

        // // Get the field info for m_HandshakeTimer
        // FieldInfo fieldInfo = type.GetField("m_HandshakeTimer",
		//   BindingFlags.NonPublic | BindingFlags.Instance);
        
        // if (fieldInfo != null) {
		// 	return false;
		// }

		if (false && registeredGameControllers.ContainsKey(deviceId)) {
			
			// TODO: not sure if this is something we should try to survive
			if (true) {
			  throw new InvalidOperationException($"Unity is registering the device id {deviceId} multiple times.");
			}
		}

		float now = Time.time;
		float cutOffTimestamp = now - registerBuffer;

		foreach (var controller in registeredGameControllers) {
			// if (false && controller.Value.registeredTimestamp > cutOffTimestamp) {
			if (controller.Value.registeredTimestamp > cutOffTimestamp) {
				// We are going to consider this input a dupe of the
				// one we already found.
				// TODO: if this only happens for, like, a specific
				// kind of controller, we can add more conditions here.

				// We add another entry to the dictionary pointing to
				// the same spec as its dupe.

				JConsole.i.WriteLine($"Device {deviceId} was detected as a duplicate, ignored.");

				// registeredGameControllers.Add(deviceId, controller.Value);

				// TODO maybe DisableDevice
				// InputSystem.DisableDevice(newDevice);

				return false;
			}
		}

		registeredGameControllers.Add(deviceId, new GameControllerSpec(now));

		int connectToPort = GetFirstAvailablePort();

		if (connectToPort == -1) {
			connectToPort = ports.Count;
			ports.Add(connectToPort, newInput);

			// ports.Add(ports.Count, newInput);
			// connectToPort = ports.Count - 1;
		} else {
			ports[connectToPort] = newInput;
		}

		AlexInput input = GetInputForPort(connectToPort);

		foreach (var handler in inputHandlers) {
			// The handler cares about the port   or the handler cares about every port
			if (handler.GetPort() == connectToPort || handler.GetPort() < 0) {
				handler.OnInputChanged(input);

				eventManager.RegisterListener(input.GetDeviceId(), handler.GetEventListener(), handler.ListensForEvents().ToArray());
			}
		}

		return true;
	}

	// Returns the first available port (the first port with a null input)
	// If there is none, it returns -1
	public int GetFirstAvailablePort() {
		int result = -1;

		for (int i = 0; i < ports.Count; i++) {
			if (ports.ElementAt(i).Value == null) {
				result = ports.ElementAt(i).Key;
				break;
			}
		}

		return result;
	}

	public int PortUsedByDevice(int deviceId) {
		for (int i = 0; i < ports.Count; i++) {
			if (ports.ElementAt(i).Value == null) {
				continue;
			}

			if (ports.ElementAt(i).Value.GetDeviceId() == deviceId) {
				return i;
			}
		}

		return -1;
	}

	public bool PortConnected(int port) {
		return GetInputForPort(port) != null;
	}

	public AlexInput GetInputForPort(int port) {
		return (port < ports.Count) ? ports[port] : null;
	}

	void Update() {
		CallUpdateOnAllHandlers();
	}

	void CallUpdate(int port) {
		if (port >= ports.Count) {
			return;
		}

		AlexInput input = ports[port];
		// AlexInput input = ports.ElementAt(port).Value;
		// Input input = GetInputForPort(port);

		bool inputDisconnected = false;

		if (input != null) {
			inputDisconnected = !eventManager.OnUpdate(input);
		}

		// In the case of a disconnected input
		if (inputDisconnected) {
			registeredGameControllers.Remove(input.GetDeviceId()); // Remove the controller from the list of paired controllers?
			Destroy(ports[port].gameObject);
			ports[port] = null; // Remove the controller from the list of ports (the port remains while the Input is set to null)

			JConsole.i.LogSystemMessage($"{input} disconnected");
			// Maybe this will lead to issues reconnecting it...

			foreach (var handler in inputHandlers) {
				// The handler cares about the port   or the handler cares about every port
				if (handler.GetPort() == port || handler.GetPort() < 0) {
					handler.OnInputChanged(input);
				}
			}
		}
	}

	void CallUpdateOnAllHandlers() {
		// I wish:
		foreach (var handler in inputHandlers) {
			int port = handler.GetPort();

			if (port >= 0) {
				CallUpdate(port);
			} else if (port < 0) {
				for (int i = 0; i < ports.Count; i++) {
					CallUpdate(i);
				}
			}
		}
	}

	public void RegisterInputHandler(InputHandler inputHandler) {
		inputHandlers.Add(inputHandler);

		int handlerPort = inputHandler.GetPort();

		if (handlerPort >= 0) {
			RegisterHandlerToPort(handlerPort, inputHandler);
		} else if (handlerPort < 0) {
			RegisterHandlerToAllPorts(inputHandler);
		}
	}

	void RegisterHandlerToPort(int port, InputHandler inputHandler) {
		if (port < ports.Count) {
			AlexInput input = GetInputForPort(port);

			inputHandler.OnInputChanged(input);

			if (input != null) {
				eventManager.RegisterListener(input.GetDeviceId(), inputHandler.GetEventListener(), inputHandler.ListensForEvents().ToArray());
			}
		}
	}

	void RegisterHandlerToAllPorts(InputHandler inputHandler) {
		for (int i = 0; i < ports.Count; i++) {
			RegisterHandlerToPort(i, inputHandler);
		}
	}

	public string GetControllersAsString() {
		string result = "";

		foreach (var port in ports) {
			if (port.Value != null) {
				result += $"{port.Value}\n";
			}
		}

		return result;
	}

	public string GetControllersAsString(int startWith) {
		string result = "";

		if (startWith >= ports.Count) {
			return result;
		}

		for (int i = startWith; i < ports.Count; i++) {
			if (ports.ElementAt(i).Value != null) {
				result += $"{ports.ElementAt(i).Value}\n";
			}
		}

		return result;
	}

	public int GetFirstAvailablePortAllowingOverflowSkippingActivePorts() {
		int result = ports.Count;

		// if(ports.Count < 2) {
		// 	return result;
		// }

		for (int i = 2; i < ports.Count; i++) {
			if (ports.ElementAt(i).Value == null) {
				result = ports.ElementAt(i).Key;
				break;
			}
		}

		return result;
	}

	public void SwapPortInputs(int entryOne, int entryTwo) {
		if (entryTwo == ports.Count) {
			ports.Add(ports.Count, null);
		}

		AlexInput temp = ports[entryTwo];
		ports[entryTwo] = ports[entryOne];
		ports[entryOne] = temp;

		CallUpdateOnAllHandlers();
		ReregisterHandlers();
	}

	public void ReregisterHandlers() {
		eventManager.ClearRegistry();

		foreach (var handler in inputHandlers) {
			int port = handler.GetPort();

			if (port >= 0) {
				AlexInput input = GetInputForPort(handler.GetPort());

				if (input != null) {
					eventManager.RegisterListener(input.GetDeviceId(), handler.GetEventListener(), handler.ListensForEvents().ToArray());
				}

				handler.OnInputChanged(input);
			} else if(port < 0) {
				for(int i = 0; i < ports.Count; i++) {
					AlexInput input = GetInputForPort(i);

					if (input != null) {
						eventManager.RegisterListener(input.GetDeviceId(), handler.GetEventListener(), handler.ListensForEvents().ToArray());
					}

					handler.OnInputChanged(input);
				}
			}
		}
	}

	public int ActivePortsAssigned() {
		int result = 0;
		
		if(ports.ContainsKey(0)) {
			if(ports[0] != null) {
				result++;
			}
		}

		if(ports.ContainsKey(1)) {
			if(ports[1] != null) {
				result++;
			}
		}

		return result;
	}

	public void ClearHandlers() {
		List<InputHandler> persistentHandlers = new List<InputHandler>();

		foreach (var handler in inputHandlers) {
			if (handler.PersistsThroughLoads()) {
				persistentHandlers.Add(handler);
			}
		}

		inputHandlers = new List<InputHandler>();

		// inputHandlers.Clear();

		foreach(var persistentHandler in persistentHandlers) {
			RegisterInputHandler(persistentHandler);
		}
	}

	// Removes all connected controllers from the game
	public void ClearControllers() {
		JConsole.i.LogSystemMessage($"Clearing controllers...");
		// TODO: Humm...
		AlexInput[] allInputs = FindObjectsOfType<AlexInput>();

		for(int i = 0; i < allInputs.Length; i++) {
			Destroy(allInputs[i].gameObject);
		}

		ports.Clear();
		registeredGameControllers.Clear();
	}
}