using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompleteState : GameStateBase
{
    [SerializeField] GameObject levelCompletePanel;
    [SerializeField] Button loadNextLevelButton; 
    [SerializeField] Button returnToMenuButton;

    void OnEnable()
    {
        loadNextLevelButton.onClick.AddListener(LoadNextLevel);
        returnToMenuButton.onClick.AddListener(ReturnToMenu);
    }

    void OnDisable()
    {
        loadNextLevelButton.onClick.RemoveListener(LoadNextLevel);
        returnToMenuButton.onClick.RemoveListener(ReturnToMenu);
    }

    public override void OnStateEnter()
    {
        levelCompletePanel.SetActive(true);
        if(GameController.CurrentLevel >= GameController.AllLevels.Count)
        {
            loadNextLevelButton.gameObject.SetActive(false);
        }
        else
        {
            loadNextLevelButton.gameObject.SetActive(true);
        }
    }

    public override void OnStateExit()
    {
        levelCompletePanel.SetActive(false);
    }

    public override void Tick()
    {
    }

    void LoadNextLevel()
    {
        GameController.Instance.GoToPreviousGameState();
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene(InputHolder.MENU_SCENE);
        //Application.Quit();
    }
}
