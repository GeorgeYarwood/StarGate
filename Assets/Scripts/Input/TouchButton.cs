using UnityEngine;
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
            ControllerManager.Instance.WaitForFrame(toToggle[t]);
        }

        //Hack for now
        ControllerManager.GetInput[(int)ControllerInput.UP_BUTTON].Pressed = false;
        ControllerManager.GetInput[(int)ControllerInput.DOWN_BUTTON].Pressed = false;
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

    MoveDirection lastDirection;
    public void OnDrag(PointerEventData EventData)
    {
        if(EventData.delta.y > DEAD_ZONE)
        {
            if (lastDirection != MoveDirection.UP)
            {
                ControllerManager.GetInput[(int)ControllerInput.DOWN_BUTTON].Pressed = false;
            }
            ControllerManager.GetInput[(int)ControllerInput.UP_BUTTON].Pressed = true;
            lastDirection = MoveDirection.UP;
        }
        else if(EventData.delta.y < DEAD_ZONE)
        {
            if (lastDirection != MoveDirection.DOWN)
            {
                ControllerManager.GetInput[(int)ControllerInput.UP_BUTTON].Pressed = false;
            }
            ControllerManager.GetInput[(int)ControllerInput.DOWN_BUTTON].Pressed = true;
            lastDirection = MoveDirection.DOWN;
        }
    }
}
