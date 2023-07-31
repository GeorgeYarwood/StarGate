using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEnemy : EnemyBase
{
    [SerializeField] float moveSpeed = 1.5f;
    [SerializeField] float minDistanceToPlayer = 10.0f; //How close player must be before we start chasing
    [SerializeField] float collisionAvoidRadius = 0.01f; //Radius for checking with collisions against other enemies
    public override void Tick()
    {
        TrackPlayer();
    }

    void TrackPlayer()
    {
        //Account for different width enemies
        if((transform.position.x + spriteRenderer.bounds.extents.x) >= GameController.GetMapBoundsXVal
            || (transform.position.x - spriteRenderer.bounds.extents.x) <= -GameController.GetMapBoundsXVal)
        {
            return;
        }
        if(Vector2.Distance(PlayerShip.Instance.GetPos, transform.position) <= minDistanceToPlayer)
        {
            transform.position = GetNonOverlappingVector();
        }
    }

    Vector2 GetNonOverlappingVector()
    {
        Vector2 BaseVector = Vector2.MoveTowards(transform.position,
                PlayerShip.Instance.GetPos, moveSpeed * Time.deltaTime);
        RaycastHit2D Hit = Physics2D.CircleCast(BaseVector, collisionAvoidRadius, Vector2.zero);
        if (!Hit)
        {
            return BaseVector;
        }
        if(Hit.transform.TryGetComponent(out EnemyBase HitEnemy))
        {
            if(HitEnemy == this)
            {
                return BaseVector;
            }

            BaseVector += Hit.normal;
        }

        return Vector2.MoveTowards(transform.position,
                BaseVector, moveSpeed * Time.deltaTime);
    }
}
