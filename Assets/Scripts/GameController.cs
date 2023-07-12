using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStates 
{ 
    FLYING,
    END_GAME,
    LEVEL_COMPLETE,
    LIFE_LOST,
    DEATH,
    PAUSE
}

public class GameController : MonoBehaviour
{
    Stack<GameStateBase> gameStates = new Stack<GameStateBase>();

    public GameStateBase GetCurrentGameState
    {
        get { return gameStates.Peek(); }
    }

    GameStateBase lastGameState;
    public GameStateBase LastGameState
    {
        get { return lastGameState; }
    }

    static GameController instance;
    public static GameController Instance
    {
        get { return instance; }
    }

    static int currentLevel = 0;
    public static int CurrentLevel
    {
        get { return currentLevel; }
    }

    static int currentScore = 0;
    public static int CurrentScore
    {
        get { return currentScore; }
    }

    static int currentLives = 3;
    public static int CurrentLives
    {
        get { return currentLives; }
    }

    public static float GetMapBoundsYVal
    {
        get { return MAX_Y_VAL; }
    }

    public static float GetMapBoundsXVal
    {
        get { return MAX_X_VAL; }
    }

    public static float GetSpawnAdjustmentXVal
    {
        get { return X_SPAWN_ADJUSTMENT_VAL; }
    }

    [SerializeField] List<LevelObject> allLevels = new List<LevelObject>();
    public static List<LevelObject> AllLevels
    {
        get { return GameController.instance.allLevels; }
    }

    [SerializeField] int startingLives = 3;

    //All available states
    [SerializeField] FlyingState flyingState;
    public FlyingState FlyingStateInstance
    {
        get { return flyingState; }
    }
    [SerializeField] EndGameState endGameState;
    [SerializeField] LevelCompleteState levelCompleteState;
    [SerializeField] LifeLostState lifeLostState;
    [SerializeField] PauseGameState pauseGameState;

    const float TIME_TO_WAIT_VFX = 0.25f;
    //Map bounds
    const float MAX_Y_VAL = 4.0f;
    const float MAX_X_VAL = 20.0f;

    //Spawning X limits so we don't spawn in the centre (Where the player is)
    const float X_SPAWN_ADJUSTMENT_VAL = 6.0f;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        currentLives = startingLives;
        ResetAllLevels();
        ResetPlayerPosition();
        //Temp until menu/state loading is added
        GoToState(flyingState);
    }

    public void ResetPlayerPosition()
    {
        PlayerShip.Instance.transform.position = Vector3.zero;
    }

    void ResetAllLevels()
    {
        //SO's have some persistance so reset each start
        for(int l = 0; l < allLevels.Count; l++)
        {
            for(int e = 0; e < allLevels[l].EnemyTypesToSpawn.Length; e++)
            {
                allLevels[l].EnemyTypesToSpawn[e].IsSpawned = false;
            }
            allLevels[l].IsInitialised = false;
            allLevels[l].EnemiesInScene = new List<EnemyBase>();
            if (!allLevels[l].IsSubLevel)
            {
                allLevels[l].SubLevel.EnemiesInScene = new List<EnemyBase>();
                allLevels[l].SubLevel.IsInitialised = false;
                for (int e = 0; e < allLevels[l].SubLevel.EnemyTypesToSpawn.Length; e++)
                {
                    allLevels[l].SubLevel.EnemyTypesToSpawn[e].IsSpawned = false;
                }
            }
        }

        currentLevel = 0;
    }

    public void GoToPreviousGameState()
    {
        if (gameStates.Count == 1)
        {
            return;
        }
        ExitPrevState(gameStates.Peek());
        gameStates.Pop();
        EnterNewState(gameStates.Peek());
    }

    public void GoToState(GameStateBase State)
    {
        if(gameStates.Count > 0)
        {
            ExitPrevState(gameStates.Peek());
        }
       
        gameStates.Push(State);
        EnterNewState(State);
    }

    public void GoToState(GameStates State)
    {
        if (gameStates.Count > 0)
        {
            ExitPrevState(gameStates.Peek());
        }

        GameStateBase StateToLoad = new();

        switch (State)
        {
            case GameStates.FLYING:
                StateToLoad = flyingState;
                break;
            case GameStates.END_GAME:
                StateToLoad = endGameState;
                break;
            case GameStates.LEVEL_COMPLETE:
                StateToLoad = levelCompleteState;
                break;
            case GameStates.LIFE_LOST:
                StateToLoad = lifeLostState;
                break;
            case GameStates.PAUSE:
                StateToLoad = pauseGameState;
                break;
        }

        gameStates.Push(StateToLoad);
        EnterNewState(StateToLoad);
    }

    void ExitPrevState(GameStateBase StateToExit)
    {
        StateToExit.OnStateExit();
        lastGameState = StateToExit;
    }

    void EnterNewState(GameStateBase StateToEnter)
    {
        AudioManager.Instance.StopAllLoops();
        StateToEnter.OnStateEnter();
    }

    void TickCurrentState()
    {
        if(gameStates.Count == 0)
        {
            return;
        }
        gameStates.Peek().Tick();
    }

    void Update()
    {
        TickCurrentState();
    }

    public void OnPlayerDeath()
    {
        if(currentLives - 1 >= 0)
        {
            currentLives--;
            GoToState(lifeLostState);
            return;
        }

        EndGame();
    }

    void EndGame()
    {
        StartCoroutine(WaitForDeathVfx());
    }

    public void OnLevelComplete()
    {
        currentLevel++;
        GoToState(levelCompleteState);
    }

    IEnumerator WaitForDeathVfx()
    {
        yield return new WaitForSeconds(TIME_TO_WAIT_VFX);
        GoToState(endGameState);
    }

    public void AddScore(int ScoreToAdd)
    {
        currentScore += ScoreToAdd;
    }
}
