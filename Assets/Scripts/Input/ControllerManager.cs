using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ControllerInput
{
    UP_BUTTON,
    DOWN_BUTTON,
    LEFT_BUTTON,
    RIGHT_BUTTON,
    NONE,
    SELECT,
    START,
    X_BUTTON
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

    static InputButton[] currentInput = new InputButton[8];
    public static InputButton[] GetInput
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
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        //DontDestroyOnLoad(this);
        InitialiseInputArray();
        StartCoroutine(PollController());
    }

    void InitialiseInputArray()
    {
        currentInput[0] = new(ControllerInput.UP_BUTTON);
        currentInput[1] = new(ControllerInput.DOWN_BUTTON);
        currentInput[2] = new(ControllerInput.LEFT_BUTTON);
        currentInput[3] = new(ControllerInput.RIGHT_BUTTON);
        currentInput[4] = new(ControllerInput.NONE);
        currentInput[5] = new(ControllerInput.SELECT);
        currentInput[6] = new(ControllerInput.START);
        currentInput[7] = new(ControllerInput.X_BUTTON);
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
        inputMethod = GetCurrentInputMethod();
    }

    public struct InputButton
    {
        public ControllerInput Map;
        public bool Pressed;
        public bool JustReleased;
        public InputButton(ControllerInput NewMap)
        {
            Map = NewMap;
            Pressed = false;
            JustReleased = false;
        }
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

        if (ToReturn == CurrentInputMethod.KEYBOARD_MOUSE && inputMethod == CurrentInputMethod.CONTROLLER)
        {
            switchToKeyboardMouse?.Invoke();
        }
        else if (ToReturn == CurrentInputMethod.CONTROLLER && inputMethod == CurrentInputMethod.KEYBOARD_MOUSE)
        {
            switchToController?.Invoke();
        }

        lastMousePos = Input.mousePosition;

        return ToReturn;
    }

    List<ControllerInput> lastInput = new List<ControllerInput>();
    IEnumerator PollController()
    {
        //This checks every mapped input on the controller to see what is being pressd. Writing/clearing a list every frame was too slow,
        //so we are using a pre-populated array of every available input, which we then toggle its values accordingly. The enum maps directly to the inputs in the array so it can be used
        //to access each button by casting to an int.
        while (true)
        {
            bool HorizontalPriority = Mathf.Abs(Input.GetAxis(InputHolder.CONTROLLER_JOY_X)) > Mathf.Abs(Input.GetAxis(InputHolder.CONTROLLER_JOY_Y));
            List<ControllerInput> CurrentlyPressed = new List<ControllerInput>();
            if (Input.GetButton(InputHolder.CONTROLLER_A_BUTTON))
            {
                StartCoroutine(ConsumeInput());
                currentInput[(int)ControllerInput.SELECT].Pressed = true;
                CurrentlyPressed.Add(ControllerInput.SELECT);
            }

            if (Input.GetButton(InputHolder.CONTROLLER_START_BUTTON))
            {
                StartCoroutine(ConsumeInput());
                currentInput[(int)ControllerInput.START].Pressed = true;
                CurrentlyPressed.Add(ControllerInput.START);
            }

            if (Input.GetButton(InputHolder.CONTROLLER_X_BUTTON))
            {
                StartCoroutine(ConsumeInput());
                currentInput[(int)ControllerInput.X_BUTTON].Pressed = true;
                CurrentlyPressed.Add(ControllerInput.X_BUTTON);
            }

            //Button being pressed DOWN

            if (Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) > 0
                || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) < 0)
            {
                if ((Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) > 0 && !HorizontalPriority)
                    || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) < 0)
                {
                    StartCoroutine(ConsumeInput());
                    currentInput[(int)ControllerInput.DOWN_BUTTON].Pressed = true;
                    CurrentlyPressed.Add(ControllerInput.DOWN_BUTTON);
                }
            }

            if (Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) < 0
                || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) > 0)
            {
                if ((Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) < 0 && !HorizontalPriority)
                    || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) > 0)
                {
                    StartCoroutine(ConsumeInput());
                    currentInput[(int)ControllerInput.UP_BUTTON].Pressed = true;
                    CurrentlyPressed.Add(ControllerInput.UP_BUTTON);
                }
            }

            if (Input.GetAxis(InputHolder.CONTROLLER_JOY_X) > 0
                || Input.GetAxis(InputHolder.CONTROLLER_DPAD_X) > 0)
            {
                StartCoroutine(ConsumeInput());
                currentInput[(int)ControllerInput.RIGHT_BUTTON].Pressed = true;
                CurrentlyPressed.Add(ControllerInput.RIGHT_BUTTON);
            }

            if (Input.GetAxis(InputHolder.CONTROLLER_JOY_X) < 0
                || Input.GetAxis(InputHolder.CONTROLLER_DPAD_X) < 0)
            {
                StartCoroutine(ConsumeInput());
                currentInput[(int)ControllerInput.LEFT_BUTTON].Pressed = true;
                CurrentlyPressed.Add(ControllerInput.LEFT_BUTTON);
            }

            //Button being RELEASED

            if (Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) == 0
               || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) == 0)
            {
                if (!CurrentlyPressed.Contains(ControllerInput.DOWN_BUTTON))
                {
                    currentInput[(int)ControllerInput.DOWN_BUTTON].Pressed = false;

                    if (lastInput.Contains(ControllerInput.DOWN_BUTTON))
                    {
                        StartCoroutine(WaitForFrame(ControllerInput.DOWN_BUTTON));
                    }
                }
            }

            if (Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) == 0
                || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) == 0)
            {
                if (!CurrentlyPressed.Contains(ControllerInput.UP_BUTTON))
                {
                    currentInput[(int)ControllerInput.UP_BUTTON].Pressed = false;
                    if (lastInput.Contains(ControllerInput.UP_BUTTON))
                    {
                        StartCoroutine(WaitForFrame(ControllerInput.UP_BUTTON));
                    }
                }
            }

            if (Input.GetAxis(InputHolder.CONTROLLER_JOY_X) == 0
                || Input.GetAxis(InputHolder.CONTROLLER_DPAD_X) == 0)
            {
                if (!CurrentlyPressed.Contains(ControllerInput.RIGHT_BUTTON))
                {
                    currentInput[(int)ControllerInput.RIGHT_BUTTON].Pressed = false;

                    if (lastInput.Contains(ControllerInput.RIGHT_BUTTON))
                    {
                        StartCoroutine(WaitForFrame(ControllerInput.RIGHT_BUTTON));
                    }
                }
            }

            if (Input.GetAxis(InputHolder.CONTROLLER_JOY_X) == 0
                || Input.GetAxis(InputHolder.CONTROLLER_DPAD_X) == 0)
            {
                if (!CurrentlyPressed.Contains(ControllerInput.LEFT_BUTTON))
                {
                    currentInput[(int)ControllerInput.LEFT_BUTTON].Pressed = false;

                    if (lastInput.Contains(ControllerInput.LEFT_BUTTON))
                    {
                        StartCoroutine(WaitForFrame(ControllerInput.LEFT_BUTTON));
                    }
                }
            }

            if (!Input.GetButton(InputHolder.CONTROLLER_A_BUTTON))
            {
                if (!CurrentlyPressed.Contains(ControllerInput.SELECT))
                {
                    currentInput[(int)ControllerInput.SELECT].Pressed = false;
                }
            }

            if (!Input.GetButton(InputHolder.CONTROLLER_START_BUTTON))
            {
                if (!CurrentlyPressed.Contains(ControllerInput.START))
                {
                    currentInput[(int)ControllerInput.START].Pressed = false;
                }
            }

            if (!Input.GetButton(InputHolder.CONTROLLER_X_BUTTON))
            {
                if (!CurrentlyPressed.Contains(ControllerInput.X_BUTTON))
                {
                    currentInput[(int)ControllerInput.X_BUTTON].Pressed = false;
                }
            }

            lastInput = CurrentlyPressed;
            yield return new WaitForEndOfFrame();
        }
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

    IEnumerator WaitForFrame(ControllerInput JustReleased)
    {
        currentInput[(int)JustReleased].JustReleased = true;
        yield return new WaitForEndOfFrame();
        currentInput[(int)JustReleased].JustReleased = false;
    }
}
