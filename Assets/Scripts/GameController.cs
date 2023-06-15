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

    int currentScore = 0;
    public int CurrentScore
    {
        get { return currentScore; }
    }

    const int SCORE_ADDITION_AMOUNT = 150; //Temp until per-enemy scores added

    [SerializeField] GameStateBase flyingState; //Temp until dynamic state loading added

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

        //Temp until menu/state loading is added
        GoToState(flyingState);
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
            gameStates.Pop();
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

    public void AddScore()
    {
        currentScore += SCORE_ADDITION_AMOUNT;
    }
}
