using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEnemy : EnemyBase
{
    const float MOVE_SPEED = 1.5f;
    const float MIN_DIST_TO_PLAYER = 10.0f;
    public override void Tick()
    {
        TrackPlayer();
    }

    void TrackPlayer()
    {
        if(Vector2.Distance(PlayerShip.Instance.GetPos, transform.position) <= MIN_DIST_TO_PLAYER)
        {
            transform.position = Vector2.MoveTowards(transform.position,
                PlayerShip.Instance.GetPos, MOVE_SPEED * Time.deltaTime);
        }
    }
}
