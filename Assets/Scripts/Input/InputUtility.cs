using System;
using System.Collections.Generic;
using UnityEngine;

public static class InputUtility {
	public static List<EventType> allButtons = new List<EventType> {
		Button.BUTTON_A.Pressed(),
		Button.BUTTON_B.Pressed(),
		Button.BUTTON_X.Pressed(),
		Button.BUTTON_Y.Pressed(),
		Button.NORTH.Pressed(),
		Button.NORTH_EAST.Pressed(),
		Button.EAST.Pressed(),
		Button.SOUTH_EAST.Pressed(),
		Button.SOUTH.Pressed(),
		Button.SOUTH_WEST.Pressed(),
		Button.WEST.Pressed(),
		Button.NORTH_WEST.Pressed(),
		Button.BUTTON_RB.Pressed(),
		Button.BUTTON_LB.Pressed(),
		Button.HOME.Pressed(),
		Button.BUTTON_PLUS.Pressed(),
		Button.RESPECT.Pressed(),
		Button.TAUNT.Pressed()
	};

	public static Vector2 ButtonToCardinal(Button button) {
		Vector2 result = Vector2.zero;

		switch (button) {
			case Button.NORTH:
				result = new Vector2(0, 1);
				break;
			case Button.NORTH_EAST:
				result = new Vector2(1, 1);
				break;
			case Button.EAST:
				result = new Vector2(1, 0);
				break;
			case Button.SOUTH_EAST:
				result = new Vector2(1, -1);
				break;
			case Button.SOUTH:
				result = new Vector2(0, -1);
				break;
			case Button.SOUTH_WEST:
				result = new Vector2(-1, -1);
				break;
			case Button.WEST:
				result = new Vector2(-1, 0);
				break;
			case Button.NORTH_WEST:
				result = new Vector2(-1, 1);
				break;
			default:
				break;
		}

		return result;
	}

	public static Button CardinalToButton(Vector2 cardinal) {
		if (cardinal.x > 0) {
			if (cardinal.y > 0) {
				return Button.NORTH_EAST;
			} else if (cardinal.y < 0) {
				return Button.SOUTH_EAST;
			}

			return Button.EAST;
		} else if (cardinal.x < 0) {
			if (cardinal.y > 0) {
				return Button.NORTH_WEST;
			} else if (cardinal.y < 0) {
				return Button.SOUTH_WEST;
			}

			return Button.WEST;
		} else if (cardinal.x == 0) {
			if (cardinal.y > 0) {
				return Button.NORTH;
			} else if (cardinal.y < 0) {
				return Button.SOUTH;
			}
		}

		return Button.CENTER;
	}

	public static bool ButtonIsCardinal(Button button) {
		switch (button) {
			case Button.NORTH:
				return true;
			case Button.NORTH_EAST:
				return true;
			case Button.EAST:
				return true;
			case Button.SOUTH_EAST:
				return true;
			case Button.SOUTH:
				return true;
			case Button.SOUTH_WEST:
				return true;
			case Button.WEST:
				return true;
			case Button.NORTH_WEST:
				return true;
			default:
				break;
		}

		return false;
	}

	public static Button MirrorCardinalInput(Button button) {
		Vector2 originalCardinal = ButtonToCardinal(button);
		return CardinalToButton(new Vector2(originalCardinal.x * -1, originalCardinal.y));
	}

	public static string ButtonToString(Button button) {
		switch (button) {
			case Button.NORTH:
				return "NORTH";
			case Button.NORTH_EAST:
				return "NORTH_EAST";
			case Button.EAST:
				return "EAST";
			case Button.SOUTH_EAST:
				return "SOUTH_EAST";
			case Button.SOUTH:
				return "SOUTH";
			case Button.SOUTH_WEST:
				return "SOUTH_WEST";
			case Button.WEST:
				return "WEST";
			case Button.NORTH_WEST:
				return "NORTH_WEST";
			case Button.BUTTON_A:
				return "BUTTON_A";
			case Button.BUTTON_B:
				return "BUTTON_B";
			case Button.BUTTON_X:
				return "BUTTON_X";
			case Button.BUTTON_Y:
				return "BUTTON_Y";
			case Button.BUTTON_LB:
				return "BUTTON_LB";
			case Button.BUTTON_RB:
				return "BUTTON_RB";
			default:
				return "DEFAULT";
		}
	}

	public static string ButtonToString(Button button, bool onKeyboard) {
		switch (button) {
			case Button.NORTH:
				return onKeyboard ? "W" : "NORTH";
			case Button.NORTH_EAST:
				return onKeyboard ? "WD" : "NORTH_EAST";
			case Button.EAST:
				return onKeyboard ? "D" : "EAST";
			case Button.SOUTH_EAST:
				return onKeyboard ? "SD" : "SOUTH_EAST";
			case Button.SOUTH:
				return onKeyboard ? "S" : "SOUTH";
			case Button.SOUTH_WEST:
				return onKeyboard ? "SA" : "SOUTH_WEST";
			case Button.WEST:
				return onKeyboard ? "A" : "WEST";
			case Button.NORTH_WEST:
				return onKeyboard ? "WA" : "NORTH_WEST";
			case Button.BUTTON_A:
				return onKeyboard ? "J" : "BUTTON_A";
			case Button.BUTTON_B:
				return onKeyboard ? "K" : "BUTTON_B";
			case Button.BUTTON_X:
				return onKeyboard ? "L" : "BUTTON_X";
			case Button.BUTTON_Y:
				return onKeyboard ? "I" : "BUTTON_Y";
			case Button.BUTTON_LB:
				return onKeyboard ? "O" : "BUTTON_LB";
			case Button.BUTTON_RB:
				return onKeyboard ? "P" : "BUTTON_RB";
			case Button.RESPECT:
				return onKeyboard ? "X" : "RESPECT";
			case Button.TAUNT:
				return onKeyboard ? "Z" : "TAUNT";
			default:
				return "DEFAULT";
		}
	}
}