using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingState : GameStateBase
{
    [SerializeField] PlayerShip playerShip;

    //Input buttons
    const string MOVE_LEFT = "MoveLeft";
    const string MOVE_RIGHT = "MoveRight";
    const string MOVE_UP = "MoveUp";
    const string MOVE_DOWN = "MoveDown";
    const string FIRE = "Fire";

    public override void OnStateEnter()
    {

    }

    public override void OnStateExit()
    {
        
    }

    public override void Tick()
    {
        GetInput();
        TrackPlayerWithCamera();
    }

    void TrackPlayerWithCamera()
    {
        CameraController.Instance.UpdatePosition(playerShip.transform.position, TrackAxis.X_AXIS);
    }

    void GetInput()
    {
        if (Input.GetButton(MOVE_UP))
        {
            playerShip.UpdatePosition(MoveDirection.UP);
        }
        if (Input.GetButton(MOVE_DOWN))
        {
            playerShip.UpdatePosition(MoveDirection.DOWN);
        }
        if (Input.GetButton(MOVE_LEFT))
        {
            playerShip.UpdatePosition(MoveDirection.LEFT);
        }
        if (Input.GetButton(MOVE_RIGHT))
        {
            playerShip.UpdatePosition(MoveDirection.RIGHT);
        }
        if (Input.GetButton(FIRE))
        {
            playerShip.FireProjectile();
        }
    }
}
