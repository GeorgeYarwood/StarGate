using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserProjectile : BaseProjectile
{
    FacingDirection projectileIsFacing;
    public FacingDirection ProjectileIsFacing
    {
        set { projectileIsFacing = value; }
    }

    public override void Tick()
    {
        switch (projectileIsFacing)
        {
            case FacingDirection.LEFT:
                transform.Translate(new Vector2(-1, 0) * MoveSpeed * Time.deltaTime);
                break;
            case FacingDirection.RIGHT:
                transform.Translate(new Vector2(1, 0) * MoveSpeed * Time.deltaTime);
                break;
        }
    }
}
