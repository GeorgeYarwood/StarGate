using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using System.Threading;
using System;
using System.Threading.Tasks;

public enum MoveDirection { UP, DOWN, LEFT, RIGHT }
public enum FacingDirection { LEFT, RIGHT }

public class PlayerShip : MonoBehaviour
{
    [SerializeField] Transform projectileSpawn;
    [SerializeField] LaserProjectile projectilePrefab;
    [SerializeField] ParticleSystem deathVfx;

    public Transform ProjectileSpawn
    {
        get { return projectileSpawn; }
    }

    const float PLAYER_MOVE_SPEED = 10.0f;
    const float PLAYER_COAST_SPEED = 2000.0f;

    const float PROJECTILE_SPAWN_MODIFIER = 1.2f;
    const float FIRE_LOCKOUT_TIMER = 1.0f;
    const float PLAYER_COAST_TIMER = 3.5f; //How long the player will coast after releasing input

    bool canFire = true;

    //Input tracking
    bool inputWasDown = false;
    bool inputIsDown = false;

    bool coasting = false;

    MoveDirection lastMoveDirection;
    FacingDirection playerIsFacing;

    //Thread coastPlayerThread;

    //static List<Action> actionsQueueForMainThread = new List<Action>();

    Task coastPlayer;

    static PlayerShip instance;
    public static PlayerShip Instance
    {
        get { return instance; }
    }

    public Vector2 GetPos
    {
        get { return transform.position; }
    }

    public void UpdatePosition(MoveDirection ThisDirection, float Speed = PLAYER_MOVE_SPEED)
    {
        switch (ThisDirection)
        {
            case MoveDirection.UP:
                transform.Translate(new Vector2(0.0f, 1.0f) * Speed * Time.deltaTime);
                break;
            case MoveDirection.DOWN:
                transform.Translate(new Vector2(0.0f, -1.0f) * Speed * Time.deltaTime);
                break;
            case MoveDirection.LEFT:
                transform.Translate(new Vector2(-1.0f, 0.0f) * Speed * Time.deltaTime);
                playerIsFacing = FacingDirection.LEFT;
                break;
            case MoveDirection.RIGHT:
                transform.Translate(new Vector2(1.0f, 0.0f) * Speed * Time.deltaTime);
                playerIsFacing = FacingDirection.RIGHT;
                break;
        }
        if (!coasting)
        {
            inputIsDown = true;
            lastMoveDirection = ThisDirection;
        }
        else
        {
            Debug.Log("Coasting");
        }
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

        StartCoroutine(FireLockOutTimer());
    }

    void Start()
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
        //coastPlayerThread = new Thread(new ThreadStart(CoastPlayer));
        //coastPlayer = new Task(CoastPlayer);
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
        GameController.Instance.OnPlayerDeath();
    }

    public void OnCollisionWithSubLevelEntrance()
    {
        //TODO Play VFX
    }

    void CoastPlayer()
    {
        coasting = true;
        float ThisTimer = PLAYER_COAST_TIMER;
        while (ThisTimer > 0)
        {
            if (inputIsDown)
            {
                coasting = false;
                //return;
            }

            //Action NewActionForQueue = () =>
            //{
            //    UpdatePosition(lastMoveDirection, PLAYER_COAST_SPEED, Coasting: true);
            //};
            //actionsQueueForMainThread.Add(NewActionForQueue);

            UpdatePosition(lastMoveDirection, PLAYER_COAST_SPEED);

            ThisTimer -= 1.0f * Time.deltaTime;
        }
        coasting = false;
        inputWasDown = false;
    }

    void Update()
    {
        //ProcessActionsFromThreads();

        if (!inputIsDown && inputWasDown && !coasting /*&& coastPlayerThread.ThreadState is not ThreadState.Running*/)
        {
            //coastPlayerThread.Start();
            Task.Run(CoastPlayer);
            inputWasDown = false;
        }
        else
        {
            inputWasDown = inputIsDown;
        }
        inputIsDown = false;
    }

    //void ProcessActionsFromThreads()
    //{
    //    foreach (var action in actionsQueueForMainThread)
    //    {
    //        action();
    //        actionsQueueForMainThread.Remove(action);
    //    }
    //}
}
