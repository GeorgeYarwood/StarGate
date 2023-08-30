using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType //Used for comparison checks
{
    NORMAL,
    BOSS,
    BOSS_MASSIVE
}

public class EnemyBase : MonoBehaviour
{
    const string BACKGROUND_LAYER_MASK = "Background";
    const string WARNING_MESSAGE = "This enemy couldn't find a background to parent to, so has destroyed itself to prevent a softlock! This isn't good!";

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

    [SerializeField] internal SpriteRenderer spriteRenderer;
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

    const float MAP_EDGE_OFFSET = 7.5f;

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

    internal int backgroundLayerMask;

    void OnTriggerEnter2D(Collider2D Collision)
    {
        if (Collision.TryGetComponent(out LaserProjectile HitProjectile))
        {
            if (!HitProjectile.IgnoreEnemy)
            {
                OnHit(HitProjectile.HitDamage);
            }
        }
        else if (Collision.TryGetComponent(out PlayerShip _) && !waitingToDie)
        {
            PlayerShip.Instance.OnCollisionWithEnemy();
        }
    }

    public virtual void OnHit(int DamageToDeduct)
    {
        if (waitingToDie)
        {
            return;
        }

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
        if (GameController.Instance.GetCurrentGameState != GameController.Instance.FlyingStateInstance)
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

    public void ParentToBackground()
    {
        RaycastHit2D Centre = Physics2D.Raycast(transform.position, transform.forward, 1.0f, backgroundLayerMask);
        if (Centre.collider)
        {
            transform.parent = Centre.collider.transform;
            return;
        }
        //Just destroy this if it can't find a background to parent to so the game won't get softlocked
        GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene.Remove(this);
        Debug.LogWarning(WARNING_MESSAGE);
        Destroy(gameObject);
    }

    public void CorrectPosition(float Xadjustment, float Xmin, float Xmax, bool Positive)
    {
        //This will put the enemies on the inner edge, so to prevent visible pop-in, we need to push them out a bit
        if(Positive)
        {
            transform.position = new(Mathf.Clamp(transform.position.x + Xadjustment - MAP_EDGE_OFFSET, Xmin, Xmax), transform.position.y, transform.position.z);
        }
        else
        {
            transform.position = new(Mathf.Clamp(transform.position.x - Xadjustment + MAP_EDGE_OFFSET, Xmin, Xmax), transform.position.y, transform.position.z);
        }
    }

    public void DetachFromParent()
    {
        transform.parent = null;
    }

    public virtual void Init()
    {
        backgroundLayerMask = LayerMask.GetMask(BACKGROUND_LAYER_MASK);
        ParentToBackground();
    }

    public virtual void Tick()
    {

    }
}
