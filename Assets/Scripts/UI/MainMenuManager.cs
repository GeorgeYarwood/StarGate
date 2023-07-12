using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] Button startGameButton;
    [SerializeField] Button quitButton;
    [SerializeField] AudioClip mainMenuSong;


    void OnEnable()
    {
        SetupButtons();
        AudioManager.Instance.PlayLoopedAudioClip(
            mainMenuSong, OnlyPermitOne: true, IsMusic: true);
    }

    void SetupButtons()
    {
        startGameButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    void StartGame()
    {
        SceneManager.LoadScene(InputHolder.GAME_SCENE);
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
