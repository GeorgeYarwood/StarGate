using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TouchButton : UnityEngine.UI.Button, IPointerUpHandler, IPointerDownHandler, IDragHandler
{
    [SerializeField] ControllerInput[] toToggle = new ControllerInput[2];
    const float DEAD_ZONE = 2.0f;

    bool held = false;
    //Basically a wrapper around the controller class that toggles the bool as if it were a controller
    public override void OnPointerUp(PointerEventData EventData)
    {
        held = false;
        base.OnPointerUp(EventData);
        for(int t = 0; t < toToggle.Length; t++)
        {
            ControllerManager.GetInput[(int)toToggle[t]].Pressed = false;
            ControllerManager.Instance.WaitForFrame(toToggle[t]);
        }

        ////Hack for now
        //StopAllCoroutines();
        //ControllerManager.GetInput[(int)ControllerInput.UP_BUTTON].Pressed = false;
        //ControllerManager.GetInput[(int)ControllerInput.UP_BUTTON].JustReleased = false;
        //ControllerManager.GetInput[(int)ControllerInput.DOWN_BUTTON].Pressed = false;
        //ControllerManager.GetInput[(int)ControllerInput.DOWN_BUTTON].JustReleased = false;
    }

    public override void OnPointerDown(PointerEventData EventData)
    {
        base.OnPointerDown(EventData);
        for (int t = 0; t < toToggle.Length; t++)
        {
            ControllerManager.GetInput[(int)toToggle[t]].Pressed = true;
        }
        held = true;
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

    bool block = false;
    IEnumerator ExecuteDrag()
    {
        while (held)
        {
            //block = true;
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
                ControllerManager.Instance.WaitForFrame(ControllerInput.UP_BUTTON);
                break;
            case MoveDirection.DOWN:
                ControllerManager.GetInput[(int)ControllerInput.DOWN_BUTTON].Pressed = false;
                ControllerManager.Instance.WaitForFrame(ControllerInput.DOWN_BUTTON);
                break;
        }
        block = false;
    }

    MoveDirection lastDirection;
    public void OnDrag(PointerEventData EventData)
    {
        if (block) return;
        if(EventData.delta.y > DEAD_ZONE)
        {
            if (lastDirection != MoveDirection.UP)
            {
                ControllerManager.GetInput[(int)ControllerInput.DOWN_BUTTON].Pressed = false;
            }

            lastDirection = MoveDirection.UP;
            StartCoroutine(ExecuteDrag());

        }
        else if(EventData.delta.y < 0.0f - DEAD_ZONE)
        {
            if (lastDirection != MoveDirection.DOWN)
            {
                ControllerManager.GetInput[(int)ControllerInput.UP_BUTTON].Pressed = false;
            }
            lastDirection = MoveDirection.DOWN;
            StartCoroutine(ExecuteDrag());

        }

    }
}
