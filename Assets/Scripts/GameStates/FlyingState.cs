using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlyingState : PlayState
{
    public override void OnStateEnter()
    {
        PlayerShip.Instance.SetEnabled(true);
        base.OnStateEnter();
    }
   
    public override void OnStateExit()
    {
        PlayerShip.Instance.SetEnabled(false);
        base.OnStateExit();
    }

    public override void Tick()
    {
        base.Tick();
        TrackPlayerWithCamera();
    }

    public override void EndLevel(bool Debug = false)
    {
        PlayerShip.Invincible = true;
        base.EndLevel(Debug);
    }

    void TrackPlayerWithCamera()
    {
        CameraController.Instance.UpdatePosition(PlayerShip.Instance.transform.position, TrackAxis.X_AXIS);
    }

    public override void GetInput()
    {
        base.GetInput();

        if (Input.GetButton(InputHolder.MOVE_UP) || ControllerManager.GetInput[(int)ControllerInput.UP_BUTTON].Pressed)
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.UP);
        }
        if (Input.GetButton(InputHolder.MOVE_DOWN) || ControllerManager.GetInput[(int)ControllerInput.DOWN_BUTTON].Pressed)
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.DOWN);
        }
        if (Input.GetButton(InputHolder.MOVE_LEFT) || ControllerManager.GetInput[(int)ControllerInput.LEFT_BUTTON].Pressed) 
        { 
            PlayerShip.Instance.UpdatePosition(MoveDirection.LEFT);
        }

        if (Input.GetButton(InputHolder.MOVE_RIGHT) || ControllerManager.GetInput[(int)ControllerInput.RIGHT_BUTTON].Pressed)
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.RIGHT);
        }
        if (Input.GetButton(InputHolder.FIRE) || ControllerManager.GetInput[(int)ControllerInput.A_BUTTON].Pressed)
        {
            PlayerShip.Instance.FireProjectile();
        }

        if (Input.GetButtonDown(InputHolder.DEPLOY_BOMB) || (ControllerManager.GetInput[(int)ControllerInput.Y_BUTTON].Pressed)
            && !(ControllerManager.GetInput[(int)ControllerInput.Y_BUTTON].Consumed))
        {
            //Fire bomb...
            AbilityController.Instance.TryFireBomb();
        }
        
        //Listen for release so we can coast
        if (Input.anyKey || ControllerManager.Instance.AnyControllerInput())
        {
            return;
        }

        if (Input.GetButtonUp(InputHolder.MOVE_UP) || ControllerManager.GetInput[(int)ControllerInput.UP_BUTTON].JustReleased
            || Input.GetButtonUp(InputHolder.MOVE_DOWN) || ControllerManager.GetInput[(int)ControllerInput.DOWN_BUTTON].JustReleased
            || Input.GetButtonUp(InputHolder.MOVE_LEFT) || ControllerManager.GetInput[(int)ControllerInput.LEFT_BUTTON].JustReleased
            || Input.GetButtonUp(InputHolder.MOVE_RIGHT) || ControllerManager.GetInput[(int)ControllerInput.RIGHT_BUTTON].JustReleased)
        {   
            StartCoroutine(PlayerShip.Instance.CoastPlayer());
        }
    }

    public override Vector2 ReturnRandomSpawnPositionInRange(bool CloseToPlayer = false)
    {
        float SpawnVal = 0.0f;
        bool LeftSpawn = Convert.ToBoolean(Random.Range(0, 2));

        float MinX = CloseToPlayer ? GameController.GetMapBoundsMinXVal / 2.0f : GameController.GetMapBoundsMinXVal;
        float MaxX = CloseToPlayer ? GameController.GetMapBoundsMaxXVal / 2.0f : GameController.GetMapBoundsMaxXVal;
        if (LeftSpawn)
        {
            SpawnVal = Random.Range(MinX, PlayerShip.Instance.GetPos.x//<- Use the player's position instead WorldScroller.Instance.GetCurrentCentre()
                - GameController.GetSpawnAdjustmentXVal);
        }
        else
        {
            SpawnVal = Random.Range(MaxX, PlayerShip.Instance.GetPos.x//WorldScroller.Instance.GetCurrentCentre()
               + GameController.GetSpawnAdjustmentXVal);
        }

        Vector2 RandomSpawnPosition = new(SpawnVal, Random.Range(GameController.GetMapBoundsYVal,
            -GameController.GetMapBoundsYVal));

        if (Mathf.Approximately(PlayerShip.Instance.transform.position.x, RandomSpawnPosition.x)
            || Mathf.Approximately(PlayerShip.Instance.transform.position.y, RandomSpawnPosition.y))
        {
            return ReturnRandomSpawnPositionInRange();
        }

        return RandomSpawnPosition;
    }
}
