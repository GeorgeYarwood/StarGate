using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TouchButton : UnityEngine.UI.Button, IPointerUpHandler, IPointerDownHandler, IDragHandler
{
    [SerializeField] ControllerInput[] toToggle = new ControllerInput[2];

    bool held = false;
    //Basically a wrapper around the controller class that toggles the bool as if it were a controller
    public override void OnPointerUp(PointerEventData EventData)
    {
        held = false;
        base.OnPointerUp(EventData);
        for(int t = 0; t < toToggle.Length; t++)
        {
            ControllerManager.GetInput[(int)toToggle[t]].Pressed = false;
            StartCoroutine(ControllerManager.Instance.WaitForFrame(toToggle[t]));
        }
    }

    public override void OnPointerDown(PointerEventData EventData)
    {
        base.OnPointerDown(EventData);
        for (int t = 0; t < toToggle.Length; t++)
        {
            ControllerManager.GetInput[(int)toToggle[t]].Pressed = true;
        }
        held = true;

        Vector2 NormalisedMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (NormalisedMousePos.y > 1.5f)
        {
            lastDirection = MoveDirection.UP;
            breakOut = false;
            StartCoroutine(ExecuteDrag());
        }
        else if(NormalisedMousePos.y < -1.5f)
        {
            lastDirection = MoveDirection.DOWN;
            breakOut = false;
            StartCoroutine(ExecuteDrag());
        }
    }

    void Update()
    {
        if (held)
        {
            for (int t = 0; t < toToggle.Length; t++)
            {
                ControllerManager.GetInput[(int)toToggle[t]].Pressed = true;
            }
        }
    }

    IEnumerator ExecuteDrag()
    {
        while (held && !breakOut)
        {
            switch (lastDirection)
            {
                case MoveDirection.UP:
                    ControllerManager.GetInput[(int)ControllerInput.UP_BUTTON].Pressed = true;
                    break;
                case MoveDirection.DOWN:
                    ControllerManager.GetInput[(int)ControllerInput.DOWN_BUTTON].Pressed = true;
                    break;
            }
            yield return null;
        }

        switch (lastDirection)
        {
            case MoveDirection.UP:
                ControllerManager.GetInput[(int)ControllerInput.UP_BUTTON].Pressed = false;
                StartCoroutine(ControllerManager.Instance.WaitForFrame(ControllerInput.UP_BUTTON));
                break;
            case MoveDirection.DOWN:
                ControllerManager.GetInput[(int)ControllerInput.DOWN_BUTTON].Pressed = false;
                StartCoroutine(ControllerManager.Instance.WaitForFrame(ControllerInput.DOWN_BUTTON));
                break;
        }
    }

    bool breakOut = false;
    MoveDirection lastDirection;
    public void OnDrag(PointerEventData EventData)
    {
        Vector2 NormalisedMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if(NormalisedMousePos.y > -1.5f && NormalisedMousePos.y < 1.5f)
        {
            breakOut = true;
            return;
        }

        if (NormalisedMousePos.y > 1.5f)
        {
            if (lastDirection != MoveDirection.UP)
            {
                ControllerManager.GetInput[(int)ControllerInput.DOWN_BUTTON].Pressed = false;
            }

            lastDirection = MoveDirection.UP;
            breakOut = false;
            StartCoroutine(ExecuteDrag());
        }
        else if (NormalisedMousePos.y < -1.5f)
        {
            if (lastDirection != MoveDirection.DOWN)
            {
                ControllerManager.GetInput[(int)ControllerInput.UP_BUTTON].Pressed = false;
            }
            lastDirection = MoveDirection.DOWN;
            breakOut = false;
            StartCoroutine(ExecuteDrag());
        }
    }
}
