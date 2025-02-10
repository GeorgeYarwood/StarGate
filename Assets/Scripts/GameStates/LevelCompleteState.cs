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

    public override void OnStateEnter()
    {
        loadNextLevelButton.onClick.AddListener(LoadNextLevel);
        returnToMenuButton.onClick.AddListener(ReturnToMenu);

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
        loadNextLevelButton.onClick.RemoveListener(LoadNextLevel);
        returnToMenuButton.onClick.RemoveListener(ReturnToMenu);
        levelCompletePanel.SetActive(false);
    }

    public override void Tick()
    {
    }

    void LoadNextLevel()
    {
        GameController.Instance.LoadCurrentLevel();
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene(InputHolder.MENU_SCENE);
        //Application.Quit();
    }
}
