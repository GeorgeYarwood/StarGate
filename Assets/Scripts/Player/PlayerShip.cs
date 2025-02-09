using System;
using System.Collections;
using System.Collections.Generic;
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
    NONE,
    LEFT,
    RIGHT
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
    [SerializeField] AudioClip mapRepeatBoostSfx;

    public Transform ProjectileSpawn
    {
        get { return projectileSpawn; }
    }

    const float PLAYER_MOVE_SPEED = 5.0f;
    const float PLAYER_BOOST_ADJUSTMENT = 2.5f;
    const float PLAYER_COAST_SPEED = 0.75f;
    const float PLAYER_MAP_REPEAT_BOOST_SPEED = 10.5f;

    const float PROJECTILE_SPAWN_MODIFIER = 2.5f;
    const float SINGLE_FIRE_LOCKOUT_TIMER = 1.0f;
    const float RAPID_FIRE_LOCKOUT_TIMER = 0.15f;
    const float PLAYER_COAST_TIMER = 1.2f; //How long the player will coast after releasing input

    const float LEFT_ROT_Y_VAL = 180.0f;
    const float RIGHT_ROT_Y_VAL = 0.0f;

    bool canFire = true;
    bool lockInput = false;
    public bool LockInput
    {
        get { return lockInput; }
    }

    FacingDirection playerIsFacing;
    MoveDirection lastDirection;

    static PlayerShip instance;
    public static PlayerShip Instance
    {
        get { return instance; }
    }

    public Vector2 GetPos
    {
        get { return transform.position; }
    }

    bool invincible = false;
    public bool Invincible
    {
        get { return invincible; }
        set { invincible = value; }
    }

    bool debugInvincible = false;
    public bool DebugInvincible
    {
        get { return debugInvincible; }
        set { debugInvincible = value; }
    }

    Dictionary<PowerUpType, ActivePowerUp> heldPowerups = new Dictionary<PowerUpType, ActivePowerUp>();
    public Dictionary<PowerUpType, ActivePowerUp> HeldPowerups
    {
        get { return heldPowerups; }
    }

    public class ActivePowerUp 
    {
        public float RemainingTime;
        public bool IsActive() 
        {
            return RemainingTime > 0.0f;
        }

        public ActivePowerUp(float SetTime) 
        {
            RemainingTime = SetTime;
        }
    }

    public void UpdatePosition(MoveDirection ThisDirection, bool Coasting = false, bool FastSpeed = false)
    {
        float ActualSpeed = PLAYER_MOVE_SPEED;

        if (Coasting)
        {
            ActualSpeed = PLAYER_COAST_SPEED;

            if (FastSpeed)
            {
                ActualSpeed = PLAYER_MAP_REPEAT_BOOST_SPEED;
            }
            AudioManager.Instance.PlayLoopedAudioClip(engineSfx, EndLoop: true);
        }
       else
        {
            AudioManager.Instance.PlayLoopedAudioClip(engineSfx);
            if (HasPowerUp(PowerUpType.SPEED_BOOST))
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
                if (playerIsFacing != FacingDirection.LEFT)
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
    }

    public void ParentToBackground()
    {
        RaycastHit2D Centre = Physics2D.Raycast(transform.position, transform.forward);

        if (Centre.collider)
        {
            transform.parent = Centre.collider.transform;
        }
    }

    public void DetachFromParent()
    {
        transform.parent = null;
    }

    public void ResetPosition()
    {
        transform.position = Vector3.zero;
    }

    public void FireProjectile(FacingDirection ForceDir = FacingDirection.NONE)
    {
        if (!canFire)
        {
            return;
        }

        Vector2 FireDirection = new();
        Vector2 ActualProjectileSpawn = ProjectileSpawn.transform.position;

        FacingDirection Dir = ForceDir == FacingDirection.NONE ? playerIsFacing : ForceDir;
        switch (Dir)
        {
            case FacingDirection.LEFT:
                FireDirection = -Vector2.right;
                ActualProjectileSpawn.x -= PROJECTILE_SPAWN_MODIFIER;
                if(HasPowerUp(PowerUpType.DOUBLE_LASER) && ForceDir == FacingDirection.NONE) 
                {
                    FireProjectile(ForceDir: FacingDirection.RIGHT);
                }
                break;
            case FacingDirection.RIGHT:
                FireDirection = Vector2.right;
                ActualProjectileSpawn.x += PROJECTILE_SPAWN_MODIFIER;
                if (HasPowerUp(PowerUpType.DOUBLE_LASER) && ForceDir == FacingDirection.NONE)
                {
                    FireProjectile(ForceDir: FacingDirection.LEFT);
                }
                break;
        }

        LaserProjectile ProjectileToSpawn;
        if (!HasPowerUp(PowerUpType.RAPID_FIRE))
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
        ProjectileInstance.ProjectileIsFacing = Dir;
        GameController.Instance.FlyingStateInstance.
            ProjectilesInScene.Add(ProjectileInstance.gameObject);
        AudioManager.Instance.PlayAudioClip(fireSfx, RandomPitch: true);
        StartCoroutine(FireLockOutTimer());
    }

    public void EndAllPowerups()
    {
        foreach (KeyValuePair<PowerUpType, ActivePowerUp> Entry in HeldPowerups)
        {
            Entry.Value.RemainingTime = 0.0f;
        }
    }

    public void ApplyPowerup(PowerUpType PUType, PowerUpContainer PUContainer)
    {
        ActivePowerUp NewEntry = null;

        if (HeldPowerups.TryGetValue(PUType, out ActivePowerUp ThisActivePU))
        {
            NewEntry = ThisActivePU;
            NewEntry.RemainingTime += PUContainer.Time;
        }
        else
        {
            NewEntry = new ActivePowerUp(PUContainer.Time);
        }
           
        HeldPowerups[PUType] = NewEntry;
    }

    bool HasPowerUp(PowerUpType PUType) 
    {
        if (HeldPowerups.TryGetValue(PUType, out ActivePowerUp ThisActivePU)) 
        {
            return ThisActivePU.IsActive();
        }

        return false;
    }

    IEnumerator TickPowerups() 
    {
        while (true) 
        {
            foreach(KeyValuePair<PowerUpType, ActivePowerUp> Entry in HeldPowerups) 
            {
                if (Entry.Value.IsActive()) 
                {
                    Entry.Value.RemainingTime -= 1.0f;
                    if(Entry.Value.RemainingTime < 0.0f)
                    {
                        Entry.Value.RemainingTime = 0.0f;
                    }
                }
            }
            yield return new WaitForSeconds(1.0f);
        }
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

    void Start()
    {
        StartCoroutine(TickPowerups());
    }

    IEnumerator FireLockOutTimer()
    {
        canFire = false;
        if (!HasPowerUp(PowerUpType.RAPID_FIRE))
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
        if (invincible || debugInvincible)
        {
            return;
        }

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

            if (Input.anyKey || ControllerManager.Instance.AnyControllerInput())
            {
                break;
            }
        }
    }

    void OnDestroy()
    {
        instance = null;
    }
}
