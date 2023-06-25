using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AlexianInput : MonoBehaviour
{
    public PlayerInput playerInput;
    AlexianButton northButton;
    AlexianButton shootButton;
    AlexianButton westButton;
    AlexianButton pauseButton;

    Camera mainCam;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        InitializeNewInput();
        mainCam = Camera.main;
    }

    private void LateUpdate()
    {
        RefreshButtonData();
    }

    private void InitializeNewInput()
    {
        northButton = new AlexianButton();
        shootButton = new AlexianButton();
        westButton = new AlexianButton();
        pauseButton = new AlexianButton();

        AddAlexianActionViewer(northButton, "NorthButton");
        AddAlexianActionViewer(shootButton, "Shoot");
        AddAlexianActionViewer(westButton, "WestButton");
        AddAlexianActionViewer(pauseButton, "PauseButton");
    }

    private void RefreshButtonData()
    {
        northButton.lastPressed = northButton.pressed;
        shootButton.lastPressed = shootButton.pressed;
        westButton.lastPressed = westButton.pressed;
        pauseButton.lastPressed = pauseButton.pressed;
    }

    public Vector2 LeftStick()
    {
        Vector2 result = Vector2.zero;
        result = playerInput.actions.FindActionMap("Player").FindAction("LeftStick").ReadValue<Vector2>();
        return result;
    }

    public Vector2 RightStick()
    {
        Vector2 result = Vector2.zero;
        result = playerInput.actions.FindActionMap("Player").FindAction("RightStick").ReadValue<Vector2>();
        return result;
    }

    public Vector2 MouseScroll()
    {
        Vector2 result = Vector2.zero;
        result = playerInput.actions.FindActionMap("Player").FindAction("Scroll").ReadValue<Vector2>();
        return result;
    }

    public Vector2 MouseWorldPosition()
    {
        Vector2 result = Vector2.zero;
        result = mainCam.ScreenToWorldPoint(playerInput.actions.FindActionMap("Player").FindAction("MousePosition").ReadValue<Vector2>());
        return result;
    }

    public bool NorthButtonDown()
    {
        return AlexianButtonDown(northButton);
    }

    public bool NorthButtonUp()
    {
        return AlexianButtonUp(northButton);
    }

    public bool ShootDown()
    {
        return AlexianButtonDown(shootButton);
    }

    public bool ShootUp()
    {
        return AlexianButtonUp(shootButton);
    }

    public bool ShootIsDown()
    {
        return AlexianButtonHold(shootButton);
    }

    public bool WestButtonDown()
    {
        return AlexianButtonDown(westButton);
    }

    public bool PauseButtonDown()
    {
        return AlexianButtonDown(pauseButton);
    }

    public bool AlexianButtonUp(AlexianButton button)
    {
        if (button.pressed != button.lastPressed)
        {
            if (!button.pressed)
            {
                return true;
            }
        }

        return false;
    }

    public bool AlexianButtonDown(AlexianButton button)
    {
        if (button.pressed != button.lastPressed)
        {
            if (button.pressed)
            {
                return true;
            }
        }

        return false;
    }

    public bool AlexianButtonHold(AlexianButton button)
    {
        return button.pressed;
    }

    public void AddAlexianActionViewer(AlexianButton button, string actionName)
    {
        playerInput.actions.FindActionMap("Player").FindAction(actionName).performed += ctx => button.pressed = true;
        playerInput.actions.FindActionMap("Player").FindAction(actionName).canceled += ctx => button.pressed = false;
    }
}

[System.Serializable]
public class AlexianButton
{
    public bool lastPressed = false;
    public bool pressed = false;
}