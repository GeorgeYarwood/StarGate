using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseGameState : GameStateBase
{
    [SerializeField] GameObject pausePanel;

    [SerializeField] Button returnToGameButton;
    [SerializeField] Button returnToMenuButton;
    [SerializeField] Button quitButton;

    const string MENU_SCENE = "MainMenu";
    const string PAUSE_MENU = "PauseMenu";

    public override void OnStateEnter()
    {
        pausePanel.SetActive(true);
        AddListeners();
    }

    void AddListeners()
    {
        returnToGameButton.onClick.AddListener(ReturnToGame);
        quitButton.onClick.AddListener(QuitGame);
        returnToMenuButton.onClick.AddListener(ReturnToMenu);
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene(MENU_SCENE);
    }

    void QuitGame()
    {
        Application.Quit();
    }

    void RemoveListeners()
    {
        returnToGameButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
        returnToMenuButton.onClick.RemoveAllListeners();
    }

    public override void OnStateExit()
    {
        pausePanel.SetActive(false);
        RemoveListeners();
    }

    void ReturnToGame()
    {
        GameController.Instance.GoToPreviousGameState();
    }

    public override void Tick()
    {
        if (Input.GetButtonDown(PAUSE_MENU))
        {
            GameController.Instance.GoToPreviousGameState();
        }
    }
}
