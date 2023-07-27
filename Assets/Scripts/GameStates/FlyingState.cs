using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using Random = UnityEngine.Random;
public class FlyingState : GameStateBase
{
    const string ERROR_MESSAGE = "No levels added to FlyingState gameobject! Returning out";
    const string INFINITE_LOOP_ERROR_MESSAGE = "Infinite loop detected! The enemy type for this level is set to only allow one per level, but the enemies per level is set higher than 1!";

    List<GameObject> projectilesInScene = new List<GameObject>();
    public List<GameObject> ProjectilesInScene
    {
        get { return projectilesInScene; }
    }

    bool waitingForStateExit = false;

    public override void OnStateEnter()
    {
        if (GameController.AllLevels.Count == 0)
        {
            Debug.Log(ERROR_MESSAGE);
            return;
        }
        if (!GameController.AllLevels[GameController.CurrentLevel].IsInitialised
            && (!GameController.AllLevels[GameController.CurrentLevel].HasSublevel 
            ||!GameController.AllLevels[GameController.CurrentLevel].SubLevel.IsInitialised))
        {
            GameController.Instance.ResetPlayerPosition();
            LoadLevel(GameController.AllLevels[GameController.CurrentLevel]);
        }
        else if (GameController.AllLevels[GameController.CurrentLevel].IsInitialised
            && GameController.Instance.LastGameState is not PauseGameState) //Reset spawns if we're already in the level
        {
            ResetEnemyPositions();
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        AudioManager.Instance.PlayLoopedAudioClip(
            GameController.AllLevels[GameController.CurrentLevel].LevelSong,
            OnlyPermitOne: true, IsMusic: true);
    }

    void ResetEnemyPositions()
    {
        for (int e = 0; e < GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene.Count; e++)
        {
            GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].transform.position
                = ReturnRandomSpawnPositionInRange();
        }
    }

