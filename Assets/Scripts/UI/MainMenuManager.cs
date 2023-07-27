using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] Button startGameButton;
    [SerializeField] Button continueGameButton;
    [SerializeField] Button quitButton;
    [SerializeField] AudioClip mainMenuSong;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;

    void OnEnable()
    {
        SetupButtons();
        AudioManager.Instance.StopAllMusicLoops();
        AudioManager.Instance.PlayLoopedAudioClip(
            mainMenuSong, OnlyPermitOne: true, IsMusic: true);

        musicVolumeSlider.value = PlayerPrefs.GetFloat(InputHolder.MUSIC_VOLUME);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat(InputHolder.SFX_VOLUME);
    }

    void SetupButtons()
    {
        startGameButton.onClick.AddListener(() =>
            StartGame(false)
        );
        quitButton.onClick.AddListener(QuitGame);
        if (PlayerPrefs.GetInt(InputHolder.LAST_LEVEL) > 0)
        {
            continueGameButton.gameObject.SetActive(true);
            continueGameButton.onClick.AddListener(() =>
                StartGame(true)
            );
        }
        else
        {
            continueGameButton.gameObject.SetActive(false);
        }
    }

    void StartGame(bool IsContinue)
    {
        if (IsContinue)
        {
            GameController.CurrentLevel = PlayerPrefs.GetInt(InputHolder.LAST_LEVEL);
        }
        else
        {
            GameController.CurrentLevel = 0;
        }
        SceneManager.LoadScene(InputHolder.GAME_SCENE);
    }

    public void SetSfxVolume(Slider ThisSlider)
    {
        PlayerPrefs.SetFloat(InputHolder.SFX_VOLUME, ThisSlider.value);
        AudioManager.Instance.ForceUpdateSourceVolumes();
    }

    public void SetMusicVolume(Slider ThisSlider) 
    {
        PlayerPrefs.SetFloat(InputHolder.MUSIC_VOLUME, ThisSlider.value);
        AudioManager.Instance.ForceUpdateSourceVolumes();
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
