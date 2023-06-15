using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    int hitDamage = 10;
    public int HitDamage
    {
        get { return hitDamage; }
    }

    void OnTriggerEnter2D(Collider2D Collision)
    {
        if(Collision.TryGetComponent(out EnemyBase _))
        {
            Destroy(gameObject);
        }
    }

    const float MOVE_SPEED = 4.0f;

    FacingDirection projectileIsFacing;
    public FacingDirection ProjectileIsFacing
    {
        set { projectileIsFacing = value; }
    }

    const float TIME_TO_LIVE = 10.0f;

    void Start()
    {
        StartCoroutine(WaitToDespawn());
    }

    void Update()
    {
        switch (projectileIsFacing)
        {
            case FacingDirection.LEFT:
                transform.Translate(new Vector2(-1, 0) * MOVE_SPEED * Time.deltaTime);
                break;
            case FacingDirection.RIGHT:
                transform.Translate(new Vector2(1, 0) * MOVE_SPEED * Time.deltaTime);
                break;
        }
    }

    IEnumerator WaitToDespawn()
    {
        yield return new WaitForSeconds(TIME_TO_LIVE);
        DestroyImmediate(gameObject);
    }
}
