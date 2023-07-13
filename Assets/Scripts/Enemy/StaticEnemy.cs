using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Enemy type that doesn't chase the enemy, but (can) shoot at the player
//Kind of poorly named as we can animate this
public class StaticEnemy : EnemyBase
{
    [SerializeField] float timeBetweenShots = 1.5f;
    [SerializeField] bool shootAtPlayer;

    [SerializeField] LaserProjectile projectile;

    List<LaserProjectile> spawnedProjectiles = new List<LaserProjectile>();

    FacingDirection lastFacingDirection = FacingDirection.RIGHT;

    public override void Init()
    {
        if (shootAtPlayer)
        {
            StartCoroutine(FireAtPlayer());
        }
    }

    IEnumerator FireAtPlayer()
    {
        while(true)
        {
            if (GameController.Instance.GetCurrentGameState
                is FlyingState)
            {
                LaunchProjectile();
                yield return new WaitForSeconds(timeBetweenShots);
            }
            yield return null;
        }
    }

    void LaunchProjectile()
    {
        LaserProjectile ThisProjectile = Instantiate(projectile, transform.position, Quaternion.identity);
        switch(lastFacingDirection)
        {
            case FacingDirection.LEFT:
                lastFacingDirection = FacingDirection.RIGHT;
                break;
            case FacingDirection.RIGHT:
                lastFacingDirection = FacingDirection.LEFT;
                break;
        }

        ThisProjectile.ProjectileIsFacing = lastFacingDirection;
        spawnedProjectiles.Add(ThisProjectile);
    }

    void ClearAllProjectiles()
    {
        for (int p = 0; p < spawnedProjectiles.Count; p++)
        {
            if (spawnedProjectiles[p])
            {
                Destroy(spawnedProjectiles[p].gameObject);
            }
        }

        spawnedProjectiles.Clear();
    }

    void CleanList()    //Ensure we don't have dead references
    {
        for (int p = 0; p < spawnedProjectiles.Count; p++)
        {
            if (!spawnedProjectiles[p])
            {
                spawnedProjectiles.RemoveAt(p);
            }
        }
    }

    public override void Tick()
    {
        if(GameController.Instance.GetCurrentGameState
            is PauseGameState && spawnedProjectiles.Count > 0)
        {
            ClearAllProjectiles();
        }

        else
        {
            CleanList();
        }
    }
}
