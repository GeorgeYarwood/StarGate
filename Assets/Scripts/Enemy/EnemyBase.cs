using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    int enemyHealth;
    public int EnemyHealth
    {
        get { return enemyHealth; }
    }

    void Start()
    {
        
    }

    void OnTriggerEnter2D(Collider2D Collision)
    {
        if(Collision.TryGetComponent(out LaserProjectile HitProjectile))
        {
            OnHit(HitProjectile.HitDamage);
        }
    }

    public virtual void OnHit(int DamageToDeduct)
    {
        if (enemyHealth - DamageToDeduct > 0)
        {
            enemyHealth -= DamageToDeduct;
            return;
        }

        OnDie();
    }

    public virtual void OnDie()
    {
        Destroy(gameObject);
        GameController.Instance.AddScore();
    }
}
