using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum FacingDirection
{
    NONE,
    LEFT,
    RIGHT,
    UP,
    DOWN
}

public class PlayerShip : PlayerController
{

    const float LEFT_ROT_Y_VAL = 180.0f;
    const float RIGHT_ROT_Y_VAL = 0.0f;

    [SerializeField] AudioClip engineSfx;
    
    FacingDirection playerIsFacing;

    static PlayerShip instance;
    public static PlayerShip Instance
    {
        get { return instance; }
    }

    public override void UpdatePosition(MoveDirection ThisDirection, float Speed, bool Coasting = false)
    {
        float ActualSpeed = PlayerMoveSpeed;

        if (Coasting)
        {
            ActualSpeed = PlayerCoastSpeed;
            AudioManager.Instance.PlayLoopedAudioClip(engineSfx, EndLoop: true);
        }
       else
        {
            AudioManager.Instance.PlayLoopedAudioClip(engineSfx);
            if (HasPowerUp(PowerUpType.SPEED_BOOST))
            {
                ActualSpeed += PlayerBoostSpeedAdj;
            }
        }

        switch (ThisDirection)
        {
            case MoveDirection.UP:
                transform.Translate(new Vector2(0.0f, 1.0f) * (ActualSpeed * Speed) * Time.deltaTime);
                break;
            case MoveDirection.DOWN:
                transform.Translate(new Vector2(0.0f, -1.0f) * (ActualSpeed * Speed) * Time.deltaTime);
                break;
            case MoveDirection.LEFT:
                transform.Translate(new Vector2(-1.0f, 0.0f) * (ActualSpeed * Speed) * Time.deltaTime);
                if (playerIsFacing != FacingDirection.LEFT)
                {
                    playerIsFacing = FacingDirection.LEFT;
                    PlayerSprite.transform.rotation = Quaternion.Euler(new(PlayerSprite.transform.rotation.eulerAngles.x, LEFT_ROT_Y_VAL));
                }
                break;
            case MoveDirection.RIGHT:
                transform.Translate(new Vector2(1.0f, 0.0f) * (ActualSpeed * Speed) * Time.deltaTime);
                if (playerIsFacing != FacingDirection.RIGHT)
                {
                    playerIsFacing = FacingDirection.RIGHT;
                    PlayerSprite.transform.rotation = Quaternion.Euler(new(PlayerSprite.transform.rotation.eulerAngles.x, RIGHT_ROT_Y_VAL));
                }
                break;
        }

        LastDirection = ThisDirection;
        ClampPlayer();
    }

    public override void ClampPlayer()
    {
        float RawY = transform.position.y;
        float RawX = transform.position.x;
        if (RawY > GameController.GetMapBoundsYVal)
        {
            transform.position = new(transform.position.x, GameController.GetMapBoundsYVal);
        }
        else if (RawY < -GameController.GetMapBoundsYVal)
        {
            transform.position = new(transform.position.x, -GameController.GetMapBoundsYVal);
        }
    }
    
    public override BaseProjectile FireProjectileInternal(FacingDirection ForceDir = FacingDirection.NONE)
    {
        Vector2 FireDirection = new();
        Vector2 ActualProjectileSpawn = ProjectileSpawn.transform.position;

        FacingDirection Dir = ForceDir == FacingDirection.NONE ? playerIsFacing : ForceDir;
        switch (Dir)
        {
            case FacingDirection.LEFT:
                FireDirection = -Vector2.right;
                ActualProjectileSpawn.x -= ProjectileSpawnModifer;
                if(HasPowerUp(PowerUpType.DOUBLE_LASER) && ForceDir == FacingDirection.NONE) 
                {
                    FireProjectileInternal(ForceDir: FacingDirection.RIGHT);
                }
                break;
            case FacingDirection.RIGHT:
                FireDirection = Vector2.right;
                ActualProjectileSpawn.x += ProjectileSpawnModifer;
                if (HasPowerUp(PowerUpType.DOUBLE_LASER) && ForceDir == FacingDirection.NONE)
                {
                    FireProjectileInternal(ForceDir: FacingDirection.LEFT);
                }
                break;
        }

        LaserProjectile ProjectileToSpawn;
        if (!HasPowerUp(PowerUpType.RAPID_FIRE))
        {
            ProjectileToSpawn = SingleFireProjectilePrefab;
        }
        else
        {
            ProjectileToSpawn = RapidFireProjectilePrefab;
        }
        //Make sure we spawn infront/behind player not inside
        LaserProjectile ProjectileInstance =
            Instantiate(ProjectileToSpawn, ActualProjectileSpawn, Quaternion.Euler(FireDirection));
        ProjectileInstance.ProjectileIsFacing = Dir;

        AudioManager.Instance.PlayAudioClip(FireSFX, RandomPitch: true);
        return ProjectileInstance;
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

        playerIsFacing = FacingDirection.RIGHT;

        base.Init();

    }

    void OnDestroy()
    {
        instance = null;
    }
}
