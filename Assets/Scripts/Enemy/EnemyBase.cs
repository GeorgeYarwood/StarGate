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

    int scoreAddition = 5;
    public int ScoreAddition
    {
        get { return scoreAddition; }
    }

    void OnTriggerEnter2D(Collider2D Collision)
    {
        if(Collision.TryGetComponent(out LaserProjectile HitProjectile))
        {
            OnHit(HitProjectile.HitDamage);
        }
        else if(Collision.TryGetComponent(out PlayerShip _))
        {
            PlayerShip.Instance.OnCollisionWithEnemy();
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
        GameController.Instance.AddScore(scoreAddition);
        GameController.Instance.FlyingStateInstance.RemoveEnemyFromList(this);
        Destroy(gameObject);
    }
}
