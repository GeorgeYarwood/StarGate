using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossEnemy : MovingEnemy
{
    [SerializeField] int waveDifficultyScale = 3; //Amount of enemies added on for each wave level

    [SerializeField] List<EnemyWave> waves = new List<EnemyWave>();

    [SerializeField] EnemyBase[] enemyTypesToSpawn = new EnemyBase[2];

    [Serializable]
    struct EnemyWave
    {
        public int HealthThreshhold;
        internal bool HasBeenTriggered;
    }

    void Start()
    {
        waves.OrderByDescending(X => X);
    }

    public override void OnHit(int DamageToDeduct)
    {
        base.OnHit(DamageToDeduct);

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
}
