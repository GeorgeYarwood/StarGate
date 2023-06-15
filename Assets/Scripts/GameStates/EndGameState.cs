using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameState : GameStateBase
{
    [SerializeField] GameObject gameOverPanel;

    public override void OnStateEnter()
    {
        gameOverPanel.SetActive(true);
    }

    public override void OnStateExit()
    {
        gameOverPanel.SetActive(false);
    }

    public override void Tick()
    {
        
    }
}
