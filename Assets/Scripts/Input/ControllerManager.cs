using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if !FOR_MOBILE
using XInputDotNetPure;
#endif
public enum ControllerInput
{
    UP_BUTTON,
    DOWN_BUTTON,
    LEFT_BUTTON,
    RIGHT_BUTTON,
    NONE,
    START,
    X_BUTTON,
    B_BUTTON,
    Y_BUTTON,
    A_BUTTON
}

public enum CurrentInputMethod
{
    KEYBOARD_MOUSE,
    CONTROLLER
}

public class ControllerManager : MonoBehaviour
{
    const string MAIN_MENU = "MainMenu";

    static ControllerManager instance;
    public static ControllerManager Instance
    {
        get { return instance; }
    }

    static InputButton[] currentInput = new InputButton[10];
    public static InputButton[] GetInput
    {
        get { return currentInput; }
        set { currentInput = value; } //Hacky but we need to write to it for touch
    }

    static CurrentInputMethod inputMethod;
    public static CurrentInputMethod InputMethod
    {
        get { return inputMethod; }
    }

    const float WAIT_BETWEEN_INPUTS = 0.25f;

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

    const float VIBRATION_PULSE_TIMER = 0.15f;
    const float VIBRATION_PULSE_AMOUNT= 0.4f;

    void Awake()
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

