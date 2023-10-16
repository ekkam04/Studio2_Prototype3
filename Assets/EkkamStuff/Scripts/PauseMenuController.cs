using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    PlayerController playerController;
    public bool isPaused = false;
    [SerializeField] GameObject pauseMenuPanel;
    bool bgMusicPaused = false;
    bool endingMusicPaused = false;
    float initialTimeScale;

    void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        pauseMenuPanel.SetActive(false);
    }

    public void PauseButtonPressed()
    {
        isPaused = true;
        initialTimeScale = Time.timeScale;
        pauseMenuPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        // pause the music if playing
        if (playerController.backgroundMusic.isPlaying)
        {
            playerController.backgroundMusic.Pause();
            bgMusicPaused = true;
        }
        if (playerController.endingMusic.isPlaying)
        {
            playerController.endingMusic.Pause();
            endingMusicPaused = true;
        }
    }

    public void ResumeButtonPressed()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = initialTimeScale;

        // resume the music if paused
        if (bgMusicPaused)
        {
            playerController.backgroundMusic.Play();
            bgMusicPaused = false;
        }
        if (endingMusicPaused)
        {
            playerController.endingMusic.Play();
            endingMusicPaused = false;
        }
    }

    public void MainMenuButtonPressed()
    {
        isPaused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
