using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeLostState : GameStateBase
{
    [SerializeField] LifeLostPanel lifeLostPanel;
    const float STATE_ENTRY_WAIT = 1.0f;
    bool canExitState = false;

    public override void OnStateEnter()
    {
        canExitState = false;
        lifeLostPanel.SetRemainingLives(GameController.CurrentLives);
        lifeLostPanel.gameObject.SetActive(true);
        StartCoroutine(WaitForInteraction());
    }

    IEnumerator WaitForInteraction()
    {
        yield return new WaitForSeconds(STATE_ENTRY_WAIT);
        canExitState = true;
    }

    public override void Tick()
    {
        if((Input.anyKey || ControllerManager.GetInput == ControllerInput.SELECT) && canExitState)
        {
            GameController.Instance.GoToPreviousGameState();
        }
    }

    public override void OnStateExit()
    {
        lifeLostPanel.gameObject.SetActive(false);
        GameController.Instance.ResetPlayerPosition();
    }
}
