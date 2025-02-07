using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Enemy type that doesn't chase the enemy, but (can) shoot at the player
//Kind of poorly named as we can animate this
public class StaticEnemy : EnemyBase
{
    const string BACKGROUND_LAYER_MASK = "Background";

    FacingDirection lastFacingDirection = FacingDirection.RIGHT;

    void Start()
    {
        base.backgroundLayerMask = LayerMask.GetMask(BACKGROUND_LAYER_MASK);
    }
    
    public override void LaunchProjectile()
    {
        BaseProjectile NewProjectile = Instantiate(Projectile, transform.position, Quaternion.identity);

        LaserProjectile AsLaser = (LaserProjectile)NewProjectile;
        switch(lastFacingDirection)
        {
            case FacingDirection.LEFT:
                lastFacingDirection = FacingDirection.RIGHT;
                break;
            case FacingDirection.RIGHT:
                lastFacingDirection = FacingDirection.LEFT;
                break;
        }

        AsLaser.ProjectileIsFacing = lastFacingDirection;
        SpawnedProjectiles.Add(AsLaser);
    }

    

    public override void Tick()
    {
    }
}
