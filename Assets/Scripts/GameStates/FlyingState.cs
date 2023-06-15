using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingState : GameStateBase
{
    [SerializeField] EnemyBase[] enemyTypesToSpawn = new EnemyBase[2];
    List<EnemyBase> enemiesInScene = new List<EnemyBase>();

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
        CameraController.Instance.UpdatePosition(PlayerShip.Instance.transform.position, TrackAxis.X_AXIS);
    }

    void GetInput()
    {
        if (Input.GetButton(MOVE_UP))
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.UP);
        }
        if (Input.GetButton(MOVE_DOWN))
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.DOWN);
        }
        if (Input.GetButton(MOVE_LEFT))
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.LEFT);
        }
        if (Input.GetButton(MOVE_RIGHT))
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.RIGHT);
        }
        if (Input.GetButton(FIRE))
        {
            PlayerShip.Instance.FireProjectile();
        }
    }

    void SpawnEnemies()
    {
        
    }
}
