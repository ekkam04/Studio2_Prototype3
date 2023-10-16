using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] AudioSource menuAudio;
    [SerializeField] AudioSource backgroundMusic;
    [SerializeField] AudioClip startSound;
    [SerializeField] ParticleSystem snowParticles;

    public void PlayButtonPressed()
    {
        backgroundMusic.Stop();
        menuAudio.PlayOneShot(startSound);
        StartCoroutine(LoadGame());
    }

    public void QuitButtonPressed()
    {
        StartCoroutine(QuitGame());
    }

    void Start()
    {
        mainMenuPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 1000f);
        mainMenuPanel.SetActive(true);
        LeanTween.moveLocalY(mainMenuPanel, 0f, 1.5f).setEaseOutQuad();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator LoadGame()
    {
        snowParticles.Stop();
        yield return new WaitForSeconds(0.1f);
        LeanTween.moveLocalY(mainMenuPanel, 1000f, 1.25f).setEaseOutQuad();
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene("EkkamScene");
    }

    IEnumerator QuitGame()
    {
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }
}
