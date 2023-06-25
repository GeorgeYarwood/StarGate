using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEnemy : EnemyBase
{
    const float MOVE_SPEED = 1.5f;

    public override void Tick()
    {
        TrackPlayer();
    }

    void TrackPlayer()
    {
        float Step = MOVE_SPEED * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position,
            PlayerShip.Instance.GetPos, MOVE_SPEED * Time.deltaTime);
    }
}
