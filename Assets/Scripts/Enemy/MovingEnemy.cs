using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEnemy : EnemyBase
{
    [SerializeField] float moveSpeed = 1.5f;
    [SerializeField] float minDistanceToPlayer = 10.0f; //How close player must be before we start chasing
    public override void Tick()
    {
        TrackPlayer();
    }

    void TrackPlayer()
    {
        if(Vector2.Distance(PlayerShip.Instance.GetPos, transform.position) <= minDistanceToPlayer)
        {
            transform.position = Vector2.MoveTowards(transform.position,
                PlayerShip.Instance.GetPos, moveSpeed * Time.deltaTime);
        }
    }
}