    public override void OnStateExit()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        waitingForStateExit = false;
        ClearAllProjectiles();
        if(GameController.CurrentLevel > 0) //First level shares song with main menu... just a happy little accident
        {
            AudioManager.Instance.PlayLoopedAudioClip(
                GameController.AllLevels[GameController.CurrentLevel - 1].LevelSong,
                EndLoop: true);
        }
    }

    void ClearAllProjectiles()
    {
        for (int p = 0; p < projectilesInScene.Count; p++)
        {
            if (projectilesInScene[p])
            {
                Destroy(projectilesInScene[p].gameObject);
            }
        }

        projectilesInScene.Clear();
    }

    public override void Tick()
    {
        GetInput();
        TrackPlayerWithCamera();
        CheckIfLevelComplete();
        CleanList();
    }

    void CleanList()
    {
        for (int p = 0; p < projectilesInScene.Count; p++)
        {
            if (!projectilesInScene[p])
            {
                projectilesInScene.RemoveAt(p);
            }
        }
    }

    void CheckIfLevelComplete()
    {
        if (GameController.AllLevels.Count == 0 || waitingForStateExit)
        {
            return;
        }

        LevelObject CurrentLevel = GameController.AllLevels[GameController.CurrentLevel];

        if (CurrentLevel.HasSublevel)
        {
            if ((CurrentLevel.EnemiesInScene.Count + CurrentLevel.SubLevel.EnemiesInScene.Count) <= 0
                && CurrentLevel.SubLevel.IsInitialised && CurrentLevel.IsInitialised)
            {
                StartCoroutine(EndLevel());
            }
        }
        else
        {
            if (CurrentLevel.EnemiesInScene.Count <= 0 && CurrentLevel.IsInitialised)
            {
                StartCoroutine(EndLevel());
            }
        }

    }

    IEnumerator EndLevel()
    {
        waitingForStateExit = true;
        yield return new WaitUntil(() => HandleDialogue(DialogueQueuePoint.LEVEL_END, GameController.AllLevels[GameController.CurrentLevel]));
        if (GameController.AllLevels[GameController.CurrentLevel].SubLevel)
        {
            yield return new WaitUntil(() => HandleDialogue(DialogueQueuePoint.LEVEL_END, GameController.AllLevels[GameController.CurrentLevel].SubLevel));
        }

        StartCoroutine(WaitForEndOfDialogue());
    }

    IEnumerator WaitForEndOfDialogue()
    {
        GameController.Instance.BlockStateExit = true;
        yield return new WaitUntil(() => DialoguePanel.Instance.PanelActive == false);
        GameController.Instance.BlockStateExit = false;
        GameController.Instance.OnLevelComplete();
    }

    void TrackPlayerWithCamera()
    {
        CameraController.Instance.UpdatePosition(PlayerShip.Instance.transform.position, TrackAxis.X_AXIS);
    }

    public void RemoveEnemyFromList(EnemyBase EnemyToRemove)
    {
        if (GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene.Contains(EnemyToRemove))
        {
            GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene.Remove(EnemyToRemove);
        }
        else if (GameController.AllLevels[GameController.CurrentLevel].SubLevel.EnemiesInScene.Contains(EnemyToRemove))
        {
            GameController.AllLevels[GameController.CurrentLevel].SubLevel.EnemiesInScene.Remove(EnemyToRemove);
        }
    }

    void GetInput()
    {
        if (Input.GetButton(InputHolder.MOVE_UP))
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.UP);
        }
        if (Input.GetButton(InputHolder.MOVE_DOWN))
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.DOWN);
        }
        if (Input.GetButton(InputHolder.MOVE_LEFT))
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.LEFT);
        }
        if (Input.GetButton(InputHolder.MOVE_RIGHT))
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.RIGHT);
        }
        if (Input.GetButton(InputHolder.FIRE))
        {
            PlayerShip.Instance.FireProjectile();
        }

        if (Input.GetButtonDown(InputHolder.PAUSE_MENU))
        {
            GameController.Instance.GoToState(GameStates.PAUSE);
        }
        //Listen for release so we can coast

        if (Input.anyKey)
        {
            return;
        }

        if (Input.GetButtonUp(InputHolder.MOVE_UP))
        {
            PlayerShip.Instance.CoastPlayerCoroutine =
                StartCoroutine(PlayerShip.Instance.CoastPlayer());
        }
        if (Input.GetButtonUp(InputHolder.MOVE_DOWN))
        {
            PlayerShip.Instance.CoastPlayerCoroutine =
                StartCoroutine(PlayerShip.Instance.CoastPlayer());
        }
        if (Input.GetButtonUp(InputHolder.MOVE_LEFT))
        {
            PlayerShip.Instance.CoastPlayerCoroutine =
                StartCoroutine(PlayerShip.Instance.CoastPlayer());
        }
        if (Input.GetButtonUp(InputHolder.MOVE_RIGHT))
        {
            PlayerShip.Instance.CoastPlayerCoroutine =
                StartCoroutine(PlayerShip.Instance.CoastPlayer());
        }
    }

    public void LoadLevel(LevelObject LevelToLoad)
    {
        ClearAllProjectiles();
        if (LevelToLoad.IsInitialised)
        {
            int MaxIterator;
            if (LevelToLoad.IsSubLevel) //We will never load a sublevel directly so we don't need to worry about it being the sublevel of a different level
            {
                MaxIterator = GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene.Count;
            }
            else
            {
                //Disable enemies in the level we're LEAVING
                MaxIterator = GameController.AllLevels[GameController.CurrentLevel].SubLevel.EnemiesInScene.Count;
            }
            for (int e = 0; e < MaxIterator; e++)
            {
                if (LevelToLoad.IsSubLevel)
                {
                    GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene[e].gameObject.SetActive(false);
                }
                else
                {
                    GameController.AllLevels[GameController.CurrentLevel].SubLevel.EnemiesInScene[e].gameObject.SetActive(false);
                }
            }
            for (int e = 0; e < LevelToLoad.EnemiesInScene.Count; e++)
            {
                LevelToLoad.EnemiesInScene[e].gameObject.SetActive(true);
            }

            return;
        }
        else if (LevelToLoad.IsSubLevel)
        {
            SublevelEntrance.Instance.IsInSublevel = true;
            if (LevelToLoad.ParentLevel.IsInitialised)
            {
                for (int e = 0; e < LevelToLoad.ParentLevel.EnemiesInScene.Count; e++)
                {
                    LevelToLoad.ParentLevel.EnemiesInScene[e].gameObject.SetActive(false);
                }
            }
        }
        else if (!LevelToLoad.IsSubLevel)
        {
            SublevelEntrance.Instance.ToggleSublevel(LevelToLoad.HasSublevel);
            SublevelEntrance.Instance.IsInSublevel = false;
        }

        StartCoroutine(HandleSpawnEnemies(LevelToLoad));

        HandleDialogue(DialogueQueuePoint.LEVEL_START, LevelToLoad);

        LevelToLoad.IsInitialised = true;
    }

    IEnumerator HandleSpawnEnemies(LevelObject LevelToLoad)
    {
        while (LevelToLoad.EnemiesInScene.Count < LevelToLoad.EnemiesPerLevel)
        {
            SpawnEnemies(LevelToLoad.EnemiesInScene, LevelToLoad.EnemyTypesToSpawn);
            if (LevelToLoad.EnemiesInScene.Count == 1 && LevelToLoad.EnemiesInScene[0].OnePerLevel
                && LevelToLoad.EnemiesPerLevel > 1 && LevelToLoad.EnemyTypesToSpawn.Length == 1)
            {
                Debug.Log(INFINITE_LOOP_ERROR_MESSAGE);
                break;
            }
            yield return null;
        }
    }

    bool HandleDialogue(DialogueQueuePoint WhatPointIsThis, LevelObject ThisLevel)
    {
        if (ThisLevel.LevelDialogue.Length > 0)
        {
            for (int d = 0; d < ThisLevel.LevelDialogue.Length; d++)
            {
                if (!ThisLevel.LevelDialogue[d])
                {
                    continue;
                }

                if (ThisLevel.LevelDialogue[d].WhenToPlay == WhatPointIsThis)
                {
                    DialogueManager.Instance.PlayDialogue(ThisLevel.LevelDialogue[d]);
                }
            }
        }

        return true;
    }

    public void SpawnEnemies(List<EnemyBase> ListToAddTo, EnemyBase[] EnemyTypes)
    {
        int RandomEnemyType = Random.Range(0, EnemyTypes.Length);
        if (EnemyTypes[RandomEnemyType].OnePerLevel)
        {
            for(int e = 0; e < ListToAddTo.Count; e++)
            {
                if (ListToAddTo[e].ThisEnemyType == EnemyTypes[RandomEnemyType].ThisEnemyType)
                {
                    return;
                }
            }
        }
        EnemyBase ThisEnemy = Instantiate(EnemyTypes[RandomEnemyType],
            ReturnRandomSpawnPositionInRange(), Quaternion.identity);
        ListToAddTo.Add(ThisEnemy);
    }

    Vector2 ReturnRandomSpawnPositionInRange()
    {
    GetNewPosition:
        bool LeftSpawn = Convert.ToBoolean(Random.Range(0, 2));

        float SpawnVal = 0.0f;
        if (LeftSpawn)
        {
            SpawnVal = Random.Range(-GameController.GetMapBoundsXVal, 0.0f -
                GameController.GetSpawnAdjustmentXVal);
        }
        else
        {
            SpawnVal = Random.Range(GameController.GetMapBoundsXVal, 0.0f +
                GameController.GetSpawnAdjustmentXVal);
        }

        Vector2 RandomSpawnPosition = new(SpawnVal, Random.Range(GameController.GetMapBoundsYVal,
            -GameController.GetMapBoundsYVal));
        if (Mathf.Approximately(PlayerShip.Instance.transform.position.x, RandomSpawnPosition.x)
            || Mathf.Approximately(PlayerShip.Instance.transform.position.y, RandomSpawnPosition.y))
        {
            goto GetNewPosition;    //Programming like it's 1970
        }

        return RandomSpawnPosition;
    }
}
