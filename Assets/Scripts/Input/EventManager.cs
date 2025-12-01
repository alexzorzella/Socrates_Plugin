using System;
using System.Collections.Generic;
using UnityEngine;

public enum Button {
	BUTTON_A,
	BUTTON_B,
	BUTTON_X,
	BUTTON_Y,
	NORTH,
	NORTH_EAST,
	EAST,
	SOUTH_EAST,
	SOUTH,
	SOUTH_WEST,
	WEST,
	NORTH_WEST,
	BUTTON_PLUS,
	BUTTON_RB,
	BUTTON_LB,
	CENTER,
	SCREENSHOT,
	HOME,
	TAUNT,
	RESPECT,
	SENTINEL
}

public static class Extensions {
	public static EventType Pressed(this Button button) {
		return new EventType(button, true);
	}
	public static EventType Released(this Button button) {
		return new EventType(button, false);
	}
}

public class EventType {
	readonly int deviceId; // TODO is this really a valid solution
	public readonly Button button;
	public readonly bool pressed;

	public EventType(Button button, bool pressed, int deviceId = -1) {
		this.button = button;
		this.pressed = pressed;
		this.deviceId = deviceId;
	}

	public int GetDeviceId() {
		return deviceId;
	}

	public override bool Equals(object obj) {
		if (obj == null || GetType() != obj.GetType()) {
			return false;
		}

		EventType that = (EventType)obj;

		return this.button == that.button && this.pressed == that.pressed;
	}

	public override int GetHashCode() {
		return this.button.GetHashCode() + 17 * pressed.GetHashCode();
	}

	public override string ToString() {
		return button.ToString() + (pressed ? "[pressed]" : "[released]");
	}
}

public class Event {
	public readonly EventType eventType;

	public Event(EventType eventType) {
		this.eventType = eventType;
	}
}

public interface EventListener {
	public void OnEvent(Event e);
}

public class ButtonState {
	public bool isPressed;
}

public class EventManager {
	/**
	 * We use this to keep track of who is interested in every event type
	 * for each controller (the id in outer dictionary).
	 */
	private Dictionary<int, Dictionary<EventType, List<EventListener>>> listeners =
	  new Dictionary<int, Dictionary<EventType, List<EventListener>>>();

	/**
	 * We use this to keep track of the state (pressed, not-pressed) of every "button" per
	 * controller (the id in the outer dictionary).
	 */
	private Dictionary<int, Dictionary<Button, ButtonState>> buttonStates =
	  new Dictionary<int, Dictionary<Button, ButtonState>>();

	// private Dictionary<EventType, List<EventListener>> listeners = new Dictionary<EventType, List<EventListener>>();
	// private Dictionary<Button, ButtonState> buttonStates = new Dictionary<Button, ButtonState>();

	public bool ListenerRegistered(int id) {
		return buttonStates.ContainsKey(id);
	}

	public void RegisterListener(int id, EventListener listener, params EventType[] eventTypes) {
		if (!buttonStates.ContainsKey(id)) {
			buttonStates.Add(id, new Dictionary<Button, ButtonState>());
		}

		if (!listeners.ContainsKey(id)) {
			listeners.Add(id, new Dictionary<EventType, List<EventListener>>());
		}

		foreach (Button button in Enum.GetValues(typeof(Button))) {
			if (!buttonStates[id].ContainsKey(button)) {
				buttonStates[id].Add(button, new ButtonState());
			}
		}

		foreach (var eventType in eventTypes) {
			List<EventListener> listeners;

			if (!this.listeners[id].ContainsKey(eventType)) {
				listeners = new List<EventListener>();
				this.listeners[id].Add(eventType, listeners);
			} else {
				listeners = this.listeners[id][eventType];
			}

			listeners.Add(listener);
		}
	}

	public void NotifyListeners(int id, EventType eventType) {
		if (!listeners[id].ContainsKey(eventType)) {
			return;
		}

		eventType = new EventType(eventType.button, eventType.pressed, id);
		Event e = new Event(eventType);

		foreach (var listener in listeners[id][eventType]) {
			if (listener == null)
				continue;
			
			listener.OnEvent(e);
		}
	}

	// Returns false when an input is disconnected
	public bool OnUpdate(AlexInput input) {
		if (input == null || input.GetDevice() == null) {
			Debug.Log($"{input} was disconnected, open the controller manager");
			return false;
		}

		int id = input.GetDeviceId();

		UpdateButtonState(id, input.LightKickPressed(), Button.BUTTON_Y);
		UpdateButtonState(id, input.MediumPunchPressed(), Button.BUTTON_A);
		UpdateButtonState(id, input.HeavyPunchPressed(), Button.BUTTON_RB);
		UpdateButtonState(id, input.LightPunchPressed(), Button.BUTTON_B);
		UpdateButtonState(id, input.MediumKickPressed(), Button.BUTTON_X);
		UpdateButtonState(id, input.HeavyKickPressed(), Button.BUTTON_LB);
		UpdateButtonState(id, input.PlusPressed(), Button.BUTTON_PLUS);
		UpdateButtonState(id, input.ScreenshotPressed(), Button.SCREENSHOT);
		UpdateButtonState(id, input.HomePressed(), Button.HOME);
		UpdateButtonState(id, input.TauntPressed(), Button.TAUNT);
		UpdateButtonState(id, input.RespectPressed(), Button.RESPECT);

		Vector2 leftStick = input.LeftStick();

		UpdateButtonState(id, leftStick.x == 0 && leftStick.y > 0, Button.NORTH);
		UpdateButtonState(id, leftStick.x > 0 && leftStick.y > 0, Button.NORTH_EAST);
		UpdateButtonState(id, leftStick.x > 0 && leftStick.y == 0, Button.EAST);
		UpdateButtonState(id, leftStick.x > 0 && leftStick.y < 0, Button.SOUTH_EAST);
		UpdateButtonState(id, leftStick.x == 0 && leftStick.y < 0, Button.SOUTH);
		UpdateButtonState(id, leftStick.x < 0 && leftStick.y < 0, Button.SOUTH_WEST);
		UpdateButtonState(id, leftStick.x < 0 && leftStick.y == 0, Button.WEST);
		UpdateButtonState(id, leftStick.x < 0 && leftStick.y > 0, Button.NORTH_WEST);

		return true;
	}

	private void UpdateButtonState(int id, bool pressed, Button button) {
		if (!buttonStates.ContainsKey(id)) {
			return;
		} else if (!buttonStates[id].ContainsKey(button)) {
			return;
		}

		if (buttonStates[id][button].isPressed != pressed) {
			if (pressed) {
				NotifyListeners(id, button.Pressed());
			} else {
				NotifyListeners(id, button.Released());
			}

			if (!buttonStates.ContainsKey(id)) {
				return;
			} else if (!buttonStates[id].ContainsKey(button)) {
				return;
			}

			buttonStates[id][button].isPressed = pressed;
		}
	}

	public void ClearRegistry() {
		listeners = new Dictionary<int, Dictionary<EventType, List<EventListener>>>();
		buttonStates = new Dictionary<int, Dictionary<Button, ButtonState>>();
	}
}