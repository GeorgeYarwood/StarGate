using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEnemy : EnemyBase
{
    [SerializeField] float moveSpeed = 1.5f;
    [SerializeField] float minDistanceToPlayer = 10.0f; //How close player must be before we start chasing
    [SerializeField] float collisionAvoidRadius = 6.5f; //Radius for checking with collisions against other enemies
    [SerializeField] float minShootDistance = 5.0f; //How close player must be before we start shooting

    public override void Tick()
    {
        TrackPlayer();
    }

    void TrackPlayer()
    {
        PlayerController PController = GameController.Instance.GetActivePlayerController();
        if (Vector2.Distance(PController.GetPos, transform.position) <= minDistanceToPlayer)
        {
            transform.position = GetNonOverlappingVector();
        }
    }

    public override void LaunchProjectile()
    {
        BaseProjectile NewProjectile = Instantiate(Projectile, transform.position, Quaternion.identity);
        EnemyProjectile AsEnemyProjectile = (EnemyProjectile)NewProjectile;
        PlayerController PController = GameController.Instance.GetActivePlayerController();

        AsEnemyProjectile.Target = PController.GetPos;
        SpawnedProjectiles.Add(AsEnemyProjectile);
    }

    public override bool CanFireAtPlayer()
    {
        PlayerController PController = GameController.Instance.GetActivePlayerController();

        return Vector2.Distance(PController.GetPos, transform.position) <= minShootDistance;
    }

    Vector2 GetNonOverlappingVector()
    {
        PlayerController PController = GameController.Instance.GetActivePlayerController();

        Vector2 BaseVector = Vector2.MoveTowards(transform.position,
                PController.GetPos, moveSpeed * Time.deltaTime);
        RaycastHit2D Hit = Physics2D.CircleCast(BaseVector, collisionAvoidRadius, Vector2.zero);
        if (!Hit)
        {
            return BaseVector;
        }
        if (Hit.transform.TryGetComponent(out EnemyBase HitEnemy))
        {
            if (HitEnemy == this)
            {
                return BaseVector;
            }

            BaseVector += Hit.normal;
        }

        return Vector2.MoveTowards(transform.position,
                BaseVector, moveSpeed * Time.deltaTime);
    }
}
