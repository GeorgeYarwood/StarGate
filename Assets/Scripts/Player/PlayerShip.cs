using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using System.Threading;
using System;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public enum MoveDirection { UP, DOWN, LEFT, RIGHT }
public enum FacingDirection { LEFT, RIGHT }

public class PlayerShip : MonoBehaviour
{
    [SerializeField] GameObject shipSprite;
    [SerializeField] Transform projectileSpawn;
    [SerializeField] LaserProjectile projectilePrefab;
    [SerializeField] ParticleSystem deathVfx;
    [SerializeField] AudioClip fireSfx;
    [SerializeField] AudioClip deathSfx;
    [SerializeField] AudioClip engineSfx;

    public Transform ProjectileSpawn
    {
        get { return projectileSpawn; }
    }

    const float PLAYER_MOVE_SPEED = 5.0f;
    const float PLAYER_COAST_SPEED = 0.75f;

    const float PROJECTILE_SPAWN_MODIFIER = 1.2f;
    const float FIRE_LOCKOUT_TIMER = 1.0f;
    const float PLAYER_COAST_TIMER = 1.2f; //How long the player will coast after releasing input

    const float LEFT_ROT_Y_VAL = 180.0f;
    const float RIGHT_ROT_Y_VAL = 0.0f;

    bool canFire = true;

    FacingDirection playerIsFacing;
    MoveDirection lastDirection;

    Coroutine coastPlayerCoroutine;
    public Coroutine CoastPlayerCoroutine
    {
        set { coastPlayerCoroutine = value; }
    }

    static PlayerShip instance;
    public static PlayerShip Instance
    {
        get { return instance; }
    }

    public Vector2 GetPos
    {
        get { return transform.position; }
    }

    public void UpdatePosition(MoveDirection ThisDirection, bool Coasting = false)
    {
        float ActualSpeed = PLAYER_MOVE_SPEED;

        if (Coasting)
        {
            ActualSpeed = PLAYER_COAST_SPEED;
            AudioManager.Instance.PlayLoopedAudioClip(engineSfx, EndLoop: true);
        }
        else if(coastPlayerCoroutine != null)
        {
            StopCoroutine(coastPlayerCoroutine);
        }

        if (!Coasting)
        {
            AudioManager.Instance.PlayLoopedAudioClip(engineSfx);
        }

        switch (ThisDirection)
        {
            case MoveDirection.UP:
                transform.Translate(new Vector2(0.0f, 1.0f) * ActualSpeed * Time.deltaTime);
                break;
            case MoveDirection.DOWN:
                transform.Translate(new Vector2(0.0f, -1.0f) * ActualSpeed * Time.deltaTime);
                break;
            case MoveDirection.LEFT:
                transform.Translate(new Vector2(-1.0f, 0.0f) * ActualSpeed * Time.deltaTime);
                if(playerIsFacing != FacingDirection.LEFT)
                {
                    playerIsFacing = FacingDirection.LEFT;
                    shipSprite.transform.rotation = Quaternion.Euler(new(shipSprite.transform.rotation.eulerAngles.x, LEFT_ROT_Y_VAL));
                }
                break;
            case MoveDirection.RIGHT:
                transform.Translate(new Vector2(1.0f, 0.0f) * ActualSpeed * Time.deltaTime);
                if (playerIsFacing != FacingDirection.RIGHT)
                {
                    playerIsFacing = FacingDirection.RIGHT;
                    shipSprite.transform.rotation = Quaternion.Euler(new(shipSprite.transform.rotation.eulerAngles.x, RIGHT_ROT_Y_VAL));
                }
                break;
        }

        lastDirection = ThisDirection;
        ClampPlayer();
    }

    void ClampPlayer()
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
        if (RawX > GameController.GetMapBoundsXVal)
        {
            transform.position = new(GameController.GetMapBoundsXVal, transform.position.y);
        }
        else if (RawX < -GameController.GetMapBoundsXVal)
        {
            transform.position = new(-GameController.GetMapBoundsXVal, transform.position.y);
        }
    }

    public void ResetPosition()
    {
        transform.position = Vector3.zero;
    }

    public void FireProjectile()
    {
        if (!canFire)
        {
            return;
        }

        Vector2 FireDirection = new();
        Vector2 ActualProjectileSpawn = ProjectileSpawn.transform.position;
        switch (playerIsFacing)
        {
            case FacingDirection.LEFT:
                FireDirection = -Vector2.right;
                ActualProjectileSpawn.x -= PROJECTILE_SPAWN_MODIFIER;
                break;
            case FacingDirection.RIGHT:
                FireDirection = Vector2.right;
                ActualProjectileSpawn.x += PROJECTILE_SPAWN_MODIFIER;
                break;
        }

        //Make sure we spawn infront/behind player not inside
        LaserProjectile ProjectileInstance =
            Instantiate(projectilePrefab, ActualProjectileSpawn, Quaternion.Euler(FireDirection));
        ProjectileInstance.ProjectileIsFacing = playerIsFacing;
        GameController.Instance.FlyingStateInstance.
            ProjectilesInScene.Add(ProjectileInstance.gameObject);
        AudioManager.Instance.PlayAudioClip(fireSfx, RandomPitch: true);
        StartCoroutine(FireLockOutTimer());
    }

    void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        playerIsFacing = FacingDirection.RIGHT;
    }

    IEnumerator FireLockOutTimer()
    {
        canFire = false;
        yield return new WaitForSeconds(FIRE_LOCKOUT_TIMER);
        canFire = true;
    }

    public void OnCollisionWithEnemy()
    {
        deathVfx.Play();
        AudioManager.Instance.PlayAudioClip(deathSfx);
        GameController.Instance.OnPlayerDeath();
    }

    public IEnumerator CoastPlayer()
    {
        float ThisTimer = PLAYER_COAST_TIMER;
        while (ThisTimer > 0)
        {
            UpdatePosition(lastDirection, Coasting: true);
            ThisTimer -= 1.0f * Time.deltaTime;
            yield return null;
        }
        coastPlayerCoroutine = null;
    }

    void OnDestroy() 
    {
        instance = null;
    }
}
