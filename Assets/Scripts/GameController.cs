using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    Stack<GameStateBase> gameStates = new Stack<GameStateBase>();

    public GameStateBase GetCurrentGameState
    {
        get { return gameStates.Peek(); }
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

    const float TIME_TO_WAIT_VFX = 2.0f;
    //Map bounds
    const float MAX_Y_VAL = 5.0f;
    const float MAX_X_VAL = 20.0f;

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

        ResetAllLevels();

        //Temp until menu/state loading is added
        GoToState(flyingState);
    }

    void ResetAllLevels()
    {
        //SO's have some persistance so reset each start
        for(int l = 0; l < allLevels.Count; l++)
        {
            allLevels[l].EnemiesInScene = new List<EnemyBase>();
            if (!allLevels[l].IsSubLevel)
            {
                allLevels[l].SubLevel.EnemiesInScene = new List<EnemyBase>();
            }
        }
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
            //gameStates.Pop();
        }
       
        gameStates.Push(State);
        EnterNewState(State);
    }

    void ExitPrevState(GameStateBase StateToExit)
    {
        StateToExit.OnStateExit();
    }

    void EnterNewState(GameStateBase StateToEnter)
    {
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
            //TODO respawn`
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
