using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossEnemy : MovingEnemy
{
    const string BACKGROUND_LAYER_MASK = "Background";

    [SerializeField] int waveDifficultyScale = 3; //Amount of enemies added on for each wave level

    [SerializeField] List<EnemyWave> waves = new List<EnemyWave>();

    [SerializeField] EnemyBase[] enemyTypesToSpawn = new EnemyBase[2];

    [SerializeField] List<CrackHp> healthCrackPoints = new List<CrackHp>();

    [Serializable]
    struct EnemyWave
    {
        public int HealthThreshhold;
        internal bool HasBeenTriggered;
    }

    [Serializable]
    struct CrackHp
    {
        public int HealthThreshhold;
        internal bool HasBeenTriggered;
        public GameObject CrackImage;
    }

    void Start()
    {
        waves.OrderByDescending(X => X);
        base.backgroundLayerMask = LayerMask.GetMask(BACKGROUND_LAYER_MASK); //I guess double inheritance isn't a thing cus this don work in EnemyBase for BossEnemies
    }

    public override void OnHit(int DamageToDeduct)
    {
        base.OnHit(DamageToDeduct);

        for(int c = 0; c < healthCrackPoints.Count; c++)
        {
            if (healthCrackPoints[c].HasBeenTriggered)
            {
                continue;
            }

            if(EnemyHealth <= healthCrackPoints[c].HealthThreshhold)
            {
                TriggerCrack(healthCrackPoints[c].CrackImage);
                CrackHp ModifiedHp = healthCrackPoints[c];
                ModifiedHp.HasBeenTriggered = true;
                healthCrackPoints[c] = ModifiedHp;
                break;
            }
        }

        for (int w = 0; w < waves.Count; w++)
        {
            if (waves[w].HasBeenTriggered)
            {
                continue;
            }

            if (EnemyHealth <= waves[w].HealthThreshhold)
            {
                EnemyWave ModifiedWave = waves[w];
                ModifiedWave.HasBeenTriggered = true;
                TriggerWave(w);
                waves[w] = ModifiedWave;
                break;
            }
        }
    }

    IEnumerator SpawnEnemy(int AmountToSpawn)
    {
        int CurrentEnemyCount = GetCurrentLevel().EnemiesInScene.Count;

        while (GetCurrentLevel().EnemiesInScene.Count < (CurrentEnemyCount + AmountToSpawn)) //A little hacky, we just add the enemies to the current level like if we were loading a level
        {
            GameController.Instance.FlyingStateInstance.SpawnEnemies
                (GetCurrentLevel().EnemiesInScene, enemyTypesToSpawn);
            yield return null;
        }
    }

    LevelObject GetCurrentLevel()
    {
        return GameController.AllLevels[GameController.CurrentLevel];
    }

    void TriggerWave(int Level)
    {
        StartCoroutine(SpawnEnemy(Level + waveDifficultyScale));
    }

    void TriggerCrack(GameObject CrackImage)
    {
        CrackImage.SetActive(true);
    }
}
