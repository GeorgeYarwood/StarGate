using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum NavigationDirection
{
    HORIZONTAL,
    VERTICAL
}

//Used for navigation UI on a controller, handles highlights,interactions, etc.
public class MenuControllerNav : MonoBehaviour
{
    const float SLIDER_ADJUST_VAL = 5.0f;

    [Serializable]
    public struct NavigationArea
    {
        public NavigationItem[] AreaNavSelectables;
        public NavigationDirection AreaNavType;
        public NavigationDirection ExitDirection;
    }

    int selectionIndex = 0;
    int currentAreaIndex = 0;
    [SerializeField] NavigationArea[] allAreas;

    [SerializeField] GameObject previousNav; //If this is a nested menu, pass the parent/original navigation controller here to disable/enable

    NavigationArea currentArea;

    void Start()
    {
        if (previousNav)
        {
            previousNav.SetActive(false);
        }

        currentArea = allAreas[currentAreaIndex];
        ControllerManager.Instance.SwitchToKeyboardMouse += (() => HighlightSelected(false));
        ControllerManager.Instance.SwitchToController += (() => HighlightSelected(true));

        //if(ControllerManager.InputMethod == CurrentInputMethod.KEYBOARD_MOUSE)
        //{
        //    HighlightSelected(false);
        //}
        //else if(ControllerManager.InputMethod == CurrentInputMethod.CONTROLLER)
        //{
        //    HighlightSelected(true);
        //}
    }

    void OnDisable()
    {
        if (previousNav)
        {
            previousNav.SetActive(true);
        }

        ControllerManager.Instance.SwitchToKeyboardMouse -= (() => HighlightSelected(false));
        ControllerManager.Instance.SwitchToController -= (() => HighlightSelected(true));
    }

    void Update()
    {
        TraverseAreas();
    }

    void HighlightSelected(bool Setting)
    {
        currentArea.AreaNavSelectables[selectionIndex].SetHighlight(Setting);
    }

    void DisableCurrentAreaHighlights()
    {
        for (int h = 0; h < currentArea.AreaNavSelectables.Length; h++)
        {
            currentArea.AreaNavSelectables[h].SetHighlight(false);
        }
    }

    void SwitchToArea(int NewArea)
    {
        DisableCurrentAreaHighlights();
        currentAreaIndex = NewArea;
        currentArea = allAreas[currentAreaIndex];
        selectionIndex = 0;
        HighlightSelected(true);
    }

    void TraverseAreas()
    {
        if (ControllerManager.Consumed)
        {
            return;
        }

        switch (ControllerManager.GetInput)
        {
            case ControllerInput.UP_BUTTONDOWN:
                if(currentArea.AreaNavType == NavigationDirection.VERTICAL && selectionIndex - 1 >= 0)
                {
                    if (currentArea.AreaNavSelectables[selectionIndex - 1].gameObject.activeInHierarchy)
                    {
                        HighlightSelected(false);
                        selectionIndex--;
                        HighlightSelected(true);
                    }
                }
                else if(currentArea.ExitDirection == NavigationDirection.VERTICAL)
                {
                    SwitchToArea(currentAreaIndex - 1);
                }
                break;
            case ControllerInput.DOWN_BUTTONDOWN:
                if(currentArea.AreaNavType == NavigationDirection.VERTICAL && selectionIndex < currentArea.AreaNavSelectables.Length - 1)
                {
                    if (currentArea.AreaNavSelectables[selectionIndex + 1].gameObject.activeInHierarchy)
                    {
                        HighlightSelected(false);
                        selectionIndex++;
                        HighlightSelected(true);
                    }
                }
                else if (currentArea.ExitDirection == NavigationDirection.VERTICAL)
                {
                    SwitchToArea(currentAreaIndex + 1);
                }
                break;
            case ControllerInput.LEFT_BUTTONDOWN:
                if (currentArea.AreaNavType == NavigationDirection.VERTICAL && InteractingWithSlider())
                {
                    InteractingWithSlider().value -= InteractingWithSlider().maxValue / SLIDER_ADJUST_VAL;
                }
                else if (currentArea.AreaNavType == NavigationDirection.HORIZONTAL && selectionIndex - 1 >= 0)
                {
                    if (currentArea.AreaNavSelectables[selectionIndex - 1].gameObject.activeInHierarchy)
                    {
                        HighlightSelected(false);
                        selectionIndex--;
                        HighlightSelected(true);
                    }
                }
                else if (currentArea.ExitDirection == NavigationDirection.HORIZONTAL && selectionIndex == 0
                    && currentAreaIndex > 0)
                {
                    SwitchToArea(currentAreaIndex - 1);
                }
                break;
            case ControllerInput.RIGHT_BUTTONDOWN:
                if (currentArea.AreaNavType == NavigationDirection.VERTICAL && InteractingWithSlider())
                {
                    InteractingWithSlider().value += InteractingWithSlider().maxValue / SLIDER_ADJUST_VAL;
                }
                else if (currentArea.AreaNavType == NavigationDirection.HORIZONTAL && selectionIndex < currentArea.AreaNavSelectables.Length - 1)
                {
                    if (currentArea.AreaNavSelectables[selectionIndex + 1].gameObject.activeInHierarchy)
                    {
                        HighlightSelected(false);
                        selectionIndex++;
                        HighlightSelected(true);
                    }
                }
                else if (currentArea.ExitDirection == NavigationDirection.HORIZONTAL && selectionIndex == currentArea.AreaNavSelectables.Length - 1
                    && currentAreaIndex < allAreas.Length - 1)
                {
                    SwitchToArea(currentAreaIndex + 1);
                }
                break;
            case ControllerInput.SELECT:
                InteractWithSelectable();
                break;
        }
    }

    void InteractWithSelectable()
    {
        (currentArea.AreaNavSelectables[selectionIndex].ToInteractWith as Button).onClick.Invoke();
    }

    Slider InteractingWithSlider()
    {
        return currentArea.AreaNavSelectables[selectionIndex].ToInteractWith as Slider;
    }
}
