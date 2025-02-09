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
    const float CLEANUP_TIMER = 10.0f; //Will check to see if we're not on a background at this interval and destroy if so
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
    [SerializeField] PowerUp[] canDropOnDeath = new PowerUp[0];
    [SerializeField] bool alwaysDrop = false;
    [SerializeField] Dialogue[] enemyDialogue = new Dialogue[3];
    [SerializeField] bool onePerLevel = false;
    [SerializeField] float encounterDistance = 5.0f;    //How close until ON_ENCOUNTER dialogue is triggered
    [SerializeField] BaseProjectile projectile;
    [SerializeField] AudioClip shotSFX;

    public BaseProjectile Projectile
    {
        get { return projectile; }
    }
    [SerializeField] bool shootAtPlayer = false;
    public bool ShootAtPlayer
    {
        get { return shootAtPlayer; }
    }

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

    

    [SerializeField] EnemyType thisEnemyType;
    public EnemyType ThisEnemyType
    {
        get { return thisEnemyType; }
    }

    internal int backgroundLayerMask;

    [SerializeField] float timeBetweenShots = 1.5f;

    List<BaseProjectile> spawnedProjectiles = new List<BaseProjectile>();
    public List<BaseProjectile> SpawnedProjectiles
    {
        get { return spawnedProjectiles; }
        set { spawnedProjectiles = value; }
    }

   
    void Start()
    {
        if (ShootAtPlayer)
        {
            StartCoroutine(FireAtPlayer());
        }
        Init();
    }

    IEnumerator FireAtPlayer()
    {
        while (true && !waitingToDie)
        {
            if (GameController.Instance.GetCurrentGameState
                is FlyingState && CanFireAtPlayer())
            {
                if (shotSFX)
                {
                    AudioManager.Instance.PlayAudioClip(shotSFX);
                }

                LaunchProjectile();
                yield return new WaitForSeconds(timeBetweenShots);
            }
            yield return null;
        }
    }

    public virtual void LaunchProjectile()
    {
        //Handle in derived
    }

    public virtual bool CanFireAtPlayer() 
    {
        //Handle in derived

        return true;
    }

    void OnTriggerEnter2D(Collider2D Collision)
    {   
        if (Collision.TryGetComponent(out PlayerShip _) && !waitingToDie)
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
        if (GameController.VibrationEnabled)
        {
#if FOR_MOBILE
        Handheld.Vibrate();
#else
            if (ControllerManager.InputMethod is CurrentInputMethod.CONTROLLER)
            {
                ControllerManager.Instance.VibrateController();
            }
#endif
        }

        OnDie();
    }

    public virtual void OnDie(bool Dialog = true)
    {
        GameController.Instance.AddScore(scoreAddition);
        GameController.Instance.FlyingStateInstance.RemoveEnemyFromList(this);
        if (Dialog)
        {
            HandleDialogue(DialogueQueuePoint.ON_DEATH);
        }
        if (canDropOnDeath.Length > 0)
        {
            PowerUp toDrop = canDropOnDeath[0];
            if (!alwaysDrop && canDropOnDeath.Length > 1) 
            {
                toDrop = canDropOnDeath[Random.Range(0, canDropOnDeath.Length - 1)];
            }
            PowerUpManager.Instance.DropRandomPowerUpAtPosition(transform.position, toDrop, ForceSpawn: alwaysDrop);
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

        CleanList();
    }

    void ClearAllProjectiles()
    {
        for (int p = 0; p < spawnedProjectiles.Count; p++)
        {
            if (spawnedProjectiles[p])
            {
                Destroy(spawnedProjectiles[p].gameObject);
            }
        }

        spawnedProjectiles.Clear();
    }

    void CleanList() //Ensure we don't have dead references
    {
        for (int p = 0; p < spawnedProjectiles.Count; p++)
        {
            if (!spawnedProjectiles[p])
            {
                spawnedProjectiles.RemoveAt(p);
            }
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

    bool OnBackground(out Transform ColliderTransform)
    {
        ColliderTransform = null;
        Vector2 BoxSize = new Vector2(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y);
        RaycastHit2D Centre = Physics2D.BoxCast(transform.position, BoxSize, 0.0f, transform.forward, 1.0f, backgroundLayerMask);
        if (Centre.collider)
        {
            ColliderTransform = Centre.collider.transform;
        }
        return Centre.collider;
    }

    void DestroyToPeventSoftlock()
    {
        GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene.Remove(this);
        Debug.LogWarning(name + " " + WARNING_MESSAGE);
        Destroy(gameObject);
    }

    void ParentToRandomBackground()
    {
        Vector3 NewPos = Vector3.zero;
        int Sel = Random.Range(1, 3);
        if(Sel == 1)
        {
            NewPos = WorldScroller.Instance.LeftWorldSection.transform.position;
        }
        else if(Sel == 1)
        {
            NewPos = WorldScroller.Instance.MiddleWorldSection.transform.position;
        }
        else
        {
            NewPos = WorldScroller.Instance.RightWorldSection.transform.position;
        }

        transform.position = NewPos;
        ParentToBackground(DestroyIfFail: true);
    }

    public void ParentToBackground(bool DestroyIfFail = false)
    {
        Transform Hit;
        if (OnBackground(out Hit))
        {
            transform.parent = Hit;
            return;
        }

        if (DestroyIfFail)
        {
            DestroyToPeventSoftlock();
            return;
        }

        //Unable to parent to background, so just TP to random one
        ParentToRandomBackground();
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
        //ParentToBackground(); Doing this on Init can fail if bg is being repositioned, so just wait until we need to do it later
    }

    public virtual void Tick()
    {

    }
}
