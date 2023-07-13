using System;
using System.Collections;
using UnityEngine;

public enum MoveDirection
{ 
    UP,
    DOWN, 
    LEFT, 
    RIGHT 
}

public enum FacingDirection 
{ 
    LEFT, 
    RIGHT 
}

public enum WeaponFireMode
{
    SINGLE,
    RAPID
}

public class PlayerShip : MonoBehaviour
{
    [SerializeField] GameObject shipSprite;
    [SerializeField] Transform projectileSpawn;
    [SerializeField] LaserProjectile singleFireProjectilePrefab;
    [SerializeField] LaserProjectile rapidFireProjectilePrefab;
    [SerializeField] ParticleSystem deathVfx;
    [SerializeField] AudioClip fireSfx;
    [SerializeField] AudioClip deathSfx;
    [SerializeField] AudioClip engineSfx;

    public Transform ProjectileSpawn
    {
        get { return projectileSpawn; }
    }

    const float PLAYER_MOVE_SPEED = 5.0f;
    const float PLAYER_BOOST_ADJUSTMENT = 2.5f;
    const float PLAYER_COAST_SPEED = 0.75f;

    const float PROJECTILE_SPAWN_MODIFIER = 1.2f;
    const float SINGLE_FIRE_LOCKOUT_TIMER = 1.0f;
    const float RAPID_FIRE_LOCKOUT_TIMER = 0.15f;
    const float RAPID_FIRE_EFFECT_TIMER = 3.0f;
    const float PLAYER_COAST_TIMER = 1.2f; //How long the player will coast after releasing input
    const float BOOST_EFFECT_TIMER = 2.0f; //Stops player just holding boost forever

    const float LEFT_ROT_Y_VAL = 180.0f;
    const float RIGHT_ROT_Y_VAL = 0.0f;

    bool canFire = true;
    bool isBoosting = false;

    FacingDirection playerIsFacing;
    MoveDirection lastDirection;
    WeaponFireMode currentFireMode = WeaponFireMode.SINGLE;

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

    Action rapidFirePowerUp;
    public Action RapidFirePowerUp
    {
        get { return rapidFirePowerUp; }
    }
    Action speedBoostPowerUp;
    public Action SpeedBoostPowerUp
    {
        get { return speedBoostPowerUp; }
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
            if (isBoosting)
            {
                ActualSpeed += PLAYER_BOOST_ADJUSTMENT;
            }
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

        LaserProjectile ProjectileToSpawn;
        if (currentFireMode == WeaponFireMode.SINGLE)
        {
            ProjectileToSpawn = singleFireProjectilePrefab;
        }
        else
        {
            ProjectileToSpawn = rapidFireProjectilePrefab;
        }
        //Make sure we spawn infront/behind player not inside
        LaserProjectile ProjectileInstance =
            Instantiate(ProjectileToSpawn, ActualProjectileSpawn, Quaternion.Euler(FireDirection));
        ProjectileInstance.ProjectileIsFacing = playerIsFacing;
        GameController.Instance.FlyingStateInstance.
            ProjectilesInScene.Add(ProjectileInstance.gameObject);
        AudioManager.Instance.PlayAudioClip(fireSfx, RandomPitch: true);
        StartCoroutine(FireLockOutTimer());
    }

    void HandleSpeedBoostEffect()
    {
        StartCoroutine(SpeedBoostTimer());
    }

    IEnumerator SpeedBoostTimer()
    {
        isBoosting = true;
        yield return new WaitForSeconds(BOOST_EFFECT_TIMER);
        isBoosting = false;
    }


    void HandleRapidFireEffect()
    {
        StartCoroutine(RapidFireTimer());
    }

    IEnumerator RapidFireTimer()
    {
        currentFireMode = WeaponFireMode.RAPID;
        yield return new WaitForSeconds(RAPID_FIRE_EFFECT_TIMER);
        currentFireMode = WeaponFireMode.SINGLE;
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
        rapidFirePowerUp += HandleRapidFireEffect;
        speedBoostPowerUp += HandleSpeedBoostEffect;
    }

    IEnumerator FireLockOutTimer()
    {
        canFire = false;
        if(currentFireMode == WeaponFireMode.SINGLE)
        {
            yield return new WaitForSeconds(SINGLE_FIRE_LOCKOUT_TIMER);
        }
        else
        {
            yield return new WaitForSeconds(RAPID_FIRE_LOCKOUT_TIMER);
        }
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
