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
            transform.position = GetNonOverlappingVector(PController);
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

    Vector2 GetNonOverlappingVector(PlayerController PController)
    {
        Vector2 Target = PController.GetPos;

        if(PController as PlayerTurret) 
        {
            //Position differently when dealing with turret
            Target.y = 0.0f;

            float Angle = Random.Range(-90.0f, 90.0f) * Mathf.Deg2Rad;
            float MaxVal = WorldScroller.Instance.MiddleWorldSection.bounds.size.x / 2.0f;

            float X = Random.Range(-MaxVal, MaxVal);
            //float Y = (Target.y * Mathf.Sin(Angle));

            Target.x = X;
            //Target.y = Y;

            //return Target;  
        }

        Vector2 BaseVector = Vector2.MoveTowards(transform.position,
                Target, moveSpeed * Time.deltaTime);


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
