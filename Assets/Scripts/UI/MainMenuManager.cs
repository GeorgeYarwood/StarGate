using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] Button startGameButton;
    [SerializeField] Button quitButton;

    const string GAME_SCENE = "GameScene";

    void OnEnable()
    {
        SetupButtons();
    }

    void SetupButtons()
    {
        startGameButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    void StartGame()
    {
        SceneManager.LoadScene(GAME_SCENE);
    }

    void QuitGame()
    {
        Application.Quit();
    }

    void OnDestroy()
    {
        startGameButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
    }
}
