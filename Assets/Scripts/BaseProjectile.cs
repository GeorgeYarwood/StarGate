using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseProjectile : MonoBehaviour
{
    int hitDamage = 10;
    public int HitDamage
    {
        get { return hitDamage; }
    }

    [SerializeField] bool ignoreEnemy = true;
    public bool IgnoreEnemy
    {
        get { return ignoreEnemy; }
    }
    [SerializeField] bool ignorePlayer = true;

    [SerializeField] float moveSpeed = 4.0f;
    public float MoveSpeed
    {
        get { return moveSpeed; }
    }

    [SerializeField] float timeToLive = 10.0f;
    public float TimeToLive
    {
        get { return timeToLive; }
    }

    bool allowDestroy = false;

    bool dead = false;

    void Start()
    {
        StartCoroutine(AwaitAllowDestroy());
    }

    void OnTriggerEnter2D(Collider2D Collision)
    {
        if (dead || !gameObject)
        {
            return;
        }

        bool ColSuccess = false;
        if (Collision.TryGetComponent(out EnemyBase _) && !ignoreEnemy)
        {
            ColSuccess = true;
        }
        else if (Collision.TryGetComponent(out PlayerShip _) && !ignorePlayer)
        {
            PlayerShip.Instance.OnCollisionWithEnemy();
            ColSuccess = true;
        }

        if (ColSuccess)
        {
            dead = true;
            StartCoroutine(AwaitDestroy());
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

    void Update()
    {
        if (!gameObject || GameController.Instance.GetCurrentGameState != GameController.Instance.FlyingStateInstance)
        {
            return;
        }

        Tick();
    }

    public virtual void Tick()
    {

    }

    IEnumerator AwaitDestroy()
    {
        yield return new WaitUntil(()=>allowDestroy == true);
        if (gameObject)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator AwaitAllowDestroy()
    {
        //Ensure we still see the projectile even if we hit something immediately
        yield return new WaitForSeconds(0.05f);
        allowDestroy = true;
        if (!dead)
        {
            StartCoroutine(WaitToDespawn());
        }
    }

    IEnumerator WaitToDespawn()
    {
        float Maxtime = TimeToLive;
        float TimeWaited = 0.0f;
        while (!dead && TimeWaited < Maxtime)
        {
            yield return new WaitForSeconds(1);
            TimeWaited++;
        }

        if (!dead && gameObject)
        {
            Destroy(gameObject);
        }
    }
}
