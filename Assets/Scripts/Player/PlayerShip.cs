using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveDirection { UP, DOWN, LEFT, RIGHT }
public enum FacingDirection { LEFT, RIGHT }

public class PlayerShip : MonoBehaviour
{
    [SerializeField] Transform projectileSpawn;
    [SerializeField] LaserProjectile projectilePrefab;
    public Transform ProjectileSpawn
    {
        get { return projectileSpawn; }
    }

    [SerializeField] float playerMoveSpeed = 10.0f;
    const float MAX_Y_VAL = 5.0f;
    const float PROJECTILE_SPAWN_MODIFIER = 1.2f;
    const float FIRE_LOCKOUT_TIMER = 1.0f;

    bool canFire = true;

    FacingDirection playerIsFacing;

    public void UpdatePosition(MoveDirection ThisDirection)
    {
        switch (ThisDirection)
        {
            case MoveDirection.UP:
                transform.Translate(new Vector2(0.0f, 1.0f) * playerMoveSpeed * Time.deltaTime);
                break;
            case MoveDirection.DOWN:
                transform.Translate(new Vector2(0.0f, -1.0f) * playerMoveSpeed * Time.deltaTime);
                break;
            case MoveDirection.LEFT:
                transform.Translate(new Vector2(-1.0f, 0.0f) * playerMoveSpeed * Time.deltaTime);
                playerIsFacing = FacingDirection.LEFT;
                break;
            case MoveDirection.RIGHT:
                transform.Translate(new Vector2(1.0f, 0.0f) * playerMoveSpeed * Time.deltaTime);
                playerIsFacing = FacingDirection.RIGHT;
                break;
        }

        ClampPlayerHeight();
    }

    void ClampPlayerHeight()
    {
        float RawY = transform.position.y;
        if (RawY > MAX_Y_VAL)
        {
            transform.position = new(transform.position.x, MAX_Y_VAL);
            return;
        }
        else if (RawY < -MAX_Y_VAL)
        {
            transform.position = new(transform.position.x, -MAX_Y_VAL);
            return;
        }
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
        playerIsFacing = FacingDirection.RIGHT;
    }

    IEnumerator FireLockOutTimer()
    {
        canFire = false;
        yield return new WaitForSeconds(FIRE_LOCKOUT_TIMER);
        canFire = true;
    }

    void Update()
    {
        
    }
}
