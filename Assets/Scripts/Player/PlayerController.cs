using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerControllerType
{
    SHIP,
    TURRET
}
public enum MoveDirection
{
    UP,
    DOWN,
    LEFT,
    RIGHT
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

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject playerSprite;
    public GameObject PlayerSprite
    {
        get { return playerSprite; }
    }
    [SerializeField] Transform projectileSpawn;
    [SerializeField] LaserProjectile singleFireProjectilePrefab;
    public LaserProjectile SingleFireProjectilePrefab
    {
        get { return singleFireProjectilePrefab; }
    }
    [SerializeField] LaserProjectile rapidFireProjectilePrefab;
    public LaserProjectile RapidFireProjectilePrefab
    {
        get { return rapidFireProjectilePrefab; }
    }
    [SerializeField] ParticleSystem deathVfx;
    public ParticleSystem DeathVFX
    {
        get { return deathVfx; }
    }
    [SerializeField] AudioClip fireSfx;
    public AudioClip FireSFX
    {
        get { return fireSfx; }
    }
    [SerializeField] AudioClip deathSfx;
    public AudioClip DeathSFX
    {
        get { return deathSfx; }
    } 

    [SerializeField] float playerMoveSpeed = 5.0f;
    public float PlayerMoveSpeed
    {
        get
        {
            return playerMoveSpeed;
        }
    }

    [SerializeField] float playerBoostSpeedAdj = 2.5f;
    public float PlayerBoostSpeedAdj
    {
        get { return playerBoostSpeedAdj; }
    }

    [SerializeField] float playerCoastSpeed = 0.75f;
    public float PlayerCoastSpeed
    {
        get { return playerCoastSpeed; }
    }

    [SerializeField] float playerCoastTimer = 1.2f; //How long the player will coast after releasing input
    public float PlayerCoastTimer
    {
        get { return playerCoastTimer; }
    }
        
    [SerializeField ]float projectileSpawnModifier = 2.5f;
    public float ProjectileSpawnModifer
    {
        get { return projectileSpawnModifier; }
    }

    public const float SINGLE_FIRE_LOCKOUT_TIMER = 1.0f;
    public const float RAPID_FIRE_LOCKOUT_TIMER = 0.15f;

    public Transform ProjectileSpawn
    {
        get { return projectileSpawn; }
    }

    public Vector2 GetPos
    {
        get { return transform.position; }
    }

    MoveDirection lastDirection;
    public MoveDirection LastDirection
    {
        get { return lastDirection; }
        set { lastDirection = value; }
    }

    //Statics shared across PlayerControllers

    static bool canFire = true;
    public static bool CanFire
    {
        get { return canFire; }
        set { canFire = value; }
    }
    static bool lockInput = false;
    public static bool LockInput
    {
        get { return lockInput; }
        set { lockInput = value; }
    }

    static bool invincible = false;
    public static bool Invincible
    {
        get { return invincible; }
        set { invincible = value; }
    }

    static bool debugInvincible = false;
    public static bool DebugInvincible
    {
        get { return debugInvincible; }
        set { debugInvincible = value; }
    }

    static Dictionary<PowerUpType, ActivePowerUp> heldPowerups = new Dictionary<PowerUpType, ActivePowerUp>();
    public static Dictionary<PowerUpType, ActivePowerUp> HeldPowerups
    {
        get { return heldPowerups; }
    }

    //

    void Start()
    {
        Init();
    }

    public virtual void Init()
    {
    }

    public virtual void UpdatePosition(MoveDirection ThisDirection, bool Coasting = false)
    {

    }

    public virtual void ClampPlayer()
    {

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

    public void FireProjectile()
    {
        if (!CanFire)
        {
            return;
        }

        BaseProjectile NewProjectile = FireProjectileInternal();
        (GameController.Instance.GetCurrentGameState as PlayState).ProjectilesInScene.Add(NewProjectile.gameObject);
        StartCoroutine(FireLockOutTimer());
    }

    public virtual BaseProjectile FireProjectileInternal(FacingDirection ForceDir = FacingDirection.NONE) 
    {
        return null;
    }

    public static void EndAllPowerups()
    {
        foreach (KeyValuePair<PowerUpType, ActivePowerUp> Entry in HeldPowerups)
        {
            Entry.Value.RemainingTime = 0.0f;
        }
    }

    public static void ApplyPowerup(PowerUpType PUType, PowerUpContainer PUContainer)
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

    public static bool HasPowerUp(PowerUpType PUType)
    {
        if (HeldPowerups.TryGetValue(PUType, out ActivePowerUp ThisActivePU))
        {
            return ThisActivePU.IsActive();
        }

        return false;
    }

    public IEnumerator FireLockOutTimer()
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

    public void SetEnabled(bool Enabled)
    {
        canFire = true; //Incase this happens while dead and the CR never finishes
        gameObject.SetActive(Enabled);
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
        float ThisTimer = PlayerCoastTimer;
        
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
}