        switchToKeyboardMouse += OnSwitchToKeyboardMouse;
        SwitchToController += OnSwitchToController;
    }

    void OnDestroy()
    {
        switchToKeyboardMouse -= OnSwitchToKeyboardMouse;
        SwitchToController -= OnSwitchToController;
    }

    void InitialiseInputArray()
    {
        currentInput[0] = new(ControllerInput.UP_BUTTON);
        currentInput[1] = new(ControllerInput.DOWN_BUTTON);
        currentInput[2] = new(ControllerInput.LEFT_BUTTON);
        currentInput[3] = new(ControllerInput.RIGHT_BUTTON);
        currentInput[4] = new(ControllerInput.NONE);
        currentInput[5] = new(ControllerInput.START);
        currentInput[6] = new(ControllerInput.X_BUTTON);
        currentInput[7] = new(ControllerInput.B_BUTTON);
        currentInput[8] = new(ControllerInput.Y_BUTTON);
        currentInput[9] = new(ControllerInput.A_BUTTON);
    }

    IEnumerator ConsumeInput(ControllerInput Button)
    {
        yield return new WaitForEndOfFrame();
        currentInput[(int)Button].Consumed = true; //To preserve compatibilty we always return the current controller input, and it's up to each script to check this value if it wants to respect consumed inputs
        yield return new WaitForSeconds(WAIT_BETWEEN_INPUTS);
        currentInput[(int)Button].Consumed = false;
    }

    public bool AnyConsumed() 
    {
        for(int i = 0; i < currentInput.Length; i++)
        {
            if (currentInput[i].Consumed)
            {
                return true;
            }
        }

        return false;
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
        public bool Consumed;
        public InputButton(ControllerInput NewMap)
        {
            Map = NewMap;
            Pressed = false;
            JustReleased = false;
            Consumed = false;
        }
    }

    void OnSwitchToKeyboardMouse()
    {
        //Main menu isn't a state so we need to hardcode it
        if (SceneManager.GetActiveScene().name == MAIN_MENU)
        {
            Cursor.visible = true;
            return;
        }

        Cursor.visible = GameController.Instance.GetCurrentGameState.AllowCursorVisible;
        Cursor.lockState = GameController.Instance.GetCurrentGameState.AllowCursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    void OnSwitchToController()
    {
        Cursor.visible = false;
    }

    bool AnyKeyboardMouseInput() //Input.anyKey was being a dick so I just wrote my own
    {
        return Input.GetButton(InputHolder.MOVE_UP) || Input.GetButton(InputHolder.MOVE_DOWN) || Input.GetButton(InputHolder.MOVE_LEFT) || Input.GetButton(InputHolder.MOVE_RIGHT)
            || Input.GetButton(InputHolder.FIRE) || Input.mousePosition != lastMousePos;
    }

    public void VibrateController()
    {
        if (!GameController.VibrationEnabled)
        {
            return;
        }
        StartCoroutine(HandleVibration());
    }

    IEnumerator HandleVibration()
    {
 #if !FOR_MOBILE
        GamePad.SetVibration(0, VIBRATION_PULSE_AMOUNT, VIBRATION_PULSE_AMOUNT); //Unity doesn't have this?? So need to use an XInput library
#endif
        yield return new WaitForSeconds(VIBRATION_PULSE_TIMER);
#if !FOR_MOBILE
        GamePad.SetVibration(0, 0.0f, 0.0f);
#endif
    }

    Vector3 lastMousePos = Vector3.zero;
    CurrentInputMethod GetCurrentInputMethod()
    {
        CurrentInputMethod ToReturn = inputMethod;
        if (AnyKeyboardMouseInput())
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
#if FOR_MOBILE
        while (1 == 2)  //Mobile input is a bit of hack and modifies each input instead of this coroutine, so we don't run it on mobile
#else
        while (true)
#endif
        {
            bool HorizontalPriority = Mathf.Abs(Input.GetAxis(InputHolder.CONTROLLER_JOY_X)) > Mathf.Abs(Input.GetAxis(InputHolder.CONTROLLER_JOY_Y));
            List<ControllerInput> CurrentlyPressed = new List<ControllerInput>();
            if (Input.GetButton(InputHolder.CONTROLLER_A_BUTTON))
            {
                StartCoroutine(ConsumeInput(ControllerInput.A_BUTTON));
                currentInput[(int)ControllerInput.A_BUTTON].Pressed = true;
                CurrentlyPressed.Add(ControllerInput.A_BUTTON);
            }

            if (Input.GetButton(InputHolder.CONTROLLER_B_BUTTON))
            {
                StartCoroutine(ConsumeInput(ControllerInput.B_BUTTON));
                currentInput[(int)ControllerInput.B_BUTTON].Pressed = true;
                CurrentlyPressed.Add(ControllerInput.B_BUTTON);
            }

            if (Input.GetButton(InputHolder.CONTROLLER_X_BUTTON))
            {
                StartCoroutine(ConsumeInput(ControllerInput.X_BUTTON));
                currentInput[(int)ControllerInput.X_BUTTON].Pressed = true;
                CurrentlyPressed.Add(ControllerInput.X_BUTTON);
            }

            if (Input.GetButton(InputHolder.CONTROLLER_Y_BUTTON))
            {
                StartCoroutine(ConsumeInput(ControllerInput.Y_BUTTON));
                currentInput[(int)ControllerInput.Y_BUTTON].Pressed = true;
                CurrentlyPressed.Add(ControllerInput.Y_BUTTON);
            }

            if (Input.GetButton(InputHolder.CONTROLLER_START_BUTTON))
            {
                StartCoroutine(ConsumeInput(ControllerInput.START));
                currentInput[(int)ControllerInput.START].Pressed = true;
                CurrentlyPressed.Add(ControllerInput.START);
            }

            //Button being pressed DOWN

            if (Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) > 0
                || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) < 0)
            {
                if ((Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) > 0 && !HorizontalPriority)
                    || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) < 0)
                {
                    StartCoroutine(ConsumeInput(ControllerInput.DOWN_BUTTON));
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
                    StartCoroutine(ConsumeInput(ControllerInput.UP_BUTTON));
                    currentInput[(int)ControllerInput.UP_BUTTON].Pressed = true;
                    CurrentlyPressed.Add(ControllerInput.UP_BUTTON);
                }
            }

            if (Input.GetAxis(InputHolder.CONTROLLER_JOY_X) > 0
                || Input.GetAxis(InputHolder.CONTROLLER_DPAD_X) > 0)
            {
                StartCoroutine(ConsumeInput(ControllerInput.RIGHT_BUTTON));
                currentInput[(int)ControllerInput.RIGHT_BUTTON].Pressed = true;
                CurrentlyPressed.Add(ControllerInput.RIGHT_BUTTON);
            }

            if (Input.GetAxis(InputHolder.CONTROLLER_JOY_X) < 0
                || Input.GetAxis(InputHolder.CONTROLLER_DPAD_X) < 0)
            {
                StartCoroutine(ConsumeInput(ControllerInput.LEFT_BUTTON));
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

           

            if (!Input.GetButton(InputHolder.CONTROLLER_START_BUTTON))
            {
                if (!CurrentlyPressed.Contains(ControllerInput.START))
                {
                    currentInput[(int)ControllerInput.START].Pressed = false;
                }
            }

            if (!Input.GetButton(InputHolder.CONTROLLER_A_BUTTON))
            {
                if (!CurrentlyPressed.Contains(ControllerInput.A_BUTTON))
                {
                    currentInput[(int)ControllerInput.A_BUTTON].Pressed = false;
                }
            }

            if (!Input.GetButton(InputHolder.CONTROLLER_B_BUTTON))
            {
                if (!CurrentlyPressed.Contains(ControllerInput.B_BUTTON))
                {
                    currentInput[(int)ControllerInput.B_BUTTON].Pressed = false;
                }
            }

            if (!Input.GetButton(InputHolder.CONTROLLER_X_BUTTON))
            {
                if (!CurrentlyPressed.Contains(ControllerInput.X_BUTTON))
                {
                    currentInput[(int)ControllerInput.X_BUTTON].Pressed = false;
                }
            }

            if (!Input.GetButton(InputHolder.CONTROLLER_Y_BUTTON))
            {
                if (!CurrentlyPressed.Contains(ControllerInput.Y_BUTTON))
                {
                    currentInput[(int)ControllerInput.Y_BUTTON].Pressed = false;
                }
            }

            lastInput = CurrentlyPressed;
            yield return new WaitForEndOfFrame();
        }
    }

    public bool AnyControllerInput()
    {
        return Input.GetAxis(InputHolder.CONTROLLER_JOY_Y) != 0 || Input.GetAxis(InputHolder.CONTROLLER_DPAD_Y) != 0
            || Input.GetAxis(InputHolder.CONTROLLER_JOY_X) != 0 || Input.GetAxis(InputHolder.CONTROLLER_DPAD_X) != 0
            || Input.GetButton(InputHolder.CONTROLLER_A_BUTTON) || Input.GetButton(InputHolder.CONTROLLER_B_BUTTON) 
            || Input.GetButton(InputHolder.CONTROLLER_X_BUTTON) || Input.GetButton(InputHolder.CONTROLLER_Y_BUTTON) 
            || Input.GetButton(InputHolder.CONTROLLER_START_BUTTON);
    }

    public IEnumerator WaitForFrame(ControllerInput JustReleased)
    {
        currentInput[(int)JustReleased].JustReleased = true;
        yield return new WaitForEndOfFrame();
        currentInput[(int)JustReleased].JustReleased = false;
    }
}
