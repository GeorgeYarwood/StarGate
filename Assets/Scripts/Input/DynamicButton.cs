using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicButton : MonoBehaviour
{
    [SerializeField] GameObject controllerIcon;
    [SerializeField] GameObject keyboard_mouseIcon;

    void Start()
    {
        ControllerManager.Instance.SwitchToController += (() => SwitchButtonSprite(CurrentInputMethod.CONTROLLER));
        ControllerManager.Instance.SwitchToKeyboardMouse += (() => SwitchButtonSprite(CurrentInputMethod.KEYBOARD_MOUSE));
    }

    void OnEnable()
    {
        SwitchButtonSprite(ControllerManager.InputMethod);
    }

    void SwitchButtonSprite(CurrentInputMethod NewMethod)
    {
        switch (NewMethod)
        {
            case CurrentInputMethod.KEYBOARD_MOUSE:
                keyboard_mouseIcon.SetActive(true);
                controllerIcon.SetActive(false);
                break;
            case CurrentInputMethod.CONTROLLER:
                keyboard_mouseIcon.SetActive(false);
                controllerIcon.SetActive(true);
                break;
        }
    }

    void OnDestroy()
    {
        ControllerManager.Instance.SwitchToController -= (() => SwitchButtonSprite(CurrentInputMethod.CONTROLLER));
        ControllerManager.Instance.SwitchToKeyboardMouse -= (() => SwitchButtonSprite(CurrentInputMethod.KEYBOARD_MOUSE));
    }
}
