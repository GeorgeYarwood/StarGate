using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingState : GameStateBase
{
    //Input buttons
    const string MOVE_LEFT = "MoveLeft";
    const string MOVE_RIGHT = "MoveRight";
    const string MOVE_UP = "MoveUp";
    const string MOVE_DOWN = "MoveDown";
    const string FIRE = "Fire";

    const string ERROR_MESSAGE = "No levels added to FlyingState gameobject! Returning out";

    public override void OnStateEnter()
    {
        if (GameController.AllLevels.Count == 0)
        {
            Debug.Log(ERROR_MESSAGE);
            return;
        }
        LoadLevel(GameController.AllLevels[GameController.CurrentLevel]);
    }

    public override void OnStateExit()
    {

    }

    public override void Tick()
    {
        GetInput();
        TrackPlayerWithCamera();
        CheckIfLevelComplete();
    }

    void CheckIfLevelComplete()
    {
        if (GameController.AllLevels.Count == 0)
        {
            return;
        }

        if ((GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene.Count
            + GameController.AllLevels[GameController.CurrentLevel].SubLevel.EnemiesInScene.Count) <= 0)
        {
            GameController.Instance.OnLevelComplete();
        }
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
        if (Input.GetButton(MOVE_UP))
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.UP);
        }
        if (Input.GetButton(MOVE_DOWN))
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.DOWN);
        }
        if (Input.GetButton(MOVE_LEFT))
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.LEFT);
        }
        if (Input.GetButton(MOVE_RIGHT))
        {
            PlayerShip.Instance.UpdatePosition(MoveDirection.RIGHT);
        }
        if (Input.GetButton(FIRE))
        {
            PlayerShip.Instance.FireProjectile();
        }

        //Listen for release so we can coast

        if (Input.anyKey)
        {
            return;
        }

        if (Input.GetButtonUp(MOVE_UP))
        {
            PlayerShip.Instance.CoastPlayerCoroutine =
                StartCoroutine(PlayerShip.Instance.CoastPlayer());
        }
        if (Input.GetButtonUp(MOVE_DOWN))
        {
            PlayerShip.Instance.CoastPlayerCoroutine =
                StartCoroutine(PlayerShip.Instance.CoastPlayer());
        }
        if (Input.GetButtonUp(MOVE_LEFT))
        {
            PlayerShip.Instance.CoastPlayerCoroutine =
                StartCoroutine(PlayerShip.Instance.CoastPlayer());
        }
        if (Input.GetButtonUp(MOVE_RIGHT))
        {
            PlayerShip.Instance.CoastPlayerCoroutine =
                StartCoroutine(PlayerShip.Instance.CoastPlayer());
        }
    }

    public void LoadLevel(LevelObject LevelToLoad)
    {
        if (LevelToLoad.EnemiesInScene.Count > 0 || GameController.AllLevels[GameController.CurrentLevel].EnemiesInScene.Count > 0)
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
        //Initialise sublevel so we can check if all enemies are dead if it's never entered 
        else if (!LevelToLoad.IsSubLevel)
        {
            while (LevelToLoad.SubLevel.EnemiesInScene.Count < LevelToLoad.SubLevel.EnemiesPerLevel)
            {
                SpawnEnemies(LevelToLoad.SubLevel.EnemiesInScene, LevelToLoad.SubLevel.EnemyTypesToSpawn);
            }
            for (int e = 0; e < LevelToLoad.SubLevel.EnemiesInScene.Count; e++)
            {
                LevelToLoad.SubLevel.EnemiesInScene[e].gameObject.SetActive(false);
            }
        }

        while (LevelToLoad.EnemiesInScene.Count < LevelToLoad.EnemiesPerLevel)
        {
            SpawnEnemies(LevelToLoad.EnemiesInScene, LevelToLoad.EnemyTypesToSpawn);
        }
    }

    void SpawnEnemies(List<EnemyBase> ListToAddTo, EnemyBase[] EnemyTypes)
    {
        int RandomEnemyType = Random.Range(0, EnemyTypes.Length);
    GetNewPosition:
        Vector2 RandomSpawnPosition = new(Random.Range(-GameController.GetMapBoundsXVal,
            GameController.GetMapBoundsXVal), Random.Range(GameController.GetMapBoundsYVal,
            -GameController.GetMapBoundsYVal));
        if (Mathf.Approximately(PlayerShip.Instance.transform.position.x, RandomSpawnPosition.x)
            || Mathf.Approximately(PlayerShip.Instance.transform.position.y, RandomSpawnPosition.y))
        {
            goto GetNewPosition;    //Programming like it's 1970
        }

        EnemyBase ThisEnemy = Instantiate(EnemyTypes[RandomEnemyType],
            RandomSpawnPosition, Quaternion.identity);

        ListToAddTo.Add(ThisEnemy);
    }
}
