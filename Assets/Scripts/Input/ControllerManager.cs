using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControllerInput
{
    UP_BUTTONDOWN,
    DOWN_BUTTONDOWN,
    LEFT_BUTTONDOWN,
    RIGHT_BUTTONDOWN,
    NONE,
    SELECT,
    START,

    UP_BUTTONUP,
    DOWN_BUTTONUP,
    LEFT_BUTTONUP,
    RIGHT_BUTTONUP,
}

public enum CurrentInputMethod
{
    KEYBOARD_MOUSE,
    CONTROLLER
}

public class ControllerManager : MonoBehaviour
{
    static ControllerManager instance;
    public static ControllerManager Instance
    {
        get { return instance; }
    }

    static ControllerInput currentInput;
    public static ControllerInput GetInput
    {
        get { return currentInput; }
    }

    static CurrentInputMethod inputMethod;
    public static CurrentInputMethod InputMethod
    {
        get { return inputMethod; }
    }

    const float WAIT_BETWEEN_INPUTS = 0.25f;
    static bool consumed = false;
    static public bool Consumed
    {
        get { return consumed; }
    }

    Action switchToKeyboardMouse;
    public Action SwitchToKeyboardMouse
    {
        get { return switchToKeyboardMouse; }
        set { switchToKeyboardMouse = value; }
    }
    Action switchToController;
    public Action SwitchToController
    {
        get { return switchToController; }
        set { switchToController = value; }
    }

    void Start()
    {
        if (instance != null)
        {
            Destroy(instance);
        }

        instance = this;

        DontDestroyOnLoad(this);
    }

    IEnumerator ConsumeInput()
    {
        yield return new WaitForEndOfFrame();
        consumed = true; //To preserve compatibilty we always return the current controller input, and it's up to each script to check this value if it wants to respect consumed inputs
        yield return new WaitForSeconds(WAIT_BETWEEN_INPUTS);
        consumed = false;
    }

    void Update()
    {
        currentInput = PollController();
        inputMethod = GetCurrentInputMethod();
    }

    Vector2 lastMousePos = Vector2.zero;
    CurrentInputMethod GetCurrentInputMethod()
    {
        CurrentInputMethod ToReturn = inputMethod;
        if (Input.anyKey || Input.mousePosition != (Vector3)lastMousePos)
        {
            ToReturn = CurrentInputMethod.KEYBOARD_MOUSE;
        }
        else if (AnyControllerInput())
        {
            ToReturn = CurrentInputMethod.CONTROLLER;
        }

        if(ToReturn == CurrentInputMethod.KEYBOARD_MOUSE && inputMethod == CurrentInputMethod.CONTROLLER)
        {
            switchToKeyboardMouse.Invoke();
        }
        else if(ToReturn == CurrentInputMethod.CONTROLLER && inputMethod == CurrentInputMethod.KEYBOARD_MOUSE)
        {
            switchToController.Invoke();
        }

        lastMousePos = Input.mousePosition;

        return ToReturn;
    }


    ControllerInput lastInput;
    ControllerInput PollController()
    {
        bool HorizontalPriority = Mathf.Abs(Input.GetAxis(InputHolder.CONTROLLER_JOY_X)) > Mathf.Abs(Input.GetAxis(InputHolder.CONTROLLER_JOY_Y));
        if (Input.GetButton(InputHolder.CONTROLLER_A_BUTTON))
        {
            StartCoroutine(ConsumeInput());
            return ControllerInput.SELECT;
        }

        if (Input.GetButtonDown(InputHolder.CONTROLLER_START_BUTTON))
        {
            StartCoroutine(ConsumeInput());
            return ControllerInput.START;
        }

        //Button being pressed DOWN

        if (Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) > 0
            || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) < 0)
        {
            if ((Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) > 0 && !HorizontalPriority)
                || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) < 0)
            {
                StartCoroutine(ConsumeInput());
                lastInput = ControllerInput.DOWN_BUTTONDOWN;
                return ControllerInput.DOWN_BUTTONDOWN;
            }
        }

        if (Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) < 0
            || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) > 0)
        {
            if ((Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) < 0 && !HorizontalPriority)
                || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) > 0)
            {
                StartCoroutine(ConsumeInput());
                lastInput = ControllerInput.UP_BUTTONDOWN;
                return ControllerInput.UP_BUTTONDOWN;
            }
        }

        if (Input.GetAxis(InputHolder.CONTROLLER_JOY_X) > 0
            || Input.GetAxis(InputHolder.CONTROLLER_DPAD_X) > 0)
        {
            StartCoroutine(ConsumeInput());
            lastInput = ControllerInput.RIGHT_BUTTONDOWN;
            return ControllerInput.RIGHT_BUTTONDOWN;
        }

        if (Input.GetAxis(InputHolder.CONTROLLER_JOY_X) < 0
            || Input.GetAxis(InputHolder.CONTROLLER_DPAD_X) < 0)
        {
            StartCoroutine(ConsumeInput());
            lastInput = ControllerInput.LEFT_BUTTONDOWN;
            return ControllerInput.LEFT_BUTTONDOWN;
        }

        //Button being pressed UP

        if (Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) == 0
           || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) == 0)
        {
            if (lastInput == ControllerInput.DOWN_BUTTONDOWN)
            {
                StartCoroutine(WaitForFrame());
                return ControllerInput.DOWN_BUTTONUP;
            }
        }

        if (Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) == 0
            || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) == 0)
        {
            if (lastInput == ControllerInput.UP_BUTTONDOWN)
            {
                StartCoroutine(WaitForFrame());
                return ControllerInput.UP_BUTTONUP;
            }
        }

        if (Input.GetAxis(InputHolder.CONTROLLER_JOY_X) == 0
            || Input.GetAxis(InputHolder.CONTROLLER_DPAD_X) == 0)
        {
            if (lastInput == ControllerInput.RIGHT_BUTTONDOWN)
            {
                StartCoroutine(WaitForFrame());
                return ControllerInput.RIGHT_BUTTONUP;
            }
        }

        if (Input.GetAxis(InputHolder.CONTROLLER_JOY_X) == 0
            || Input.GetAxis(InputHolder.CONTROLLER_DPAD_X) == 0)
        {
            if (lastInput == ControllerInput.LEFT_BUTTONDOWN)
            {
                StartCoroutine(WaitForFrame());
                return ControllerInput.LEFT_BUTTONUP;
            }
        }

        return ControllerInput.NONE;
    }

    public bool AnyControllerInput()
    {
        if (Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) != 0 || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) != 0
            || Input.GetAxis(InputHolder.CONTROLLER_JOY_X) != 0 || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) != 0)
        {
            return true;
        }

        return false;
    }

    IEnumerator WaitForFrame()
    {
        yield return new WaitForEndOfFrame();
        lastInput = ControllerInput.NONE;
    }
}
