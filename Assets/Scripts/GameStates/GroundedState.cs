using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedState : PlayState
{
    public override void OnStateEnter()
    {
        PlayerTurret.Instance.SetEnabled(true);
        base.OnStateEnter();
    }

    public override void OnStateExit()
    {
        PlayerTurret.Instance.SetEnabled(false);
        base.OnStateExit();
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void GetInput()
    {
        base.GetInput();

        if (Input.GetButton(InputHolder.MOVE_LEFT) || ControllerManager.GetInput[(int)ControllerInput.LEFT_BUTTON].Pressed)
        {
            float Amount = 1.0f;
            if (ControllerManager.GetInput[(int)ControllerInput.LEFT_BUTTON].Pressed)
            {
                Amount = ControllerManager.GetInput[(int)ControllerInput.LEFT_BUTTON].AxisAmount;
            }
            PlayerTurret.Instance.UpdatePosition(MoveDirection.LEFT, Amount);
        }

        if (Input.GetButton(InputHolder.MOVE_RIGHT) || ControllerManager.GetInput[(int)ControllerInput.RIGHT_BUTTON].Pressed)
        {
            float Amount = 1.0f;
            if (ControllerManager.GetInput[(int)ControllerInput.RIGHT_BUTTON].Pressed)
            {
                Amount = ControllerManager.GetInput[(int)ControllerInput.RIGHT_BUTTON].AxisAmount;
            }
            PlayerTurret.Instance.UpdatePosition(MoveDirection.RIGHT, Amount);
        }

        if (Input.GetButton(InputHolder.FIRE) || ControllerManager.GetInput[(int)ControllerInput.A_BUTTON].Pressed)
        {
            PlayerTurret.Instance.FireProjectile();
        }
    }

    public override Vector2 ReturnRandomSpawnPositionInRange(bool CloseToPlayer = false)
    {
        return Vector2.zero;
    }
}
