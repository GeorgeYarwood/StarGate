using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AbilityController : MonoBehaviour
{
    const int MAX_BOMBS = 6;
    public const int STARTING_BOMBS = 3;

    [SerializeField] GameObject[] bombImages = new GameObject[MAX_BOMBS];
    [SerializeField] ParticleSystem bombVFX;
    [SerializeField] AudioClip bombSFX;
    [SerializeField] float bombRange;

    static AbilityController instance;
    public static AbilityController Instance
    {
        get { return instance; }
    }

    //Reflect currently held bombs on UI

    static int heldBombs = STARTING_BOMBS;
    public static int HeldBombs
    {
        get { return heldBombs; }
        set { heldBombs = value; }
    }
    
    void Start()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        UpdateUI();

    }

    void UpdateUI() 
    {
        for (int i = 0; i < bombImages.Length; i++)
        {
            if (i > heldBombs - 1)
            {
                bombImages[i].SetActive(false);
            }
            else
            {
                bombImages[i].SetActive(true);
            }
        }
    }

    public void TryFireBomb() 
    {
        if(heldBombs <= 0)
        {
            return;
        }

        heldBombs--;
        UpdateUI();

        if (bombSFX)
        {
            AudioManager.Instance.PlayAudioClip(bombSFX);
        }
        StartCoroutine(AwaitExplosion());
    }

    IEnumerator AwaitExplosion()
    {
        //SFX has a delay/build up at start
        yield return new WaitForSeconds(0.85f);
        PlayerController PController = GameController.Instance.GetActivePlayerController();
        Vector2 PlayerPos = PController.GetPos;

        if (bombVFX)
        {
            bombVFX.transform.position = PlayerPos;
            bombVFX.Play();
        }

        //Find all enemies in range of player/bomb
        EnemyBase[] AllEnemies = FindObjectsOfType<EnemyBase>();
        foreach (EnemyBase Enemy in AllEnemies)
        {
            if (Vector2.Distance(PlayerPos, (Vector2)Enemy.transform.position) <= bombRange)
            {
                Enemy.OnDie(Dialog: false);
            }
        }
    }

    public void GrantBomb()
    {
        if(heldBombs >= MAX_BOMBS)
        {
            return;
        }

        heldBombs++;
        UpdateUI();
    }

    public void WriteBombCount()
    {
        PlayerPrefs.SetInt(InputHolder.HELD_BOMBS, heldBombs);
    }

    void Update()
    {
        
    }
}
