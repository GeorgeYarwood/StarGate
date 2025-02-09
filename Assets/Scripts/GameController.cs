using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public enum GameStates 
{ 
    FLYING,
    END_GAME,
    LEVEL_COMPLETE,
    LIFE_LOST,
    DEATH,
    PAUSE,
    GROUNDED
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
        set { currentLevel = value; }
    }

    static int currentScore = 0;
    public static int CurrentScore
    {
        get { return currentScore; }
        set { currentScore = value; }
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

    public static float GetMapBoundsMinXVal
    {
        get { return WorldScroller.Instance.GetMinXBounds(); }
    }
    public static float GetMapBoundsMaxXVal
    {
        get { return WorldScroller.Instance.GetMaxXBounds(); }
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
    [SerializeField] GroundedState groundedState;


    [SerializeField] TextMeshProUGUI scoreText;

    const float TIME_TO_WAIT_VFX = 0.25f;
    //Map bounds
    const float MAX_Y_VAL = 4.0f;
    const float MAX_X_VAL = 28.0f;

    //Spawning X limits so we don't spawn in the centre (Where the player is)
    const float X_SPAWN_ADJUSTMENT_VAL = 4.5f;

    bool blockStateExit = false;
    public bool BlockStateExit
    {
        set { blockStateExit = value; }
    }

    public static bool VibrationEnabled
    {
        get { return Convert.ToBoolean(PlayerPrefs.GetInt(InputHolder.VIBRATION)); }
    }

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

        if(CurrentLevel > 0 && PlayerPrefs.HasKey(InputHolder.CURRENT_LIVES)) //Keep compatibility with older prefs
        {
            currentLives = PlayerPrefs.GetInt(InputHolder.CURRENT_LIVES);
        }
        else
        {
            currentLives = startingLives;
            PlayerPrefs.SetInt(InputHolder.CURRENT_LIVES, currentLives);
        }

        //PlayerShip.Instance.SetEnabled(false);
        PlayerTurret.Instance.SetEnabled(false);

        ResetAllLevels();
        ResetPlayerPosition();
        //GoToState(groundedState);
        GoToState(flyingState);
    }

    public PlayerController GetActivePlayerController() 
    {
        GameStateBase CurrentState = GetCurrentGameState;

        if (CurrentState is not PlayState)
        {
            return null;
        }

        if (CurrentState is FlyingState) 
        {
            return PlayerShip.Instance;
        }

        return PlayerTurret.Instance;
    }

    public void ResetPlayerPosition()
    {
        PlayerTurret.Instance.transform.position = new(WorldScroller.Instance.GetCurrentCentre(), -4.15f);
        PlayerShip.Instance.transform.position = new(WorldScroller.Instance.GetCurrentCentre(), 0.0f);
    }

    public void ResetAllLevels()
    {
        //SO's have some persistance so reset each start
        for(int l = 0; l < allLevels.Count; l++)
        {
            allLevels[l].IsInitialised = false;
            allLevels[l].EnemiesInScene = new List<EnemyBase>();
            if (!allLevels[l].IsSubLevel && allLevels[l].SubLevel)
            {
                allLevels[l].SubLevel.EnemiesInScene = new List<EnemyBase>();
                allLevels[l].SubLevel.IsInitialised = false;
            }
        }

        if(currentLevel >= allLevels.Count)
        {
            currentLevel = 0;   //We should never get here
        } 
    }

    public void GoToPreviousGameState()
    {
        if (blockStateExit)
        {
            return;
        }
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
        if (blockStateExit)
        {
            return;
        }
        if(gameStates.Count > 0)
        {
            ExitPrevState(gameStates.Peek());
        }
       
        gameStates.Push(State);
        EnterNewState(State);
    }

    public void GoToState(GameStates State)
    {
        if (blockStateExit)
        {
            return;
        }
        if (gameStates.Count > 0)
        {
            ExitPrevState(gameStates.Peek());
        }

        GameStateBase StateToLoad = null;

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
            case GameStates.GROUNDED:
                StateToLoad = groundedState;
                break;
        }

        if (!StateToLoad)
        {
            return;
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
        Cursor.visible = StateToEnter.AllowCursorVisible;
        Cursor.lockState = StateToEnter.AllowCursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    void TickCurrentState()
    {
        if(gameStates.Count == 0)
        {
            return;
        }
        gameStates.Peek().Tick();
    }

    void Update() => TickCurrentState();

    public void OnPlayerDeath()
    {
        if(currentLives - 1 >= 0)
        {
            currentLives--;
            PlayerPrefs.SetInt(InputHolder.CURRENT_LIVES, currentLives);    //Save this so we can't cheat by exiting and re-entering
            PowerUpManager.Instance.EndAllPowerUps();
            PowerUpManager.Instance.ClearPowerUps();
            DestroyAllProjectiles();
            GoToState(lifeLostState);
            return;
        }

        EndGame();
    }

    void EndGame()
    {
        PlayerPrefs.SetInt(InputHolder.LAST_LEVEL, 0); //If you die, you can't resume your last level
        StartCoroutine(WaitForDeathVfx());
    }

    public void OnLevelComplete()
    {
        PowerUpManager.Instance.EndAllPowerUps();
        PowerUpManager.Instance.ClearPowerUps();
        AbilityController.Instance.WriteBombCount();
        currentLevel++;
        PlayerPrefs.SetInt(InputHolder.LAST_LEVEL, CurrentLevel);
        PlayerPrefs.SetInt(InputHolder.SCORE, currentScore);
        GoToState(levelCompleteState);
        PlayerController.Invincible = false;
    }

    IEnumerator WaitForDeathVfx()
    {
        yield return new WaitForSeconds(TIME_TO_WAIT_VFX);
        GoToState(endGameState);
    }

    public void AddScore(int ScoreToAdd)
    {
        currentScore += ScoreToAdd;
        scoreText.text = currentScore.ToString();
    }

    public void DestroyAllProjectiles(bool EnemyOnly = false)
    {
        //I could be less lazy and like cache these or something but cba
        BaseProjectile[] FoundProjectiles = FindObjectsOfType<BaseProjectile>();
        for(int l = 0; l < FoundProjectiles.Length; l++)
        {
            if(EnemyOnly && !FoundProjectiles[l].IgnoreEnemy)
            {
                continue;
            }
            Destroy(FoundProjectiles[l].gameObject);
        }
    }

    public BaseProjectile[] GetAllProjectiles()
    {
        return FindObjectsOfType<BaseProjectile>();
    }
}
