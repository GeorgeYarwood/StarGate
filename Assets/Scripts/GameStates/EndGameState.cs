using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameState : GameStateBase
{
    [SerializeField] GameObject gameOverPanel;
    const float STATE_ENTRY_WAIT = 1.0f;
    bool canExitState = false;

    const string MAIN_MENU_SCENE = "MainMenu";  //Should really make this a state too but cba

    public override void OnStateEnter()
    {
        gameOverPanel.SetActive(true);
        StartCoroutine(WaitForInteraction());
    }

    IEnumerator WaitForInteraction()
    {
        yield return new WaitForSeconds(STATE_ENTRY_WAIT);
        canExitState = true;
    }

    public override void OnStateExit()
    {
        gameOverPanel.SetActive(false);
    }

    public override void Tick()
    {
        if ((Input.anyKey || ControllerManager.GetInput[(int)ControllerInput.A_BUTTON].Pressed) && canExitState)
        {
            SceneManager.LoadScene(MAIN_MENU_SCENE);
        }
    }
}
