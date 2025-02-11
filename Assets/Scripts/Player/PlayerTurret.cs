using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;

public class PlayerTurret : PlayerController
{
    static PlayerTurret instance;
    public static PlayerTurret Instance
    {
        get { return instance; }
    }

    public override void Init()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        base.Init();
    }

    public override void UpdatePosition(MoveDirection ThisDirection, float Speed, bool Coasting = false)
    {
        float ActualSpeed = PlayerMoveSpeed;

        if (Coasting)
        {
            ActualSpeed = PlayerCoastSpeed;
        }
        else
        {
            if (HasPowerUp(PowerUpType.SPEED_BOOST))
            {
                ActualSpeed += PlayerBoostSpeedAdj;
            }
        }

        switch (ThisDirection)
        {
            case MoveDirection.LEFT:
                transform.Translate(new Vector2(-1.0f, 0.0f) * (ActualSpeed * Speed) * Time.deltaTime);
                break;
            case MoveDirection.RIGHT:
                transform.Translate(new Vector2(1.0f, 0.0f) * (ActualSpeed * Speed) * Time.deltaTime);
                break;
        }

        LastDirection = ThisDirection;
        ClampPlayer();
    }

    public override void ClampPlayer()
    {
        //Clamp to middle section
        float RawX = transform.position.x;
        float MaxVal = WorldScroller.Instance.MiddleWorldSection.bounds.size.x / 2.0f;

        if (RawX > MaxVal)
        {
            transform.position = new(MaxVal, transform.position.y);
        }
        else if (RawX < -MaxVal)
        {
            transform.position = new(-MaxVal, transform.position.y);
        }
    }

    public override BaseProjectile FireProjectileInternal(FacingDirection ForceDir = FacingDirection.NONE)
    {
        //Just fire upwards
        Vector2 ProjectileSpawnPos = ProjectileSpawn.transform.position;

        ProjectileSpawnPos.y += ProjectileSpawnModifer;

        LaserProjectile ProjectileToSpawn;
        if (!HasPowerUp(PowerUpType.RAPID_FIRE))
        {
            ProjectileToSpawn = SingleFireProjectilePrefab;
        }
        else
        {
            ProjectileToSpawn = RapidFireProjectilePrefab;
        }

        LaserProjectile ProjectileInstance =
            Instantiate(ProjectileToSpawn, ProjectileSpawnPos, Quaternion.Euler(new Vector3(0.0f, 0.0f, 90.0f)));
        ProjectileInstance.ProjectileIsFacing = FacingDirection.UP;
        AudioManager.Instance.PlayAudioClip(FireSFX, RandomPitch: true);

        return ProjectileInstance;
    }

    void OnDestroy()
    {
        instance = null;
    }
}
