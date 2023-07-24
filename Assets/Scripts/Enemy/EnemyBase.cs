using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { NORMAL, BOSS, BOSS_MASSIVE } //Used for comaprison checks

public class EnemyBase : MonoBehaviour
{
    [SerializeField] int enemyHealth;
    public int EnemyHealth
    {
        get { return enemyHealth; }
    }

    int scoreAddition = 5;
    public int ScoreAddition
    {
        get { return scoreAddition; }
    }

    [SerializeField] ParticleSystem deathVfx;
    [SerializeField] ParticleSystem hitVfx;
    [SerializeField] AudioClip deathSfx;
    [SerializeField] AudioClip hitSfx;
    [SerializeField] PowerUp canDropOnDeath;
    [SerializeField] bool alwaysDrop = false;
    [SerializeField] Dialogue[] enemyDialogue = new Dialogue[3];
    [SerializeField] bool onePerLevel = false;
    [SerializeField] float encounterDistance = 5.0f;    //How close until ON_ENCOUNTER dialogue is triggered

    bool hasRunFirstEncounter = false;

    public bool OnePerLevel
    {
        get { return onePerLevel; }
    }

    public Dialogue[] EnemyDialogue
    {
        get { return enemyDialogue; }
    }
    bool waitingToDie = false;

    void Start()
    {
        Init();
    }

    [SerializeField] EnemyType thisEnemyType;
    public EnemyType ThisEnemyType
    {
        get { return thisEnemyType; }
    }

    void OnTriggerEnter2D(Collider2D Collision)
    {
        if (Collision.TryGetComponent(out LaserProjectile HitProjectile))
        {
            if (!HitProjectile.IgnoreEnemy)
            {
                OnHit(HitProjectile.HitDamage);
            }
        }
        else if (Collision.TryGetComponent(out PlayerShip _))
        {
            PlayerShip.Instance.OnCollisionWithEnemy();
        }
    }

    public virtual void OnHit(int DamageToDeduct)
    {
        if (enemyHealth - DamageToDeduct > 0)
        {
            enemyHealth -= DamageToDeduct;
            if (hitSfx)
            {
                AudioManager.Instance.PlayAudioClip(hitSfx);
            }
            if (hitVfx)
            {
                hitVfx.Play();
            }

            HandleDialogue(DialogueQueuePoint.AT_HP);
            return;
        }

        OnDie();
    }

    public virtual void OnDie()
    {
        GameController.Instance.AddScore(scoreAddition);
        GameController.Instance.FlyingStateInstance.RemoveEnemyFromList(this);
        HandleDialogue(DialogueQueuePoint.ON_DEATH);
        if (canDropOnDeath)
        {
            PowerUpManager.Instance.DropRandomPowerUpAtPosition(transform.position, canDropOnDeath, ForceSpawn: alwaysDrop);
        }
        if (deathSfx)
        {
            AudioManager.Instance.PlayAudioClip(deathSfx);
        }
        if (deathVfx)
        {
            deathVfx.Play();
            waitingToDie = true;
            return;
        }

        Destroy(gameObject);
    }

    void HandleDialogue(DialogueQueuePoint WhatPointIsThis)
    {
        if (enemyDialogue.Length == 0)
        {
            return;
        }

        for (int d = 0; d < enemyDialogue.Length; d++)
        {
            if (!enemyDialogue[d])
            {
                continue;
            }
            if (enemyDialogue[d].WhenToPlay == DialogueQueuePoint.ON_DEATH
                && WhatPointIsThis == DialogueQueuePoint.ON_DEATH)
            {
                DialogueManager.Instance.PlayDialogue(enemyDialogue[d]);
            }
            else if (enemyDialogue[d].WhenToPlay == DialogueQueuePoint.ON_ENCOUNTER
                && WhatPointIsThis == DialogueQueuePoint.ON_ENCOUNTER)
            {
                DialogueManager.Instance.PlayDialogue(enemyDialogue[d]);
            }
            else if (enemyDialogue[d].WhenToPlay == DialogueQueuePoint.AT_HP
               && WhatPointIsThis == DialogueQueuePoint.AT_HP
               && EnemyHealth <= enemyDialogue[d].HpPoint && !enemyDialogue[d].HasBeenPlayed)
            {
                DialogueManager.Instance.PlayDialogue(enemyDialogue[d]);
                enemyDialogue[d].HasBeenPlayed = true;
            }
        }
    }

    void Update()
    {
        if (GameController.Instance.GetCurrentGameState
            != GameController.Instance.FlyingStateInstance)
        {
            return; //Only tick enemies in flyingstate
        }
        if (Vector2.Distance(PlayerShip.Instance.GetPos, transform.position) <= encounterDistance
            && !hasRunFirstEncounter)
        {
            HandleDialogue(DialogueQueuePoint.ON_ENCOUNTER);
            hasRunFirstEncounter = true;
        }
        Tick();
        if (waitingToDie && !deathVfx.isPlaying)
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        ResetDialogue();
    }

    void ResetDialogue()
    {
        for (int d = 0; d < enemyDialogue.Length; d++)
        {
            if (!enemyDialogue[d])
            {
                continue;
            }

            enemyDialogue[d].HasBeenPlayed = false;
        }
    }

    public virtual void Init()
    {

    }

    public virtual void Tick()
    {

    }
}
