using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TouchButton : UnityEngine.UI.Button, IPointerUpHandler, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] ControllerInput[] toToggle = new ControllerInput[2];
    [SerializeField] bool listenForDrag = false;
    [SerializeField] bool consume = false;

    bool held = false;

    const float DRAG_THRESHHOLD = 0.1f;
    const float DIFF_MULT = 2.0f;

    Vector2 dragStartPos;
    //Basically a wrapper around the controller class that toggles the bool as if it were a controller
    public override void OnPointerUp(PointerEventData EventData)
    {
        held = false;
        base.OnPointerUp(EventData);
        for (int t = 0; t < toToggle.Length; t++)
        {
            ControllerManager.Instance.ReleaseButton(toToggle[t]);
        }

        if (!listenForDrag)
        {
            return;
        }

        ControllerManager.Instance.ReleaseButton(ControllerInput.UP_BUTTON);
        ControllerManager.Instance.ReleaseButton(ControllerInput.DOWN_BUTTON);
    }

    public override void OnPointerDown(PointerEventData EventData)
    {
        base.OnPointerDown(EventData);
        for (int t = 0; t < toToggle.Length; t++)
        {
            ControllerManager.Instance.PressButton(toToggle[t], consume);
        }

        if (consume)
        {
            return;
        }

        held = true;

        if (!listenForDrag)
        {
            return;
        }


        //if (CurrentMousePos.y > DRAG_THRESHHOLD)
        //{
        //    ControllerManager.Instance.PressButton(ControllerInput.UP_BUTTON, consume);
        //}
        //else if(CurrentMousePos.y < -DRAG_THRESHHOLD)
        //{
        //    ControllerManager.Instance.PressButton(ControllerInput.DOWN_BUTTON, consume);
        //}
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

    MoveDirection lastDirection;
    public void OnDrag(PointerEventData EventData)
    {
        if (!listenForDrag)
        {
            return;
        }

       


        Vector2 Delta = EventData.pressPosition - EventData.position;
        //Delta.Normalize();
        float YDelta = Mathf.Clamp((Delta.y / Screen.height) * 2.0f, -1.0f, 1.0f);
       
        Debug.Log(YDelta);

        if (Mathf.Abs(YDelta) < DRAG_THRESHHOLD)
        {
            ControllerManager.Instance.ReleaseButton(ControllerInput.DOWN_BUTTON);
            ControllerManager.Instance.ReleaseButton(ControllerInput.UP_BUTTON);
            return;
        }

        //Debug.Log(Delta.y);

        if (YDelta > 0.0f)
        {
            if (lastDirection != MoveDirection.UP)
            {
                ControllerManager.Instance.ReleaseButton(ControllerInput.DOWN_BUTTON);
            }

            lastDirection = MoveDirection.DOWN;
            ControllerManager.Instance.PressButton(ControllerInput.DOWN_BUTTON, false, Mathf.Abs(YDelta));
        }
        else if (YDelta < 0.0f)
        {
            if (lastDirection != MoveDirection.DOWN)
            {
                ControllerManager.Instance.ReleaseButton(ControllerInput.UP_BUTTON);
            }

            lastDirection = MoveDirection.UP;
            ControllerManager.Instance.PressButton(ControllerInput.UP_BUTTON, false, Mathf.Abs(YDelta));
        }
    }

    public void OnEndDrag(PointerEventData EventData) 
    {
       
    }
}
